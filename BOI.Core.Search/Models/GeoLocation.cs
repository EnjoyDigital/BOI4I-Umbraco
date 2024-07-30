using Nest;
using Newtonsoft.Json;

namespace BOI.Core.Search.Models
{
    public class GeoLocation
    {
        [JsonProperty("latitude")]
        public string Lat { get; set; }

        [JsonProperty("longitude")]
        public string Lon { get; set; }

        [Text(Ignore = true)]
        public string Postcode { get; set; }

        [Text(Ignore = true)]
        public string GeoCodeStringForGoogleMaps { get { return string.Concat(Lat, ",", Lon); } }
    }
}
