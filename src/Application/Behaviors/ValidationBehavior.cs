using FluentValidation;
using MediatR;

namespace CleanArchitectureCqrsTemplate.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs all FluentValidation validators for the
/// incoming request. On failure it throws <see cref="ValidationException"/>,
/// which the WebApi's global exception middleware maps to a 400 response.
/// Throwing (instead of returning a failure Result) keeps this behavior
/// response-type agnostic: it works for any TResponse without reflection.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
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
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var results = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = results
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);
        }

        return await next();
    }
}
