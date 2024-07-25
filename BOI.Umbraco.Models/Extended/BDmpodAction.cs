namespace BOI.Umbraco.Models
{
    public partial class BDmpodAction
    {
        public bool BDMFound { get { return BDMDetails != null; } }
        public BDmcontact BDMDetails { get; set; }
    }
}
