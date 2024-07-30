using System.Collections.Generic;

namespace BOI.Core.Search.Models
{

    public class SolicitorsResults
    {
        public int Page { get; set; }

        public int Size { get; set; }

        public int Total { get; set; }

        public List<SolicitorResult> QueryResults { get; set; }

        public IEnumerable<SearchFilter> Filters { get; set; }
    }

    public class SolicitorResult
    {
        public string SolicitorName { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string Address3 { get; set; }

        public string Address4 { get; set; }

        public string Address5 { get; set; }

        public string PostCode { get; set; }

        public string Telephone { get; set; }

        public string Distance { get; set; }

        public float Lon { get; set; }

        public float Lat { get; set; }
    }
}
