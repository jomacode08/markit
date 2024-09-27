using MediatR;
using Microsoft.Extensions.Logging;

namespace markit.Application.Common.Behaviours
{
    /// <summary>
    /// Behaviour que monitorea las excepciones que puedan ocurrir dentro de los
    /// Handlers de los Commands/Queries y registra un log.
    /// </summary>
    public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<TRequest> _logger;

        public UnhandledExceptionBehaviour(ILogger<TRequest> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Monitorar el request
            try
            {
                return await next();
            }
            // Registrar log con la información de la excepción.
            catch (Exception ex)
            {
                string requestName = typeof(TRequest).Name;
                string message = "Application Request: Sucedio una excepción para el request {Name} {@Request}";
                _logger.LogError(ex, message, requestName, request);
                throw;
            }
        }
    }
}
