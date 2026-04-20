using FluentValidation;
using MediatR;
using YourApp.Application.Common.Exceptions;
using ValidationException = YourApp.Application.Common.Exceptions.ValidationException;

namespace YourApp.Application.Common.Behaviors;

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

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();

            if (failures.Count != 0)
            {
                // Group by property name to match ProblemDetails errors format: "PropertyName": ["Error 1", "Error 2"]
                var errors = failures
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key, 
                        g => g.Select(x => x.ErrorMessage).ToArray()
                    );
                    
                // Throw CỦA CHÚNG TA thay vì của FluentValidation
                throw new ValidationException(errors);
            }
        }

        return await next();
    }
}
