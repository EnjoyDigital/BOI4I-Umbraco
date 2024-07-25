using System;
using System.Collections.Generic;
using System.Linq;

namespace BOI.Core.Search.Models
{

    public class BdmResults
    {
        public int Page { get; set; }

        public int Size { get; set; }

        public int Total { get; set; }

        public List<BdmResult> QueryResults { get; set; }

        public IEnumerable<SearchFilter> Filters { get; set; }
    }

    public class BdmResult
    {
        public BdmResult()
        {
        }

        public BdmResult(Bdm searchResult)
        {
            ItemId = searchResult.Id;
            PostCodeRaw = searchResult.Regions??string.Empty;
           // Postcodes = PostCodeRaw?.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)??Enumerable.Empty<string>();

            FcaCodesRaw = searchResult.FCANumber ?? string.Empty;
          //  FCAcodes = FcaCodesRaw?.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>();

        }


        public int ItemId { get; set; }
        public Guid ItemGuid { get; set; }

        public string PostCodeRaw { get; set; }
        public string FcaCodesRaw { get; set; }
        //public IEnumerable<string> Postcodes { get; set; }
        //public IEnumerable<string> FCAcodes { get; set; }

    }
}
