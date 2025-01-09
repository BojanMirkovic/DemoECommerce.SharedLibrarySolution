using eCommerce.SharedLibrary.LogFolder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace eCommerce.SharedLibrary.Middleware
{
    public class GlobalException(RequestDelegate next)//RequestDelegate next represents the next middleware in the pipeline.
    {
        public async Task InvokeAsync(HttpContext context)
        {
            // Declare default variables for error response
            string message = "Sorry, internal server error occurred. Kindly";
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string title = "Error";
            try
            {
                // Pass the HTTP context to the next middleware in the pipeline
                await next(context);

                // Check if the response status code is 429 (Too Many Requests)
                if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                {
                    title = "Warning";// Set error title
                    message = "Too many request made.";// Friendly error message
                    statusCode = (int)StatusCodes.Status429TooManyRequests;// Update status code
                    await ModifyHeader(context, title, message, statusCode);// Modify the response
                }

                // Check if the response status code is 401 (Unauthorized)
                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    title = "Alert";
                    message = "You are not authorized to access.";
                    await ModifyHeader(context, title, message, statusCode);
                }
                // Check if the response status code is 403 (Forbidden)
                if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    title = "Out of Access";
                    message = "You are not alolowed/required to access.";
                    statusCode = StatusCodes.Status403Forbidden;
                    await ModifyHeader(context, title, message,statusCode);
                }
            }
            catch (Exception ex)
            {

                // Log the exception for debugging and tracking / File,Debugger,Console
                LogException.LogExceptions(ex);

                // Handle specific exception types like TaskCanceledException or TimeoutException, 408 request timeout staus code
                if (ex is TaskCanceledException || ex is TimeoutException)
                {
                    title = " Out of time";
                    message = "Request timeout... try again later";
                    statusCode = StatusCodes.Status408RequestTimeout;
                }
                // For all other exceptions, use default error values
                await ModifyHeader(context, title, message,statusCode);
            }
        }

        private static async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
        {
            // Set the response content type to JSON, display scary-free message to client
            context.Response.ContentType = "application/json";

            // Create a ProblemDetails object to structure the error response
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails 
            {
                Detail = message, // Friendly error message
                Status = statusCode, // HTTP status code
                Title = title // Error title
            }), CancellationToken.None);

            return; // End the response modification
        }
    }
}
