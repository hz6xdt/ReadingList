using ReadingList.Exceptions;
using ReadingList.Models;
using System.Net;
using System.Text.Json;

namespace ReadingList;

public class ErrorHandlingMiddleware(RequestDelegate requestDelegate, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await requestDelegate(context);
        }
        catch (Exception x)
        {
            await HandleExceptionAsync(context, x);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var result = new ApiErrorResponse()
        {
            Message = "Something went wrong..."
        };

        switch (exception)
        {
            case AuthException x:
                code = HttpStatusCode.Unauthorized;
                result.Message = x.Message;
                result.Errors = x.Errors;
                break;

            case Exception:
                logger.LogError(exception, "\r\n\r\n\r\nSERVER ERROR");
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        string jsonResponse = JsonSerializer.Serialize(result);

        await context.Response.WriteAsync(jsonResponse);
    }
}
