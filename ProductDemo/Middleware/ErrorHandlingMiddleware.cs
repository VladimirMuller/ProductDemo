
using Newtonsoft.Json;
using System.Net;

namespace ProductDemo.Middleware
{
    /// <summary>
    /// Usage:
    /// In Program.cs inside ConfigureService method for ILogger
    /// services.AddTransient<ErrorHandlingMiddleware>();
    /// 
    /// In Program.cs inside Configure method
    /// app.UseMiddleware<ErrorHandlingMiddleware>();
    /// </summary>
    public class ErrorHandlingMiddleware : IMiddleware
    {
        //private readonly ILogger myLogger;
        //public ErrorHandlingMiddleware(ILogger logger)
        //{
        //    myLogger = logger;
        //}

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                //myLogger.LogError(ex, "Exception");
                await HandleExcetionAsync(context, ex);
            }
        }

        private static Task HandleExcetionAsync(HttpContext context, Exception exception)
        {
            var result = JsonConvert.SerializeObject(new { error = exception.Message });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return context.Response.WriteAsJsonAsync(result);
        }
    }
}
