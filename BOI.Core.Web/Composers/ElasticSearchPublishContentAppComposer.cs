using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace BOI.Core.Web.Composers
{
    public class ElasticSearchPublishContentAppComponent : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            // Add our word counter content app into the composition aka into the DI
            builder.ContentApps().Append<ElasticSearchPublishContentApp>();
        }
    }


    public class ElasticSearchPublishContentApp : IContentAppFactory
    {
        public ContentApp GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            // Can implement some logic with userGroups if needed
            // Allowing us to display the content app with some restrictions for certain groups
            //if (userGroups.All(x => x.Alias.ToLowerInvariant() != Umbraco.Core.Constants.Security.AdminGroupAlias))
            //    return null;


            // only show app on content items
            if (source is IContent)
            {
                var publishApp = new ContentApp
                {
                    Alias = "esPublish",
                    Name = "Indexing Publish",
                    Icon = "icon-poll",
                    View = "/App_Plugins/ElasticSearchPublishContentApp/view.html",
                    Weight = 0
                };

                return publishApp;
            }

            return null;
        }
    }
}
