using Auth_Service.Features.Shared;
using FluentValidation;
using MediatR;

namespace Auth.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (!failures.Any())
                return await next();

            var message = string.Join("\n",
                failures.Select(f =>
                {
                    var field = f.PropertyName.Replace("RegisterDto.", "");
                    return $"{field}: {f.ErrorMessage}";
                })
             );

            var responseType = typeof(TResponse);

            if (responseType.IsGenericType &&
                responseType.GetGenericTypeDefinition().Name.StartsWith("RequestResponse"))
            {
                var failMethod = responseType.GetMethod("Fail");
                var failResult = failMethod!.Invoke(null, new object[] { message });

                return (TResponse)failResult!;
            }

            throw new ValidationException(failures);
        }
    }
}