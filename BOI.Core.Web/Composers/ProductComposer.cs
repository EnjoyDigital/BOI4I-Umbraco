using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Sections;

namespace BOI.Core.Web.Composers
{
    public class ProductComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Sections().Append<ProductSection>();
        }
    }

    public class ProductSection : ISection
    {
        public string Alias => "products";

        public string Name => "Products";
    }
}
