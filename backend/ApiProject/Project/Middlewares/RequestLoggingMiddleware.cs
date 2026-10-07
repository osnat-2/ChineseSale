namespace Project.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        // הזרקת ה-Logger וה-Middleware הבא בתור בבנאי
        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint()?.DisplayName ?? "unmatched";
            var traceId = context.TraceIdentifier;

            _logger.LogInformation(
                "HTTP request started. TraceId: {TraceId}, Method: {Method}, Endpoint: {Endpoint}",
                traceId,
                context.Request.Method,
                endpoint);

            try
            {
                await _next(context);
            }
            finally
            {
                _logger.LogInformation(
                    "HTTP request finished. TraceId: {TraceId}, StatusCode: {StatusCode}",
                    traceId,
                    context.Response.StatusCode);
            }
        }
    }
}
