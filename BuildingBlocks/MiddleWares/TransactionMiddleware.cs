using BuildingBlocks.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.MiddleWares
{
    // It is recommended to implement IMiddleware when injecting a Scoped service (IUnitOfWork) in the constructor
    public class TransactionMiddleware : IMiddleware
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionMiddleware(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // ✅ Optimization: Check if the request is a read-only operation.
            // We exclude GET, OPTIONS, and HEAD methods to avoid unnecessary transaction overhead.
            if (HttpMethods.IsGet(context.Request.Method) ||
                HttpMethods.IsOptions(context.Request.Method) ||
                HttpMethods.IsHead(context.Request.Method))
            {
                // Proceed directly without opening a transaction
                await next(context);
                return;
            }

            // ✅ Begin transaction for state-modifying operations (POST, PUT, DELETE, PATCH)
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await next(context);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}