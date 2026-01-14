using MediatR;
using Promotion_Service.Entities;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Loyalty.GetTransactions
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
