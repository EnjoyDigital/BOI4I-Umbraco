using System.Net.Http.Formatting;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;

namespace BankOfIreland.Intermediaries.Core.Web.Controllers.Backoffice
{
    [PluginController("SolicitorUpload")]
    [Tree(SolicitorUploadSection, "SolicitorUpload", TreeTitle = "Solicitor Upload", TreeGroup = "solicitorUploadGroup", SortOrder = 5)]
    public class SolicitorUploadTreeController : TreeController
    {
        public const string SolicitorUploadSection = "solicitorUpload";
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var tree = new TreeNodeCollection();
            var import = CreateTreeNode("1", "-1", queryStrings, "Upload Solicitor file");

            import.Icon = "icon-cloud-upload";
            // set to false for a custom tree with a single node.
            import.HasChildren = false;
            //url for menu
            import.MenuUrl = null;
            import.RoutePath  = string.Format("{0}/{1}/{2}", SolicitorUploadSection, "solicitorUpload", "import");

            tree.Add(import);
           
            return tree;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            return new MenuItemCollection();
        }
    }
}
