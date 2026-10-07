using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Project.Middlewares;
using Xunit;

namespace Project.Tests;

public class GlobalExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task UnhandledException_ReturnsSanitizedProblemDetails()
    {
        var context = new DefaultHttpContext();
        context.TraceIdentifier = "trace-test-123";
        context.Request.Path = "/sensitive/path";
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var middleware = new GlobalExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("sensitive exception detail"),
            NullLogger<GlobalExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);

        responseBody.Position = 0;
        using var response = await JsonDocument.ParseAsync(responseBody);
        var responseText = response.RootElement.GetRawText();

        Assert.Contains("An unexpected error occurred.", responseText);
        Assert.Contains("trace-test-123", responseText);
        Assert.DoesNotContain("sensitive exception detail", responseText);
        Assert.DoesNotContain("/sensitive/path", responseText);
    }
}
