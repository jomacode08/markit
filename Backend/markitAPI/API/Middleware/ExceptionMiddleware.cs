using markit.API.Middleware.Errors;
using markit.Application.Exceptions;
using System.Net;

namespace markit.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _requestDelegate;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware
        (
            RequestDelegate requestDelegate,
            ILogger<ExceptionMiddleware> logger
        )
        {
            _requestDelegate = requestDelegate;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Procesamiento de la petición Http Interceptada
                await _requestDelegate(context);
            }
            catch (Exception ex)
            {
                //TODO: Registrar error en Log, verificar funcionamiento
                _logger.LogError(ex, ex.Message);

                // Personalizar Response
                CodeErrorException customResponse;
                HttpStatusCode statusCode;

                switch (ex)
                {
                    // Validaciónes del Request 
                    case ValidationException validationException: {

                        statusCode = HttpStatusCode.BadRequest;
                        customResponse = new(statusCode, validationException.Message, validationException.Errors);
                        break;

                    }

                    // Validaciones personalizadas
                    case CustomValidationException customValidationException:
                    {
                        statusCode = HttpStatusCode.Conflict;
                        customResponse = new(statusCode, customValidationException.Message, ex.StackTrace);
                        break;
                    }

                    // Recursos no encontrados
                    case NotFoundException notFoundException:
                    {
                        statusCode = HttpStatusCode.NotFound;
                        customResponse = new(statusCode, notFoundException.Message, ex.StackTrace);
                        break;
                    }

                    // Errores internos
                    default:
                    {
                        statusCode = HttpStatusCode.InternalServerError;
                        customResponse = new(statusCode, ex.Message, ex.StackTrace);
                        break;
                    }
                }

                context.Response.StatusCode = (int)statusCode;
                await context.Response.WriteAsJsonAsync(customResponse);
            }
        }
    }
}
