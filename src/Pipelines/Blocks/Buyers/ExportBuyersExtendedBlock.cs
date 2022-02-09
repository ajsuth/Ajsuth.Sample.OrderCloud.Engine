// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExportBuyersExtendedBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Ajsuth.Sample.OrderCloud.Engine.Models;
using Ajsuth.Sample.OrderCloud.Engine.Pipelines.Arguments;
using Microsoft.Extensions.Logging;
using OrderCloud.SDK;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Shops;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ajsuth.Sample.OrderCloud.Engine.Pipelines.Blocks
{
    /// <summary>Defines the asynchronous executing ExportBuyersExtended pipeline block</summary>
    /// <seealso cref="AsyncPipelineBlock{TInput, TOutput, TContext}" />
    [PipelineDisplayName(OrderCloudConstants.Pipelines.Blocks.ExportBuyersExtended)]
    public class ExportBuyersExtendedBlock : AsyncPipelineBlock<ExportToOrderCloudArgument, ExportToOrderCloudArgument, CommercePipelineExecutionContext>
    {
        /// <summary>Gets or sets the commander.</summary>
        /// <value>The commander.</value>
        protected CommerceCommander Commander { get; set; }

        /// <summary>Initializes a new instance of the <see cref="ExportBuyersExtendedBlock" /> class.</summary>
        /// <param name="commander">The commerce commander.</param>
        public ExportBuyersExtendedBlock(CommerceCommander commander)
        {
            this.Commander = commander;
        }

        /// <summary>Executes the pipeline block's code logic.</summary>
        /// <param name="arg">The pipeline argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>The <see cref="ExportToOrderCloudArgument"/>.</returns>
        public override async Task<ExportToOrderCloudArgument> RunAsync(ExportToOrderCloudArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{Name}: The argument can not be null");

            if (!arg.ProcessSettings.ProcessSites)
            {
                context.Logger.LogInformation($"Skipping site export - not enabled.");
                return arg;
            }

            long itemsProcessed = 0;

            var siteSettings = arg.SiteSettings;

            context.Logger.LogInformation($"{Name}-Reviewing storefronts in Site Settings|Count:{siteSettings.Count}|Environment:{context.CommerceContext.Environment.Name}");

            if (siteSettings.Count == 0)
            {
                return arg;
            }

            itemsProcessed += siteSettings.Count;

            var storefronts = siteSettings.Select(s => s.Storefront).Distinct();

            foreach (var storefront in storefronts)
            {
                var error = false;

                var newContext = new CommercePipelineExecutionContextOptions(new CommerceContext(context.CommerceContext.Logger, context.CommerceContext.TelemetryClient)
                {
                    Environment = context.CommerceContext.Environment,
                    Headers = context.CommerceContext.Headers,
                },
                onError: x => error = true,
                onAbort: x =>
                {
                    if (!x.Contains("Ok|", StringComparison.OrdinalIgnoreCase))
                    {
                        error = true;
                    }
                });

                newContext.CommerceContext.AddObject(context.CommerceContext.GetObject<OrderCloudClient>());
                newContext.CommerceContext.AddObject(context.CommerceContext.GetObject<ExportResult>());
                var shops = context.CommerceContext.GetEntities<Shop>();
                foreach (var shop in shops)
                {
                    newContext.CommerceContext.AddUniqueEntity(shop);
                }

                context.Logger.LogDebug($"{Name}-Exporting buyer: '{storefront}'. Environment: {context.CommerceContext.Environment.Name}");
                await Commander.Pipeline<ExportBuyersExtendedPipeline>()
                    .RunAsync(
                        new ExportEntitiesArgument(storefront, arg),
                        newContext)
                    .ConfigureAwait(false);

                context.CommerceContext.AddUniqueObjectByType(newContext.CommerceContext.GetObject<ExportResult>());

                if (error)
                {
                    context.Abort(
                        await context.CommerceContext.AddMessage(
                            context.GetPolicy<KnownResultCodes>().Error,
                            OrderCloudConstants.Errors.ExportBuyersExtendedFailed,
                            new object[] { Name },
                            $"{Name}: Processing Storefronts failed.").ConfigureAwait(false),
                        context);
                }
            }

            context.Logger.LogInformation($"{Name}-Processing Storefronts Completed: {(int)itemsProcessed}. Environment: {context.CommerceContext.Environment.Name}");
            return arg;
        }
    }
}