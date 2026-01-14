//using System.Net.Http.Json;
//using System.Text.RegularExpressions;
//using Microsoft.Extensions.Caching.Memory;

//namespace Delivery_Service.Features.Addresses.Geocoding;

//public record GeocodeResult(double Latitude, double Longitude, string DisplayName);

//public interface IGeocodingService
//{
//    Task<GeocodeResult?> ForwardGeocodeAsync(string address, CancellationToken ct = default);
//}

///// <summary>
///// Nominatim (OpenStreetMap) geocoder with in-memory caching and request normalization.
///// Free, no API key. Respect rate limits (~1 req/sec) and provide a User-Agent.
///// </summary>
//public sealed class NominatimGeocodingService : IGeocodingService
//{
//    private readonly HttpClient _http;
//    private readonly IMemoryCache _cache;
//    private static readonly MemoryCacheEntryOptions CacheOptions = new()
//    {
//        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
//        SlidingExpiration = TimeSpan.FromMinutes(10)
//    };

//    public NominatimGeocodingService(HttpClient http, IMemoryCache cache)
//    {
//        _http = http;
//        _cache = cache;

//        // Nominatim requires a meaningful User-Agent with contact info.
//        if (!_http.DefaultRequestHeaders.UserAgent.Any())
//            _http.DefaultRequestHeaders.UserAgent.ParseAdd("FlowerDelivery/1.0 (contact@example.com)");
//    }

//    public async Task<GeocodeResult?> ForwardGeocodeAsync(string address, CancellationToken ct = default)
//    {
//        if (string.IsNullOrWhiteSpace(address))
//            return null;

//        var normalized = Normalize(address);
//        var cacheKey = $"geo:{normalized}";

//        if (_cache.TryGetValue(cacheKey, out GeocodeResult cached))
//            return cached;

//        // Use normalized value for cache key, but send the ORIGINAL address text to Nominatim
//        var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";

//        try
//        {
//            using var response = await _http.GetAsync(url, ct);

//            if (!response.IsSuccessStatusCode)
//            {
//                // 403 or any other non-success → treat as "no result" instead of throwing
//                return null;
//            }

//            var results = await response.Content.ReadFromJsonAsync<List<NominatimResponse>>(cancellationToken: ct);
//            var first = results?.FirstOrDefault();

//            if (first is null)
//                return null;

//            var result = new GeocodeResult(first.lat, first.lon, first.display_name);
//            _cache.Set(cacheKey, result, CacheOptions);
//            return result;
//        }
//        catch
//        {
//            // Network / deserialization errors → fail gracefully
//            return null;
//        }
//    }

//    private static string Normalize(string address)
//    {
//        // Lower, trim, and remove all whitespace characters to stabilize cache keys and queries.
//        var trimmed = address.Trim().ToLowerInvariant();
//        return Regex.Replace(trimmed, "\\s+", string.Empty);
//    }

//    private sealed record NominatimResponse(double lat, double lon, string display_name);
//}

