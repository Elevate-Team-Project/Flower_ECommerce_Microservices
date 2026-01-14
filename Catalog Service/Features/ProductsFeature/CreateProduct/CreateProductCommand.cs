using BuildingBlocks.Interfaces;
using FluentValidation;
using MediatR;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.ProductsFeature.CreateProduct
{
    public class CreateProductCommand : IRequest<EndpointResponse<int>>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int MinStock { get; set; }
        public int MaxStock { get; set; }
        public int? BrandId { get; set; }
        public int CategoryId { get; set; }
        public List<int> OccasionIds { get; set; } = new();
        public List<string> ImageUrls { get; set; } = new();
        public List<ProductSpecificationDto> Specifications { get; set; } = new();
        public bool IsAvailable { get; set; }
    }

    public class ProductSpecificationDto
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class CreateProductValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
            RuleFor(x => x.MinStock).GreaterThanOrEqualTo(0);
            RuleFor(x => x.MaxStock).GreaterThan(x => x.MinStock).WithMessage("MaxStock must be greater than MinStock");
            RuleFor(x => x.CategoryId).GreaterThan(0);
        }
    }
}
