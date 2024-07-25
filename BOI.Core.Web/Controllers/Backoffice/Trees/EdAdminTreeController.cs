using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;

namespace BankOfIreland.Intermediaries.Core.Web.Controllers.Backoffice.Trees
{
    [Tree("EdAdmin", "EdAdmin", TreeTitle = "Import/Export")]
    [PluginController("EdAdmin")]
    public class EdAdminController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            return new TreeNodeCollection
            {
                CreateTreeNode("0", string.Empty, queryStrings, "Hreflang", "icon-trafic", false, MenuRoutePath("hreflang", queryStrings, "0")),
                CreateTreeNode("1", string.Empty, queryStrings, "Meta data", "icon-autofill", false, MenuRoutePath("metaData", queryStrings, "1")),
                CreateTreeNode("2", string.Empty, queryStrings, "Member export", "icon-download", false, MenuRoutePath("memberExport", queryStrings, "2"))
            };
        }

        private static string MenuRoutePath(string viewName, FormDataCollection queryStrings, string id)
        {
            return string.Concat(queryStrings.GetValue<string>("application"), "EdAdmin".EnsureStartsWith('/'), "/", viewName, "/", id);
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            return new MenuItemCollection();
        }
    }
}