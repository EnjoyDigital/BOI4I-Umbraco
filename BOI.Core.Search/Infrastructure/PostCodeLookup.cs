using BOI.Core.Constants;
using Microsoft.Extensions.Configuration;
using Nest;
using System.Net.Http.Json;

namespace BOI.Core.Search.Infrastructure
{
    public interface IPostcodeGeocoder
    {
        IEnumerable<BulkPostcodesResponseResult> PostCodeLookup(IEnumerable<string> postcodes);
    }

    public class PostcodeGeocoder : IPostcodeGeocoder
    {
        private readonly IConfiguration configuration;

        public PostcodeGeocoder(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        
        public IEnumerable<BulkPostcodesResponseResult> PostCodeLookup(IEnumerable<string> postCodes)
        {
            var client = new HttpClient { BaseAddress = new Uri(configuration[ConfigurationConstants.PostcodeLookupBaseAddress]) };

            var chunks = postCodes.Chunk(100);

            return chunks.SelectMany(chunk => DoBulkLookup(client, chunk).Result).Where(result => result.Result != null);
        }

        private static BulkPostcodesResponse DoBulkLookup(HttpClient client, IEnumerable<string> chunk)
        {
            var response = client.PostAsJsonAsync("postcodes", new { postcodes = chunk }).Result;

            return response.Content.ReadFromJsonAsync<BulkPostcodesResponse>().Result;
        }

    }

    public class BulkPostcodesResponseResult
    {
        public string Query { get; set; }

        public Search.Models.GeoLocation Result { get; set; }
    }

    public class BulkPostcodesResponse
    {
        public int Status { get; set; }

        public IEnumerable<BulkPostcodesResponseResult> Result { get; set; }
    }
}
