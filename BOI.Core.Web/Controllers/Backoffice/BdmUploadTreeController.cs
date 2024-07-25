using System.Net.Http.Formatting;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;

namespace BankOfIreland.Intermediaries.Core.Web.Controllers.Backoffice
{
    [PluginController("BdmUpload")]
    [Tree(BdmUploadSection, "BdmUpload", TreeTitle = "Bdm Upload", TreeGroup = "bdmUploadGroup", SortOrder = 6)]
    public class BdmUploadTreeController : TreeController
    {
        public const string BdmUploadSection = "bdmUpload";
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var tree = new TreeNodeCollection();
            var import = CreateTreeNode("1", "-1", queryStrings, "Upload Bdm file");

            import.Icon = "icon-cloud-upload";
            // set to false for a custom tree with a single node.
            import.HasChildren = false;
            //url for menu
            import.MenuUrl = null;
            import.RoutePath = string.Format("{0}/{1}/{2}", BdmUploadSection, "bdmUpload", "import");

            tree.Add(import);

            return tree;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            return new MenuItemCollection();
        }
    }
}
