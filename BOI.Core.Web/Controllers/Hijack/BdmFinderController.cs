using BankOfIreland.Intermediaries.Core.Web.Models.ViewModels;
using BOI.Core.Search.Queries.Elastic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using BOI.Core.Extensions;
using BOI.Umbraco.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;
using System.Linq;
using Umbraco.Extensions;


namespace BankOfIreland.Intermediaries.Core.Web.Controllers.Hijacks
{
    public class BdmFinderController : RenderController
    {
        private readonly IConfiguration config;

        private readonly IBdmFinderSearcher bdmFinderSearcher;
        private readonly UmbracoHelper umbracoHelper;
        private readonly IPublishedValueFallback publishedValueFallback;

        public BdmFinderController(IConfiguration config, IBdmFinderSearcher bdmFinderSearcher,UmbracoHelper umbracoHelper, IPublishedValueFallback publishedValueFallback,ILogger<RenderController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor) : base(logger, compositeViewEngine, umbracoContextAccessor)
        {

            this.config = config;
            this.bdmFinderSearcher = bdmFinderSearcher;
            this.umbracoHelper = umbracoHelper;
            this.publishedValueFallback = publishedValueFallback;
        }

        public async Task<IActionResult> Index(BdmFinderSearch model)
        {

            if (model.Postcode.HasValue() || model.FCANumber.HasValue())
            {
                var isAjax = Request.IsAjaxRequest();
                if (model.Postcode.HasValue())
                {   //We've been breifed to use the full postcode- now using the "region" post code 16/4/2023
                    //model.Postcode = model.Postcode.PostcodeOutCode();
                    model.Postcode = model.Postcode.Replace(" ", "");
                }



                model.Size = 1000;
                var results = bdmFinderSearcher.Execute(model);
                var vm = new BdmFinderViewModel(CurrentPage, publishedValueFallback);
                var bdmContacts = Enumerable.Empty<BDmcontact>();
                if (results.Total != 0)
                {
                    
                    var searchOutcode = model.Postcode.PostcodeOutCode().ToUpperInvariant();
                    var bdms = results.QueryResults.Select(x => umbracoHelper.Content(x.ItemId) as BDmcontact).RemoveNulls().Where(x => x.BDMType == BDMType.BDM);
                    if (bdms.NotNullAndAny() && bdms.Count() > 1)
                    {
                        //more than one, filter by out code
                        var postCodeBDMs = bdms.Where(x => x.FcaCodes.Contains(model.FCANumber)
                            && (x.PostCodeOutcodes.Contains(searchOutcode))

                            );
                        vm.BDM = postCodeBDMs.FirstOrDefault() ?? bdms.FirstOrDefault();

                    }
                    else if (bdms.Count() == 1)//Make sure its not a must match, that doesnt on outcode
                    {
                        
                        vm.BDM = bdms.FirstOrDefault();



                    }


                    var tbdms = results.QueryResults.Select(x => umbracoHelper.Content(x.ItemId) as BDmcontact).RemoveNulls().Where(x => x.BDMType == BDMType.TBDM);
                    if (tbdms.NotNullAndAny() && tbdms.Count() > 1)
                    {
                        //more than one, filter by out code
                        var postcodeTbdms = tbdms.Where(x => x.FcaCodes.Contains(model.FCANumber)
                            && (x.PostCodeOutcodes.Contains(searchOutcode))

                            );
                        vm.TBDM = postcodeTbdms.FirstOrDefault() ?? tbdms.FirstOrDefault();

                    }
                    else if (tbdms.Count() == 1)//Make sure its not a must match, that doesnt on outcode
                    {
                        var thisTBdm = tbdms.FirstOrDefault();
                        vm.TBDM = thisTBdm;


                    }
                    //At this point we will have either a macthed BDM,TBDM or the query may have matched against an IEL
                    var iels = results.QueryResults.Select(x => umbracoHelper.Content(x.ItemId) as BDmcontact).RemoveNulls().Where(x => x.BDMType == BDMType.IEL);

                    await TryUpdateModelAsync(results);
                }


                return isAjax ? PartialView("~/Views/Partials/BdmFinder/BdmListing.cshtml", vm) : CurrentTemplate(vm);
            }
            else
            {
                return CurrentTemplate(new BdmFinderViewModel(CurrentPage, publishedValueFallback) { ListingUrl = CurrentPage.Url(mode: UrlMode.Relative), FcaNumber = model.FCANumber, Postcode = model.Postcode });
            }
            

        }
    }
}
