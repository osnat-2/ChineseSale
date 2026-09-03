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
            var method = context.Request.Method;
            var path = context.Request.Path;

            // שליפת שם המשתמש (אם הוא מחובר) - אם לא, ייכתב Anonymous
            var user = context.User?.Identity?.Name ?? "Anonymous";

            // עיצוב תאריך ושעה בצורה נקייה (למשל: 2026-05-18 19:15:30)
            var currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // 1. הדפסת לוג ברגע שנכנסת קריאה
            _logger.LogInformation("Request executed: {Method} {Path} by {User} at {Date}", method, path, user, currentDateTime);

            await _next(context);

            var statusCode = context.Response.StatusCode;

            // 2. הדפסת לוג כשהקריאה מסתיימת (אם אתה רוצה לתעד גם את הסטטוס)
            _logger.LogInformation("Response finished: {Method} {Path} with Status {Status} by {User} at {Date}", method, path, statusCode, user, currentDateTime);
        }
    }
}
