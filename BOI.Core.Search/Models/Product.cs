namespace BOI.Core.Search.Models
{
    public class Product
    {
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
    }
}
