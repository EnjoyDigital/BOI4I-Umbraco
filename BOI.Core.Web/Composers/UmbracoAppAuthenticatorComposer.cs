using BOI.Core.Web.Models.TwoFactorAuth;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Extensions;

namespace BOI.Core.Web.Composers
{
    public class UmbracoAppAuthenticatorComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            var identityBuilder = new BackOfficeIdentityBuilder(builder.Services);

            identityBuilder.AddTwoFactorProvider<UmbracoUserAppAuthenticator>(UmbracoUserAppAuthenticator.Name);

            builder.Services.Configure<TwoFactorLoginViewOptions>(UmbracoUserAppAuthenticator.Name, options =>
            {
                options.SetupViewPath = "..\\App_Plugins\\TwoFactorProviders\\twoFactorProviderGoogleAuthenticator.html";
            });
        }
    }
}
