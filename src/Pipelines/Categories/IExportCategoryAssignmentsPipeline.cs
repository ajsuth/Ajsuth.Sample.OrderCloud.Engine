// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExportCategoryAssignmentsPipeline.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Ajsuth.Sample.OrderCloud.Engine.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Pipelines;

namespace Ajsuth.Sample.OrderCloud.Engine.Pipelines
{
    /// <summary>Defines the ExportCategoryAssignments pipeline interface</summary>
    /// <seealso cref="IPipeline{TInput, TOutput, TContext}" />
    [PipelineDisplayName(OrderCloudConstants.Pipelines.ExportCategoryAssignments)]
    public interface IExportCategoryAssignmentsPipeline : IPipeline<ExportEntitiesArgument, Category, CommercePipelineExecutionContext>
    {
    }
}
