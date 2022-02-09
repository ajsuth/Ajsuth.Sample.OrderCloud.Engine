// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidateStorefrontBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Ajsuth.Sample.OrderCloud.Engine.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System.Threading.Tasks;
using Sitecore.Commerce.Plugin.Shops;
using System.Linq;

namespace Ajsuth.Sample.OrderCloud.Engine.Pipelines.Blocks
{
    /// <summary>Defines the asynchronous executing ValidateStorefront pipeline block</summary>
    /// <seealso cref="AsyncPipelineBlock{TInput, TOutput, TContext}" />
    [PipelineDisplayName(OrderCloudConstants.Pipelines.Blocks.ValidateStorefront)]
    public class ValidateStorefrontBlock : AsyncPipelineBlock<ExportEntitiesArgument, Shop, CommercePipelineExecutionContext>
    {
        /// <summary>Gets or sets the commander.</summary>
        /// <value>The commander.</value>
        protected CommerceCommander Commander { get; set; }

        /// <summary>Initializes a new instance of the <see cref="ValidateStorefrontBlock" /> class.</summary>
        /// <param name="commander">The commerce commander.</param>
        public ValidateStorefrontBlock(CommerceCommander commander)
        {
            this.Commander = commander;
        }

        /// <summary>Executes the pipeline block's code logic.</summary>
        /// <param name="arg">The pipeline argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>The <see cref="Shop"/>.</returns>
        public override async Task<Shop> RunAsync(ExportEntitiesArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{Name}: The argument cannot be null.");
            Condition.Requires(arg.EntityId).IsNotNull($"{Name}: The storefront name cannot be null.");

            context.CommerceContext.AddUniqueObjectByType(arg);

            var storefrontName = arg.EntityId;

            context.Logger.LogDebug($"{Name}: Validating storefront '{storefrontName}'");

            var storefront = context.CommerceContext.GetEntity<Shop>(shop => shop.Name.EqualsOrdinalIgnoreCase(storefrontName));
            if (storefront == null)
            {
                context.Abort(
                    await context.CommerceContext.AddMessage(
                        context.GetPolicy<KnownResultCodes>().Error,
                        OrderCloudConstants.Errors.StorefrontNotFound,
                        new object[]
                        {
                            Name,
                            storefrontName
                        },
                        $"{Name}: Storefront '{storefrontName}' not found.").ConfigureAwait(false),
                    context);

                return null;
            }

            var siteSettings = arg.SiteSettings.FirstOrDefault(site => site.Storefront.EqualsOrdinalIgnoreCase(storefrontName));
            if (string.IsNullOrWhiteSpace(siteSettings.Domain))
            {
                context.Abort(
                    await context.CommerceContext.AddMessage(
                        context.GetPolicy<KnownResultCodes>().Error,
                        "DomainNotProvided",
                        new object[]
                        {
                            Name,
                            storefrontName
                        },
                        $"{Name}: Site Settings for storefront '{storefrontName}' missing domain.").ConfigureAwait(false),
                    context);

                return null;
            }

            return storefront;
        }
    }
}