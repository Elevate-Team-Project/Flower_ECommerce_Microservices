using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.CategoriesFeature.DeleteCategory;


public sealed record DeleteCategoryCommand(int CategoryId)
    : IRequest<RequestResponse<bool>>;
