using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BOI.Core.Web.Controllers.Backoffice
{
    [PluginController("SolicitorUpload")]
    [Tree(SolicitorUploadSection, "SolicitorUpload", TreeTitle = "Solicitor Upload", TreeGroup = "solicitorUploadGroup", SortOrder = 5)]
    public class SolicitorUploadTreeController : TreeController
    {
        public const string SolicitorUploadSection = "solicitorUpload";

        public SolicitorUploadTreeController(ILocalizedTextService localizedTextService, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, IEventAggregator eventAggregator) : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
        }

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
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

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        {
            throw new NotImplementedException();
        }
    }
}
