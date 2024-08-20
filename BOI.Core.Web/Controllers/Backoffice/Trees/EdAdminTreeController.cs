using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.ModelBinders;
using Umbraco.Extensions;

namespace BOI.Core.Web.Controllers.Backoffice.Trees
{
    [Tree(SectionName, SectionName, TreeTitle = "Misc", TreeGroup = SectionName,  SortOrder = 99)]
    [PluginController("EdAdmin")]
    public class EdAdminController : TreeController
    {
        public const string SectionName = "EdAdmin";
        private readonly IMenuItemCollectionFactory menuItemCollectionFactory;

        public EdAdminController(ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IEventAggregator eventAggregator, IMenuItemCollectionFactory menuItemCollectionFactory)
            : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
            this.menuItemCollectionFactory = menuItemCollectionFactory;
        }

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings)
        {
            var nodes = new List<(string, string, string)>
            {
                ("Hreflang", "icon-trafic", "hreflang"),
                ("Meta data", "icon-autofill", "metaData"),
               // ("Member export", "icon-users", "memberExport"),
                ("Redirect Import", "icon-reply-arrow", "redirectImport"),
            }
            .Select((t, i)
                => CreateTreeNode(i.ToString(), string.Empty, queryStrings, t.Item1, t.Item2, false, MenuRoutePath(t.Item3, queryStrings, i.ToString()))
            );


            return new TreeNodeCollection(nodes);
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings)
        {
            return menuItemCollectionFactory.Create();
        }

        private static string MenuRoutePath(string viewName, FormCollection queryStrings, string id)
        {
            return string.Concat(queryStrings.GetValue<string>("application"), SectionName.EnsureStartsWith('/'), "/", viewName, "/", id);
        }
    }
}