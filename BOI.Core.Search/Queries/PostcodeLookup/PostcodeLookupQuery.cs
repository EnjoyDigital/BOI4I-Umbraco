using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text;
using System.Runtime.Serialization.Json;
using Microsoft.Extensions.Logging;
using BOI.Core.Constants;
using BingMapsRESTToolkit;

namespace BOI.Core.Search.Queries.PostcodeLookup
{
    public class PostcodeLookupQuery
    {
        public class Request
        {
            public string Postcode { get; set; }

        }

        public class RequestHandler : IRequestHandler
        {
            private readonly IConfiguration config;

            public RequestHandler(IConfiguration config, ILogger<PostcodeLookupQuery.RequestHandler> logger)
            {
                this.config = config;
                Logger = logger;
            }

            public ILogger Logger { get; }

            public Result Execute(Request request)
            {
                var result = new Result();
                string url = string.Concat("http://dev.virtualearth.net/REST/v1/Locations?query=", request.Postcode, "&key=", config[ConfigurationConstants.VirtualWorldPostcodeLookupKey]);
                try
                {
                    using (var client = new WebClient())
                    {
                        string response = client.DownloadString(url);

                        //Would have said use newtonsoft for the deserialisation, but the location doesnt then convert
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Response));
                        using (var es = new MemoryStream(Encoding.Unicode.GetBytes(response)))
                        {
                            var mapResponse = ser.ReadObject(es) as Response;
                            Location location = (Location)mapResponse.ResourceSets.First().Resources.First();
                            return new Result()
                            {
                                Latitude = location.Point.Coordinates[0],
                                Longitude = location.Point.Coordinates[1]
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error retrieving latlng based on postcode");
                    
                }

                return result;
            }
        }

        public class Result
        {


            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }


    }
}
