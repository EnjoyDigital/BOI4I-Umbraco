namespace BOI.Core.Web.Models.Cors
{
    public class CorsConfig
    {
        public CorsConfig() { }
        public Origin[] Origins { get; set; }
    }

    public class Origin
    {
        public string RequestOrigin { get; set; }
    }
}
