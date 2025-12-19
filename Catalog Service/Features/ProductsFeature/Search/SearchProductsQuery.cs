using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.ProductsFeature.Search
{
    public class SearchProductsQuery : IRequest<RequestResponse<List<ViewModel>>>
    {
        public DTOs Filters { get; }

        public SearchProductsQuery(DTOs filters)
        {
            Filters = filters;
        }
    
    }
}
