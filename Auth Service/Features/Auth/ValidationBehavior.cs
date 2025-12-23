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

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);
                var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
                var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

                if (failures.Count != 0)
                {
                    var cleanErrors = failures
                        .Select(f => new
                        {
                            Field = f.PropertyName.Replace("RegisterDto.", ""),
                            f.ErrorMessage
                        });

                    throw new FluentValidation.ValidationException(cleanErrors
                        .Select(e => new FluentValidation.Results.ValidationFailure(e.Field, e.ErrorMessage)));
                }


            }
            return await next();

        }
    }
}