namespace BOI.Core.Search.Models
{
    public class CriteriaLookupsResults
    {
        public List<CriteriaLookupResult> QueryResults { get; set; }
    }

    public class CriteriaLookupResult
    {
        public int Id { get; set; }
        public string NameId { get; set; }
        public string CriteriaName { get; set; }
        public int SortOrder { get; set; }
        public string BodyText { get; set; }
        public string CriteriaCategory { get; set; }
        public bool IsBuyToLetProduct { get; set; }
        public bool IsResidentialProduct { get; set; }
        public bool IsBespokeProduct { get; set; }
        public IEnumerable<CriteriaTabResult> CriteriaTabs { get; set; }
        public string CriteriaUpdatedDate { get; set; }
    }

    public class CriteriaTabResult
    {
        public int Id { get; set; }
        public int SortOrder { get; set; }
        public string CriteriaTabName { get; set; }
        public string BodyText { get; set; }
        public string CriteriaTabUpdatedDate { get; set; }
    }

    public class CriteriaLookupRefineFilters
    {
        public List<RefineFilter> CategoryList { get; set; }
        public string CriteriaCategoryName { get; set; }
    }
}
