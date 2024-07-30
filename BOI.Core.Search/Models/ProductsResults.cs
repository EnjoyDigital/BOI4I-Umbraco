namespace BOI.Core.Search.Models
{
    public class ProductsResults
    {
        public List<ProductResult> QueryResults { get; set; }
        public IEnumerable<SearchFilter> Filters { get; set; }
        public ProductRefineFilters RefineFilters { get; set; }

    }

    public class ProductResult
    {
        public string ProductType { get; set; }
        public string Category { get; set; }
        public string LTVTitle { get; set; }
        public string LTVFilterText { get; set; }
        public bool InterestOnly { get; set; }
        public string LaunchDateTime { get; set; }
        public bool IsNew { get; set; }
        public string Term { get; set; }
        public decimal Rate { get; set; }
        public string FormattedRate => Rate.ToString("N2");
        public bool IsFixedRate { get; set; }
        public string Description { get; set; }
        public decimal OverallCost { get; set; }
        public string FormattedOverallCost => OverallCost.ToString("N1");
        public decimal ProductFees { get; set; }
        public string Features { get; set; }
        public string EarlyRepaymentCharges { get; set; }
        public string Code { get; set; }
        public string WithdrawalDateTime { get; set; }
        public string AIPDeadlineDateTime { get; set; }
        public string ProductVariant { get; set; }
        public int LTVSortOrder { get; set; }
    }

    public class ProductRefineFilters
    {
        public List<RefineFilter> ProductCategoryList { get; set; }
        public List<RefineFilter> ProductTypeList { get; set; }
        public List<RefineFilter> ProductLTVList { get; set; }
        public List<RefineFilter> ProductTermList { get; set; }
    }
}
