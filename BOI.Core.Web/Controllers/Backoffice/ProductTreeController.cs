using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;

namespace BankOfIreland.Intermediaries.Core.Web.Controllers.Backoffice
{
    [PluginController("Product")]
    [Tree(ProductSection, "Product", TreeTitle = "Product", TreeGroup = "productGroup", SortOrder = 5)]
    public class ProductTreeController : TreeController
    {
        public const string ProductSection = "product";
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            return new TreeNodeCollection
            {
                CreateTreeNode("0", string.Empty, queryStrings, "Upload Product file", "icon-cloud-upload", false, MenuRoutePath("import", queryStrings, "0")),
                CreateTreeNode("1", string.Empty, queryStrings, "Export Products", "icon-download", false, MenuRoutePath("exports", queryStrings, "1"))
            };
        }

        private static string MenuRoutePath(string viewName, FormDataCollection queryStrings, string id)
        {
            return string.Concat(queryStrings.GetValue<string>("application"), "Product".EnsureStartsWith('/'), "/", viewName, "/", id);
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            return new MenuItemCollection();
        }
    }
}
