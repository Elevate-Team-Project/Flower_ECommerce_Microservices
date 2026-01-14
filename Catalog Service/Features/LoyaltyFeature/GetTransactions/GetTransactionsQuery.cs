using MediatR;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.LoyaltyFeature.GetTransactions
{
    public record GetTransactionsQuery(string UserId) : IRequest<EndpointResponse<List<LoyaltyTransactionDto>>>;

    public record LoyaltyTransactionDto(
        int Id,
        TransactionType Type,
        int Points,
        string Description,
        int? OrderId,
        decimal? OrderAmount,
        int BalanceAfter,
        DateTime CreatedAt
    );
}
