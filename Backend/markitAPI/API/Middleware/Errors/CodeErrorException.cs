using System.Net;

namespace markit.API.Middleware.Errors
{
    public class CodeErrorException
    {
        public HttpStatusCode StatusCode { get; set; }
        public string? Message { get; set; }
        public string? Details { get; set; }
        public IDictionary<string, string[]> Errors { get; }

        public CodeErrorException (HttpStatusCode statusCode, string? message, string? details)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessageStatusCode(statusCode);
            Details = details;
            Errors = new Dictionary<string, string[]>();
        }

        public CodeErrorException(HttpStatusCode statusCode, string? message, IDictionary<string, string[]> errors)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessageStatusCode(statusCode);
            Errors = errors;
        }

        private static string GetDefaultMessageStatusCode(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.BadRequest => "The request could not be processed.",
                HttpStatusCode.Unauthorized => "Access is denied due to invalid credentials.",
                HttpStatusCode.NotFound => "The resource requested could not be found.",
                HttpStatusCode.InternalServerError => "Something went wrong.",
                _ => string.Empty
            };
        }
    }
}
