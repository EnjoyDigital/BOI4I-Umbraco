using BOI.Core.Search.Models;
using BOI.Umbraco.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace BOI.Core.Web.Models.ViewModels
{
	public class SolicitorResultsViewModel : SolicitorLanding
	{
		public SolicitorResultsViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback) : base(content, publishedValueFallback) { }

		public SolicitorsResults Results;

		public string ListingUrl { get; set; }

		public Page<SolicitorResult> Paging { get; set; }

	}
}
