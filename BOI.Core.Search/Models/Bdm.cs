using CsvHelper.Configuration.Attributes;
using Nest;

namespace BOI.Core.Search.Models
{
    public class Bdm
    {
        [Name("FCA Firm Reference Number (FRN)")]
        public string FCANumber { get; set; }
        public string Firstname { get; set; }
        public string Surname { get; set; }

        public string JobTitle { get; set; }
        public string Email { get; set; }

        [Name("Postcodes")]
        public string RawPostCodes { get; set; }

        [CsvHelper.Configuration.Attributes.Ignore]
        public IEnumerable<string> PostCodes { get { return RawPostCodes.Split(',').Select(x => x); } }
        public string ContactNumber { get; set; }
        public string Bio { get; set; }

        [CsvHelper.Configuration.Attributes.Ignore]
        public int Id { get; set; }

        [CsvHelper.Configuration.Attributes.Ignore]
        public string NodeTypeAlias { get; set; }

        [CsvHelper.Configuration.Attributes.Ignore]
        public string Regions { get; set; }

        [CsvHelper.Configuration.Attributes.Ignore]
        public bool Active { get; set; }

       
        public string RequireFCAAndPostcodeMatch { get; set; }

        [CsvHelper.Configuration.Attributes.Ignore]
        public string ContactNumberFormatted { get { return ContactNumber.Replace(" ", ""); } }

        public BDMType BdmType { get; set; }
    }

    public enum BDMType
    {
        ELM, BDM, TBDM
    }
}
