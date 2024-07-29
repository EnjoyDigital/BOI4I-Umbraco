using Azure;
using BOI.Core.Constants;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BingMapsRESTToolkit;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace BOI.Core.Search.Queries.PostcodeLookup
{
    public class PostcodeLookupQuery
    {
        public class Request
        {
            public string Postcode { get; set; }

        }

        public class RequestHandler
        {
            private readonly IConfiguration config;

            public RequestHandler(IConfiguration config)
            {
                this.config = config;
            }


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
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(BingMapsRESTToolkit.Response));
                        using (var es = new MemoryStream(Encoding.Unicode.GetBytes(response)))
                        {
                            var mapResponse = (ser.ReadObject(es) as BingMapsRESTToolkit.Response);
                            BingMapsRESTToolkit.Location location = (BingMapsRESTToolkit.Location)mapResponse.ResourceSets.First().Resources.First();
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
