namespace EduAdvisory_Backend.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Log request details (only in Development or with Debug logging)
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Incoming request: {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path
                );

                if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
                {
                    _logger.LogDebug("Authorization header present: {HasAuth}", !string.IsNullOrEmpty(authHeader));
                }
            }

            await _next(context);
        }
    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}