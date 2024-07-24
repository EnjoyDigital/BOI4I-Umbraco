namespace BOI.Core.Search.Models
{
    public class MediaRequestLog
    {
        public int Id { get; set; }
        public string MediaUrl { get; set; }
        public DateTime DateViewed { get; set; }
        public Guid MediaItemId { get; set; }
    }
}
