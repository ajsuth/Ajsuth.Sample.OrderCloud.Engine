// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureOpsServiceApiBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Ajsuth.Sample.OrderCloud.Engine.Models;
using Ajsuth.Sample.OrderCloud.Engine.Pipelines.Arguments;
using Ajsuth.Sample.OrderCloud.Engine.Policies;
using Microsoft.AspNet.OData.Builder;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Ajsuth.Sample.OrderCloud.Engine.Pipelines.Blocks
{
    /// <summary>Defines the configure service api block, which configures the OData model for api endpoints</summary>
    /// <seealso cref="SyncPipelineBlock{TInput, TOutput, TContext}" />
    [PipelineDisplayName(OrderCloudConstants.Pipelines.Blocks.ConfigureOpsServiceApi)]
    public class ConfigureOpsServiceApiBlock : SyncPipelineBlock<ODataConventionModelBuilder, ODataConventionModelBuilder, CommercePipelineExecutionContext>
    {
        /// <summary>Maps defined Commerce Engine data types and endpoints to the OData EDM for the '/commerceops' routes.</summary>
        /// <param name="modelBuilder">The <see cref="ODataConventionModelBuilder"/>.</param>
        /// <param name="context">The context.</param>
        /// <returns>The <see cref="ODataConventionModelBuilder"/>.</returns>
        public override ODataConventionModelBuilder Run(ODataConventionModelBuilder modelBuilder, CommercePipelineExecutionContext context)
        {
            Condition.Requires(modelBuilder).IsNotNull($"{this.Name}: The argument cannot be null.");

            // Add the entities

            // Add the entity sets

            // Add complex types

            // Add unbound functions

            // Add unbound actions
            var exportCatalogsAction = modelBuilder.Action("ExportToOrderCloud");
            exportCatalogsAction.Parameter<string>("importType");
            exportCatalogsAction.Parameter<ExportSettings>("processSettings");
            exportCatalogsAction.CollectionParameter<SitePolicy>("siteSettings");
            exportCatalogsAction.Parameter<UserPolicy>("userSettings");
            exportCatalogsAction.Parameter<SellableItemExportPolicy>("productSettings");
            exportCatalogsAction.Returns<ExportResult>();

            return modelBuilder;
        }
    }
}