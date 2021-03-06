// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExportCustomerBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Ajsuth.Sample.OrderCloud.Engine.FrameworkExtensions;
using Ajsuth.Sample.OrderCloud.Engine.Models;
using Ajsuth.Sample.OrderCloud.Engine.Pipelines.Arguments;
using Ajsuth.Sample.OrderCloud.Engine.Policies;
using Microsoft.Extensions.Logging;
using OrderCloud.SDK;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Ajsuth.Sample.OrderCloud.Engine.Pipelines.Blocks
{
    /// <summary>Defines the asynchronous executing ExportCustomer pipeline block</summary>
    /// <seealso cref="AsyncPipelineBlock{TInput, TOutput, TContext}" />
    [PipelineDisplayName(OrderCloudConstants.Pipelines.Blocks.ExportCustomer)]
    public class ExportCustomerBlock : AsyncPipelineBlock<Customer, Customer, CommercePipelineExecutionContext>
    {
        /// <summary>The commerce commander.</summary>
        protected CommerceCommander Commander { get; set; }

        /// <summary>The OrderCloud client.</summary>
        protected OrderCloudClient Client { get; set; }

        /// <summary>The export result model.</summary>
        protected ExportResult Result { get; set; }

        /// <summary>Initializes a new instance of the <see cref="ExportCustomerBlock" /> class.</summary>
        /// <param name="commander">The commerce commander.</param>
        public ExportCustomerBlock(CommerceCommander commander)
        {
            this.Commander = commander;
        }

        /// <summary>Executes the pipeline block's code logic.</summary>
        /// <param name="arg">The pipeline argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>The <see cref="Customer"/>.</returns>
        public override async Task<Customer> RunAsync(Customer customer, CommercePipelineExecutionContext context)
        {
            Condition.Requires(customer).IsNotNull($"{Name}: The customer can not be null");

            Client = context.CommerceContext.GetObject<OrderCloudClient>();
            Result = context.CommerceContext.GetObject<ExportResult>();

            var exportSettings = context.CommerceContext.GetObject<ExportEntitiesArgument>();
            var siteSettings = exportSettings.SiteSettings.FirstOrDefault(site => site.Domain.EqualsOrdinalIgnoreCase(customer.Domain));

            var buyerId = customer.Domain.ToValidOrderCloudId();

            var user = await CreateOrUpdateBuyerUser(context, customer, buyerId);
            if (user == null)
            {
                return null;
            }

            var addresses = await CreateOrUpdateBuyerAddresses(context, buyerId, customer);
            if (addresses.Any())
            {
                await CreateOrUpdateBuyerAddressAssignments(context, buyerId, user, addresses);
            }

            if (!string.IsNullOrWhiteSpace(siteSettings.DefaultUserGroup))
            {
                await CreateOrUpdateBuyerUserGroupAssignment(context, buyerId, user, siteSettings.DefaultUserGroup);
            }

            return customer;
        }

        /// <summary>
        /// Creates or updates the buyer user.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="customer">The XC customer.</param>
        /// <param name="buyerId">The OC buyer identifier.</param>
        /// <returns>The <see cref="User"/>.</returns>
        protected async Task<User> CreateOrUpdateBuyerUser(CommercePipelineExecutionContext context, Customer customer, string buyerId)
        {
            try
            {
                var userPolicy = context.GetPolicy<UserPolicy>();
                var user = new User
                {
                    ID = customer.FriendlyId,
                    Username = customer.LoginName,
                    FirstName = !string.IsNullOrWhiteSpace(customer.FirstName) ? customer.FirstName : userPolicy.DefaultFirstName,
                    LastName = !string.IsNullOrWhiteSpace(customer.LastName) ? customer.LastName : userPolicy.DefaultLastName,
                    Email = customer.Email,
                    Active = customer.AccountStatus == context.GetPolicy<KnownCustomersStatusesPolicy>().ActiveAccount,
                    Phone = customer.GetCustomerDetailsEntityView()?.GetPropertyValue("PhoneNumber")?.ToString()
                };

                Result.BuyerUsers.ItemsProcessed++;

                context.Logger.LogInformation($"Saving buyer user; Buyer User ID: {user.ID}, Username: {customer.LoginName}");
                user = await Client.Users.SaveAsync(buyerId, user.ID, user);
                Result.BuyerUsers.ItemsUpdated++;

                return user;
            }
            catch (Exception ex)
            {
                Result.BuyerUsers.ItemsErrored++;
                context.Abort(
                    await context.CommerceContext.AddMessage(
                        context.GetPolicy<KnownResultCodes>().Error,
                        OrderCloudConstants.Errors.UpdateBuyerUserFailed,
                        new object[]
                        {
                            Name,
                            customer.Id,
                            ex.Message,
                            ex
                        },
                        $"{Name}: Ok| Exporting customer '{customer.Id}' - username '{customer.LoginName}' failed.\n{ex.Message}\n{ex}").ConfigureAwait(false),
                    context);

                return null;
            }
        }

        /// <summary>
        /// Gets or creates an buyer address to represent a buyer user's address.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="buyerId">The buyer identifier.</param>
        /// <param name="customer">The XC customer.</param>
        /// <param name="user">The OC user.</param>
        /// <returns>The list OC <see cref="Address"/> representing buyer user addresses.</returns>
        protected async Task<List<Address>> CreateOrUpdateBuyerAddresses(CommercePipelineExecutionContext context, string buyerId, Customer customer)
        {
            var addresses = new List<Address>();

            var addressComponents = customer.EntityComponents.OfType<AddressComponent>();

            foreach (var addressComponent in addressComponents)
            {
                Result.BuyerAddresses.ItemsProcessed++;

                var party = addressComponent.Party;
                var addressId = $"{customer.FriendlyId}_{party.AddressName}".ToValidOrderCloudId();
                try
                {
                    var address = new Address
                    {
                        ID = addressId,
                        FirstName = party.FirstName,
                        LastName = party.LastName,
                        Street1 = party.Address1,
                        Street2 = party.Address2,
                        City = party.City,
                        State = party.State,
                        Zip = party.ZipPostalCode,
                        Country = party.CountryCode,
                        Phone = party.PhoneNumber,
                        AddressName = party.AddressName
                    };
                    address.xp.IsPrimary = party.IsPrimary;

                    context.Logger.LogInformation($"Saving buyer address; Address ID: {addressId}");
                    address = await Client.Addresses.SaveAsync(buyerId, addressId, address);
                    Result.BuyerAddresses.ItemsCreated++;

                    addresses.Add(address);
                }
                catch (Exception e)
                {
                    Result.BuyerAddresses.ItemsErrored++;
                    context.Logger.LogError($"{Name}: Create buyer address '{addressId}' failed.\n{e.Message}\n{e}");
                }
            }

            return addresses;
        }

        /// <summary>
        /// Creates or updates the buyer address assignment.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="buyerId">The OC buyer identifier.</param>
        /// <param name="user">The OC user.</param>
        /// <param name="addresses">The addresses to assign to the buyer user.</param>
        /// <returns></returns>
        protected async Task CreateOrUpdateBuyerAddressAssignments(CommercePipelineExecutionContext context, string buyerId, User user, List<Address> addresses)
        {
            foreach (var address in addresses)
            {
                try
                {
                    var buyerAddressAssignment = new AddressAssignment
                    {
                        AddressID = address.ID,
                        UserID = user.ID,
                        IsShipping = true,
                        IsBilling = true
                    };

                    Result.BuyerAddressAssignments.ItemsProcessed++;

                    context.Logger.LogInformation($"Saving buyer user group assignment; Address ID: {address.ID}, User ID: {user.ID}");
                    await Client.Addresses.SaveAssignmentAsync(buyerId, buyerAddressAssignment);
                    Result.BuyerAddressAssignments.ItemsCreated++;
                }
                catch (Exception e)
                {
                    Result.BuyerAddressAssignments.ItemsErrored++;
                    context.Logger.LogError($"{Name}: Saving buyer user group assignment failed; Address ID: {address.ID}, User ID: {user.ID}.\n{e.Message}\n{e}");
                }
            }
        }

        /// <summary>
        /// Creates or updates the buyer user group assignment.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="buyerId">The OC buyer identifier.</param>
        /// <param name="user">The OC user.</param>
        /// <param name="userGroupId">The OC user group.</param>
        /// <returns></returns>
        protected async Task CreateOrUpdateBuyerUserGroupAssignment(CommercePipelineExecutionContext context, string buyerId, User user, string userGroupId)
        {
            
            try
            {
                var userGroupAssignment = new UserGroupAssignment
                {
                    UserGroupID = userGroupId,
                    UserID = user.ID
                };

                Result.BuyerGroupAssignments.ItemsProcessed++;

                context.Logger.LogInformation($"Saving buyer user group assignment; User Group ID: {userGroupId}, User ID: {user.ID}");
                await Client.UserGroups.SaveUserAssignmentAsync(buyerId, userGroupAssignment);
                Result.BuyerGroupAssignments.ItemsCreated++;
            }
            catch (Exception e)
            {
                Result.BuyerGroupAssignments.ItemsErrored++;
                context.Logger.LogError($"{Name}: Saving buyer user group assignment failed; User Group ID: {userGroupId}, User ID: {user.ID}.\n{e.Message}\n{e}");
            }
        }

    }
}