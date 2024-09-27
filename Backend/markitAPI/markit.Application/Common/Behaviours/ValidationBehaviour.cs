using FluentValidation;
using MediatR;
using ValidationException = markit.Application.Exceptions.ValidationException;

namespace markit.Application.Common.Behaviours
{
    /// <summary>
    /// Validation Behaviour que monitorea el objeto Request del cliente,
    /// y evalua si existe alguna validación de alguna de las propiedades del componente enviado por el cliente.
    /// </summary>
    public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
        {
            // Inyección Validator de FluentValidation
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Se valida si existen validaciones configuradas en la aplicación
            if (_validators.Any())
            {
                // Validation Context
                var context = new ValidationContext<TRequest>(request);

                // Se obtienen las validaciones que contengan errores
                var validationsTasks = _validators.Select(v => v.ValidateAsync(context, cancellationToken));
                var validationResults = await Task.WhenAll(validationsTasks);
                var validationFailures = validationResults.SelectMany(r => r.Errors).Where(f => f != null);

                // Se verifica si se cuenta con errores
                if (validationFailures.Any())
                {
                    // Se ejecuta una excepción del tipo personalizado ValidationException
                    throw new ValidationException(validationFailures);
                }
            }

            // Que continue el flujo si no existen errores de validación
            return await next();
        }
    }
}
