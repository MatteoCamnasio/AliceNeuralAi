using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AliceNeural.Models
{
    public class BusinessAddress
    {
        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [JsonPropertyName("addressLine")]
        public string? AddressLine { get; set; }

        [JsonPropertyName("locality")]
        public string? Locality { get; set; }

        [JsonPropertyName("adminDivision")]
        public string? AdminDivision { get; set; }

        [JsonPropertyName("countryIso2")]
        public string? CountryIso2 { get; set; }

        [JsonPropertyName("countryRegion")]
        public string? CountryRegion { get; set; }

        [JsonPropertyName("postalCode")]
        public string? PostalCode { get; set; }

        [JsonPropertyName("formattedAddress")]
        public string? FormattedAddress { get; set; }
    }

    public class BusinessesAtLocation
    {
        [JsonPropertyName("businessAddress")]
        public BusinessAddress? BusinessAddress { get; set; }

        [JsonPropertyName("businessInfo")]
        public BusinessInfo? BusinessInfo { get; set; }
    }

    public class BusinessInfo
    {
        [JsonPropertyName("entityName")]
        public string? EntityName { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("otherTypes")]
        public List<string?>? OtherTypes { get; set; }

        [JsonPropertyName("wheelchairAccessible")]
        public string? WheelchairAccessible { get; set; }
    }

    public class ResourceLocal
    {
        [JsonPropertyName("__type")]
        public string? Type { get; set; }

        [JsonPropertyName("isPrivateResidence")]
        public string? IsPrivateResidence { get; set; }

        [JsonPropertyName("businessesAtLocation")]
        public List<BusinessesAtLocation>? BusinessesAtLocation { get; set; }

        [JsonPropertyName("naturalPOIAtLocation")]
        public List<object>? NaturalPOIAtLocation { get; set; }

        [JsonPropertyName("addressOfLocation")]
        public List<object>? AddressOfLocation { get; set; }
    }

    public class ResourceSetLocal
    {
        [JsonPropertyName("estimatedTotal")]
        public int? EstimatedTotal { get; set; }

        [JsonPropertyName("resources")]
        public List<ResourceLocal>? Resources { get; set; }
    }

    public class LocalRecognition
    {
        [JsonPropertyName("authenticationResultCode")]
        public string? AuthenticationResultCode { get; set; }

        [JsonPropertyName("brandLogoUri")]
        public string? BrandLogoUri { get; set; }

        [JsonPropertyName("copyright")]
        public string? Copyright { get; set; }

        [JsonPropertyName("resourceSets")]
        public List<ResourceSetLocal>? ResourceSets { get; set; }

        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("statusDescription")]
        public string? StatusDescription { get; set; }

        [JsonPropertyName("traceId")]
        public string? TraceId { get; set; }
    }
}
