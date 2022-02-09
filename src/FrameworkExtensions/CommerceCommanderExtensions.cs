// <copyright file="CommerceCommanderExtensions.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using System.Threading.Tasks;

namespace Ajsuth.Sample.OrderCloud.Engine.FrameworkExtensions
{
    /// <summary>
    /// Defines extensions for <see cref="CommerceCommander"/>
    /// </summary>
    public static class CommerceCommanderExtensions
    {
        public static async Task<FindEntitiesInListArgument> GetCatalogToSellableItemReferences(this CommerceCommander commander, CommercePipelineExecutionContext context, string catalogId)
        {
            return await commander.Pipeline<IFindEntitiesInListPipeline>()
                 .RunAsync(new FindEntitiesInListArgument(typeof(Catalog),
                         $"{CatalogConstants.CatalogToSellableItem}-{catalogId.SimplifyEntityName()}",
                         0,
                         int.MaxValue, false, true),
                     context.CommerceContext.PipelineContextOptions)
                 .ConfigureAwait(false);
        }

        public static async Task<FindEntitiesInListArgument> GetCategoryToSellableItemReferences(this CommerceCommander commander, CommercePipelineExecutionContext context, string categoryId)
        {
            return await commander.Pipeline<IFindEntitiesInListPipeline>()
                 .RunAsync(new FindEntitiesInListArgument(typeof(Category),
                         $"{CatalogConstants.CategoryToSellableItem}-{categoryId.SimplifyEntityName()}",
                         0,
                         int.MaxValue, false, true),
                     context.CommerceContext.PipelineContextOptions)
                 .ConfigureAwait(false);
        }

        public static async Task<FindEntitiesInListArgument> GetListIds<T>(this CommerceCommander commander, CommercePipelineExecutionContext context, string listName, int take = int.MaxValue, int skip = 0)
        {
            var arg = new FindEntitiesInListArgument(typeof(T), listName, skip, take)
            {
                LoadEntities = false,
                LoadTotalItemCount = true
            };
            var result = await commander.Pipeline<FindEntitiesInListPipeline>().RunAsync(arg, context.CommerceContext.PipelineContextOptions).ConfigureAwait(false);

            return result;
        }
    }
}
