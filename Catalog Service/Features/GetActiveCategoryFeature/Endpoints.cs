using Microsoft.CodeAnalysis.CSharp.Syntax;
using Catalog_Service.Features.Shared;
using MediatR;
using Catalog_Service.Features.CategoriesFeature.GetAllCategories;
using Microsoft.CodeAnalysis.Elfie.Model;
namespace Catalog_Service.Features.CategoriesFeature.GetActiveCategoryFeature
{
    public static class Endpoints
    {

        public static void MapGetActiveCategories(this WebApplication app)
        {
            app.MapGet("/api/categories/active", async (IMediator mediator) =>
            {

                var result = await mediator.Send(new GetAllActiveCategoriesQuery());

                if (!result.IsSuccess)
                    return Results.NotFound(result.Message);

                return Results.Ok(EndpointResponse<List<CategoryactiveViewModel>>.SuccessResponse(result.Data!, result.Message));
            } )
 .WithName("GetActiveCategories")
        .WithTags("Categories")
        .WithOpenApi();
        }


         



    }
}



       
          
            
     