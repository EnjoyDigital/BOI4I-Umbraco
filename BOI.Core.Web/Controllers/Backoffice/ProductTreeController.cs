using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;

namespace BOI.Core.Web.Controllers.Backoffice
{
    [PluginController("Product")]
    [Tree(ProductSection, "Product", TreeTitle = "Product", TreeGroup = "productGroup", SortOrder = 5)]
    public class ProductTreeController : TreeController
    {
        public const string ProductSection = "product";

        public ProductTreeController(ILocalizedTextService localizedTextService, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, IEventAggregator eventAggregator) : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
        }

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
        {
            return new TreeNodeCollection
            {
                CreateTreeNode("0", string.Empty, queryStrings, "Upload Product file", "icon-cloud-upload", false, MenuRoutePath("import", queryStrings, "0")),
                CreateTreeNode("1", string.Empty, queryStrings, "Export Products", "icon-download", false, MenuRoutePath("exports", queryStrings, "1"))
            };
        }

        private static string MenuRoutePath(string viewName, FormCollection queryStrings, string id)
        {
            return string.Concat(queryStrings.GetValue<string>("application"), "Product".EnsureStartsWith('/'), "/", viewName, "/", id);
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        {
            throw new NotImplementedException();
        }
    }
}
