// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExportBuyersPipeline.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Ajsuth.Sample.OrderCloud.Engine.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Ajsuth.Sample.OrderCloud.Engine.Pipelines
{
    /// <summary>Defines the ExportBuyers pipeline interface</summary>
    /// <seealso cref="IPipeline{TInput, TOutput, TContext}" />
    [PipelineDisplayName(OrderCloudConstants.Pipelines.ExportBuyers)]
    public interface IExportBuyersPipeline : IPipeline<ExportEntitiesArgument, string, CommercePipelineExecutionContext>
    {
    }
}
