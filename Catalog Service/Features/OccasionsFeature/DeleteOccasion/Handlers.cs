using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.OccasionsFeature.DeleteOccasion
{
    public class Handlers : IRequestHandler<DeleteOccasionCommand, RequestResponse<bool>>
    {
        private readonly IBaseRepository<Entities.Occasion> _occasionRepository;
        //private readonly IBaseRepository<ProductOccasion> _productOccasionRepository;
        private readonly IUnitOfWork _uow;

        public Handlers(IBaseRepository<Entities.Occasion> occasionRepository,
            IBaseRepository<ProductOccasion> productOccasionRepository,
            IUnitOfWork uow
            )
        {
            this._occasionRepository = occasionRepository;
            //this._productOccasionRepository = productOccasionRepository;
            this._uow = uow;
        }

        public async Task<RequestResponse<bool>> Handle(DeleteOccasionCommand request, CancellationToken cancellationToken)
        {
            var occasionToDelete = _occasionRepository
             .Get(c => c.Id == request.OccasionId && !c.IsDeleted)
             .FirstOrDefault();

            if (occasionToDelete is null)
                return  RequestResponse<bool>.Fail("Occasion not found");


            //var checkoccasionProducts = _productOccasionRepository
            //    .Get(po => po.OccasionId == request.OccasionId&&!po.Occasion.IsDeleted)
            //    .Any();

            var productsToOccasion = _occasionRepository
                .Get(c => c.Id == request.OccasionId && !c.IsDeleted)
                .SelectMany(c => c.ProductOccasions)
                .Any();  


            if (productsToOccasion)
                return RequestResponse<bool>.Fail("Occasion has associated products and cannot be deleted");


            _occasionRepository.Delete(occasionToDelete);


            await _uow.SaveChangesAsync(cancellationToken);

            return RequestResponse<bool>.Success(true, "Occasion deleted successfully");
        }
    }
}
