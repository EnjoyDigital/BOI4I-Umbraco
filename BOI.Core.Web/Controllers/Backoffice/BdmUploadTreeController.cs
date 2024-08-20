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
    [PluginController("BdmUpload")]
    [Tree(BDMSection, "BdmUpload", TreeTitle = "BdmUpload", TreeGroup = "bdmGroup", SortOrder = 6)]
    public class BdmUploadTreeController : TreeController
    {
        public const string BDMSection = "BdmUpload"; 

        public BdmUploadTreeController(ILocalizedTextService localizedTextService, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, IEventAggregator eventAggregator) : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
        }

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
        {
            return new TreeNodeCollection
            {
                CreateTreeNode("0", string.Empty, queryStrings, "Upload BDM file", "icon-cloud-upload", false, MenuRoutePath("import", queryStrings, "0")),
            };
        }

        private static string MenuRoutePath(string viewName, FormCollection queryStrings, string id)
        {
            return string.Concat(queryStrings.GetValue<string>("application"), BDMSection.EnsureStartsWith('/'), "/", viewName, "/", id);
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        {
            throw new NotImplementedException();
        }
    }
}
