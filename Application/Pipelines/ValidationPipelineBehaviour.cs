using FluentValidation;
using MediatR;

namespace Application.Pipelines
{
    namespace Application.Pipelines
    {
        public class ValidationPipelineBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
        {
            private readonly IEnumerable<IValidator<TRequest>> _validators;

            public ValidationPipelineBehaviour(IEnumerable<IValidator<TRequest>> validators)
            {
                _validators = validators;
            }

            public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
            {
                if (!_validators.Any())
                {
                    return await next();
                }

                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(
                    _validators.Select(v => v.ValidateAsync(context, cancellationToken))
                );

                var failures = validationResults
                    .SelectMany(r => r.Errors)
                    .Where(f => f != null)
                    .ToList();

                if (failures.Count != 0)
                {
                    // Düzeltildi: Tam adını (Fully Qualified Name) kullanarak
                    throw new FluentValidation.ValidationException(failures);
                }

                return await next();
            }
        }
    }
}
