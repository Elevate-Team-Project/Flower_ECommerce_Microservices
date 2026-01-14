using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Delivery_Service.Features.Addresses.CreateAddress.Create_Address_automatically.DTOs;

namespace Delivery_Service.Infrastructure.Services
{
    public class CachedOsmGeocodingService : IGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public CachedOsmGeocodingService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<AddressAutoDto> GetAddressFromCoordinates(double latitude, double longitude)
        {
            
            double lat = Math.Round(latitude, 4);
            double lon = Math.Round(longitude, 4);

            var cacheKey = $"geo_{lat}_{lon}";

            if (_cache.TryGetValue(cacheKey, out AddressAutoDto cached))
            {
                return cached;
            }

            var url =
                $"https://nominatim.openstreetmap.org/reverse?lat={lat}&lon={lon}&format=json";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add(
                "User-Agent",
                "FlowerDelivery/1.0 (shiplo146@gmail.com)"
            );

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return EmptyAddress();
            }

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            string? displayName =
                root.TryGetProperty("display_name", out var display)
                    ? display.GetString()
                    : null;

           
            if (!root.TryGetProperty("address", out var address))
            {
                return EmptyAddress(displayName);
            }

            var result = new AddressAutoDto
            {
                Street = displayName,
                City =
                    address.TryGetProperty("city", out var city)
                        ? city.GetString()
                        : address.TryGetProperty("town", out var town)
                            ? town.GetString()
                            : address.TryGetProperty("village", out var village)
                                ? village.GetString()
                                : null,
                Country =
                    address.TryGetProperty("country", out var country)
                        ? country.GetString()
                        : null,

                PostalCode =
               address.TryGetProperty("postcode", out var postcode)
              ? postcode.GetString()
               : null
            };

            _cache.Set(
                cacheKey,
                result,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6),
                    SlidingExpiration = TimeSpan.FromHours(1)
                });

            return result;
        }

        private static AddressAutoDto EmptyAddress(string? street = null)
        {
            return new AddressAutoDto
            {
                Street = street ?? "Address not found",
                City = null,
                Country = null
            };
        }
    }
}
