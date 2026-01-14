using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using Catalog_Service.Features.OccasionsFeature.GetAllOccasions;
using Catalog_Service.Features.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Catalog_Service.Features.OccasionsFeature
{
    public class Handlers : IRequestHandler<GetAllOccasionProductsQuery, EndpointResponse<List<OccasionProductDto>>>
    {
        private readonly IBaseRepository<ProductOccasion> _repo;


        public Handlers(IBaseRepository<ProductOccasion> repo)
        {
            _repo = repo;
        }




        public async Task<EndpointResponse<List<OccasionProductDto>>> Handle(GetAllOccasionProductsQuery request, CancellationToken cancellationToken)
        {

            var products =await  _repo
              .GetAll()
              .Where(po => po.OccasionId == request.OccasionId)
              .Include(po=>po.Product)
              .ThenInclude(po=>po.Images)
              .Select(po=>new OccasionProductDto
              {
                  ProductId= po.ProductId,
                  Name= po.Product.Name,
                  Price= po.Product.Price,
                  Images= po.Product.Images.Select(i=>i.ImageUrl).ToList()
              }
              
              
              
              
              
              ).ToListAsync();

            if (!products.Any())
                return EndpointResponse<List<OccasionProductDto>>
                    .ErrorResponse("No products found for this occasion");

            return EndpointResponse<List<OccasionProductDto>>
                .SuccessResponse(products);
          
          
          
        }
    }
}