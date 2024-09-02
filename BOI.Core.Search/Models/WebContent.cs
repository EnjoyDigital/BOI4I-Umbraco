

namespace BOI.Core.Search.Models
{
    public class WebContent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Template { get; set; }
        public string NodeTypeAlias { get; set; }
        public string Url { get; set; }
        public int ParentNodeId { get; set; }
        public int SortOrder { get; set; }

        //Search Items
        public string SearchTitle { get; set; }
        public string SearchDescription { get; set; }
        public string SearchImage { get; set; }
        public string SearchKeywords { get; set; }
        public bool SearchExclude { get; set; }

        //SEO Items
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public bool CanonicalURL { get; set; }

        //Listing Items
        public string ListingSummary { get; set; }
        public string ListingImage { get; set; }

        //CriteriaLookup Fields
        public string CriteriaName { get; set; }
        public string CriteriaCategory { get; set; }
        public bool BuyToLetProduct { get; set; }
        public bool ResidentialProduct { get; set; }
        public bool BespokeProduct { get; set; }
        public string BodyText { get; set; }
        public string Content { get; set; }
        public string CriteriaUpdateDate { get; set; }
        public string CriteriaTabUpdateDate { get; set; }
        public bool Published { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        //FAQ Fields
        public string FaqCategory { get; set; }
        public string FaqQuestion { get; set; }
        public string FaqAnswer { get; set; }
        //public FAQNestedItem FAQs { get; set; }


        //BDM Contact Fields
        public string FCANumber { get; set; }
        public string Regions { get; set; }
        public string Bio { get; set; }
        public string BDMIdentifier { get; set; }
        public string BDMType { get; set; }
        public string RequireFCAAndPostcodeMatch { get; set; }

        //Product Fields
        public string ProductType { get; set; }
        public string Category { get; set; }
        public string LTVTitle { get; set; }
        public string LTVFilterText { get; set; }
        public bool InterestOnly { get; set; }
        public string LaunchDateTime { get; set; }
        public bool IsNew { get; set; }
        public string Term { get; set; }
        public string Rate { get; set; }
        public bool IsFixedRate { get; set; }
        public string Description { get; set; }
        public string OverallCost { get; set; }
        public string ProductFees { get; set; }
        public string Features { get; set; }
        public string EarlyRepaymentCharges { get; set; }
        public string Code { get; set; }
        public string WithdrawalDateTime { get; set; }
        public string AIPDeadlineDateTime { get; set; }
        public string ProductVariant { get; set; }
        public string PostCodeOutCodes { get; set; }

    }
}
