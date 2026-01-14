using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.LoyaltyFeature.GetTransactions
{
    public class GetTransactionsHandler : IRequestHandler<GetTransactionsQuery, EndpointResponse<List<LoyaltyTransactionDto>>>
    {
        private readonly IBaseRepository<LoyaltyAccount> _accountRepository;
        private readonly IBaseRepository<LoyaltyTransaction> _transactionRepository;

        public GetTransactionsHandler(
            IBaseRepository<LoyaltyAccount> accountRepository,
            IBaseRepository<LoyaltyTransaction> transactionRepository)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<EndpointResponse<List<LoyaltyTransactionDto>>> Handle(
            GetTransactionsQuery request,
            CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetAll()
                .FirstOrDefaultAsync(a => a.UserId == request.UserId, cancellationToken);

            if (account == null)
                return EndpointResponse<List<LoyaltyTransactionDto>>.SuccessResponse(
                    new List<LoyaltyTransactionDto>(), "No loyalty account found");

            var transactions = await _transactionRepository.GetAll()
                .Where(t => t.LoyaltyAccountId == account.Id)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new LoyaltyTransactionDto(
                    t.Id,
                    t.Type,
                    t.Points,
                    t.Description,
                    t.OrderId,
                    t.OrderAmount,
                    t.BalanceAfter,
                    t.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            return EndpointResponse<List<LoyaltyTransactionDto>>.SuccessResponse(
                transactions, "Transactions retrieved successfully");
        }
    }
}
