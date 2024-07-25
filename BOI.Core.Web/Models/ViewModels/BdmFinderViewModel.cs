using BOI.Umbraco.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
namespace BankOfIreland.Intermediaries.Core.Web.Models.ViewModels
{
    public class BdmFinderViewModel : BDmfinder
    {
        public BdmFinderViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback) : base(content, publishedValueFallback) { }

        public IEnumerable<BDmcontact> Results;

        public BDmcontact BDM { get; set; }
        public BDmcontact TBDM { get; set; }
        public bool IsIEL { get; set; }
        public string ListingUrl { get; set; }

        public string FcaNumber { get; set; }
        public string Postcode { get; set; }

    }
}
