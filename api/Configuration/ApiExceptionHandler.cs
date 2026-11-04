using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Configuration;

public class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
{
    public async Task HandleAsync(HttpContext context)
    {
        //get exception
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var badInput = exception is ArgumentException;
        if (!badInput) logger.LogError(exception, "Request {RequestId} failed", context.TraceIdentifier);

        //build problem details
        var problem = new ProblemDetails
        {
            Status = badInput ? 400 : 500,
            Title = badInput ? "Invalid request" : "An unexpected error occurred",
            Detail = badInput ? exception?.Message : null
        };
        problem.Extensions["requestId"] = context.TraceIdentifier;
        context.Response.StatusCode = problem.Status.Value;
        //return problem details
        await context.Response.WriteAsJsonAsync(problem);
    }
}
