namespace BankOfIreland.Intermediaries.Core.Web.Models.CmsModels
{
    public partial class BDmpodAction
    {
        public bool BDMFound { get { return BDMDetails != null; } }
        public BDmcontact BDMDetails { get; set; }
    }
}
