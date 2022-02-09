// <copyright file="CustomerExtensions.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Customers;
using System;
using System.Linq;

namespace Ajsuth.Sample.OrderCloud.Engine.FrameworkExtensions
{
    /// <summary>
    /// Defines extensions for <see cref="Customer"/>
    /// </summary>
    public static class CustomerExtensions
    {
        /// <summary>
        /// Retrieves the 'Details' entity view from the customer commerce entity.
        /// </summary>
        /// <param name="Customer">The <see cref="Customer"/>.</param>
        /// <returns>The 'Details' <see cref="EntityView"/>.</returns>
        public static EntityView GetCustomerDetailsEntityView(this Customer customer)
        {
            if (customer == null || !customer.HasComponent<CustomerDetailsComponent>())
            {
                return null;
            }

            return customer.GetComponent<CustomerDetailsComponent>().View?.ChildViews?.FirstOrDefault(v => v.Name.Equals("Details", StringComparison.OrdinalIgnoreCase)) as EntityView;;
        }

    }
}
