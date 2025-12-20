using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Catalog_Service.Features.OccasionsFeature.GetAllOccasions
{
    public class Handlers : IRequestHandler<GetAllOccasionsQuery, EndpointResponse<List<OccasionViewModel>>>
    {
        private readonly IBaseRepository<Occasion> _repo;
        private readonly IDistributedCache _cache;

        private const string CACHE_KEY = "occasions_all";

        public Handlers(IBaseRepository<Occasion> repo, IDistributedCache cache)
        {
            _repo = repo;
            _cache = cache;
        }




        public async Task<EndpointResponse<List<OccasionViewModel>>> Handle(GetAllOccasionsQuery request, CancellationToken cancellationToken)
        {
            // Try cache
            var cached = await _cache.GetStringAsync(CACHE_KEY);
            if (cached != null)
            {
                var cachedData = JsonSerializer.Deserialize<List<OccasionViewModel>>(cached);
                return EndpointResponse<List<OccasionViewModel>>.SuccessResponse(cachedData!);
            }

            // DB
            var occasions = await _repo.GetAll()
                .Where(o => o.IsActive && !o.IsDeleted)
                .Select(o => new OccasionViewModel(
                    o.Id,
                    o.Name,
                    o.Description,
                    o.ImageUrl
                ))
                .ToListAsync();

            // Save to Redis
            var serialized = JsonSerializer.Serialize(occasions);
            await _cache.SetStringAsync(CACHE_KEY, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

            return EndpointResponse<List<OccasionViewModel>>.SuccessResponse(occasions);
        }
    }
}