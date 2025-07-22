using Azure;
using BOI.Core.Constants;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Text.Json;

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
            private readonly IHttpClientFactory httpClientFactory;

            public RequestHandler(IConfiguration config, ILogger<PostcodeLookupQuery.RequestHandler> logger, IHttpClientFactory httpClientFactory)
            {
                this.config = config;
                this.httpClientFactory = httpClientFactory;
                Logger = logger;
            }

            public ILogger Logger { get; }

            public Result Execute(Request request)
            {
                var result = new Result();

                if (string.IsNullOrWhiteSpace(request.Postcode))
                {
                    Logger.LogWarning("Postcode is null or empty");
                    return result;
                }

                // Clean up the postcode format
                var cleanPostcode = request.Postcode.Trim().ToUpperInvariant().Replace(" ", "");
                
                // Basic UK postcode validation
                if (!IsValidUKPostcode(cleanPostcode))
                {
                    Logger.LogWarning("Invalid UK postcode format: {Postcode}", request.Postcode);
                    return result;
                }

                var baseAddress = config.GetValue<string>("CustomSettings:PostcodeLookupBaseAddress");
                if (string.IsNullOrEmpty(baseAddress))
                {
                    Logger.LogError("PostcodeLookupBaseAddress not configured");
                    return result;
                }

                // URL encode the postcode to handle special characters
                var encodedPostcode = Uri.EscapeDataString(cleanPostcode);
                var url = $"{baseAddress.TrimEnd('/')}/postcodes/{encodedPostcode}";

                Logger.LogInformation("Looking up postcode: {Postcode} at URL: {Url}", cleanPostcode, url);

                try
                {
                    using (var httpClient = httpClientFactory.CreateClient())
                    {
                        // Set proper headers
                        httpClient.DefaultRequestHeaders.Add("User-Agent", "BOI-WebApp/1.0");
                        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                        // Make the request
                        var response = httpClient.GetAsync(url).Result;
                        
                        if (!response.IsSuccessStatusCode)
                        {
                            var errorContent = response.Content.ReadAsStringAsync().Result;
                            
                            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                            {
                                Logger.LogWarning("Postcode not found: {Postcode}. API Response: {ErrorContent}", 
                                    cleanPostcode, errorContent);
                            }
                            else
                            {
                                Logger.LogError("Postcodes.io API returned error: {StatusCode} - {ReasonPhrase}. Response: {ErrorContent}", 
                                    response.StatusCode, response.ReasonPhrase, errorContent);
                            }
                            
                            return result;
                        }

                        var responseContent = response.Content.ReadAsStringAsync().Result;
                        Logger.LogDebug("API Response for postcode {Postcode}: {Response}", cleanPostcode, responseContent);

                        // Parse the JSON response
                        using (JsonDocument document = JsonDocument.Parse(responseContent))
                        {
                            var root = document.RootElement;
                            
                            if (root.TryGetProperty("result", out var resultElement))
                            {
                                if (resultElement.TryGetProperty("latitude", out var latElement) && 
                                    resultElement.TryGetProperty("longitude", out var lonElement))
                                {
                                    double latitude = 0, longitude = 0;
                                    bool latValid = false, lonValid = false;
                                    
                                    // Handle both string and numeric values
                                    if (latElement.ValueKind == JsonValueKind.String)
                                    {
                                        latValid = double.TryParse(latElement.GetString(), out latitude);
                                    }
                                    else if (latElement.ValueKind == JsonValueKind.Number)
                                    {
                                        latitude = latElement.GetDouble();
                                        latValid = true;
                                    }
                                    
                                    if (lonElement.ValueKind == JsonValueKind.String)
                                    {
                                        lonValid = double.TryParse(lonElement.GetString(), out longitude);
                                    }
                                    else if (lonElement.ValueKind == JsonValueKind.Number)
                                    {
                                        longitude = lonElement.GetDouble();
                                        lonValid = true;
                                    }
                                    
                                    if (latValid && lonValid)
                                    {
                                        Logger.LogInformation("Successfully found coordinates for postcode {Postcode}: Lat={Latitude}, Lon={Longitude}", 
                                            cleanPostcode, latitude, longitude);
                                        return new Result()
                                        {
                                            Latitude = latitude,
                                            Longitude = longitude
                                        };
                                    }
                                }
                            }
                            
                            Logger.LogWarning("No location data found in response for postcode: {Postcode}", cleanPostcode);
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    Logger.LogError(ex, "HTTP request error retrieving latlng based on postcode: {Postcode}", cleanPostcode);
                }
                catch (JsonException ex)
                {
                    Logger.LogError(ex, "JSON parsing error for postcode: {Postcode}", cleanPostcode);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error retrieving latlng based on postcode: {Postcode}", cleanPostcode);
                }

                return result;
            }

            /// <summary>
            /// Basic UK postcode validation
            /// </summary>
            private bool IsValidUKPostcode(string postcode)
            {
                if (string.IsNullOrWhiteSpace(postcode))
                    return false;

                // UK postcode format: A[A]N[A N]NAA or A[A]NAA
                // Examples: M1 1AA, M60 1NW, CR2 6XH, DN55 1PT, W1A 1HQ, EC1A 1BB
                var pattern = @"^[A-Z]{1,2}[0-9][A-Z0-9]?\s*[0-9][A-Z]{2}$";
                return System.Text.RegularExpressions.Regex.IsMatch(postcode, pattern);
            }
        }

        public class Result
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }
    }
}
