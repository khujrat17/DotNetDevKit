using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DotNetDevKit.ApiResponse
{
    /// <summary>
    /// Extension methods for API responses
    /// </summary>
    public static class ApiResponseExtensions
    {
        /// <summary>
        /// Convert any object to ApiResponse
        /// </summary>
        public static ApiResponse<T> AsApiResponse<T>(this T data, string message = "Operation successful", int statusCode = 200, string? traceId = null)
        {
            return ApiResponse<T>.Success(data, message, statusCode, traceId);
        }

        /// <summary>
        /// Create an error response
        /// </summary>
        public static ApiResponse<T> AsErrorResponse<T>(this string message, int statusCode = 400, string? traceId = null)
        {
            return ApiResponse<T>.Failure(message, statusCode, traceId);
        }

        /// <summary>
        /// Create a problem details response
        /// </summary>
        public static ApiResponse<T> AsProblemResponse<T>(this Exception exception, string message = "An error occurred", int statusCode = 500, string? traceId = null)
        {
            return ApiResponse<T>.Exception(exception, message, statusCode, traceId);
        }

        /// <summary>
        /// Check if response is valid
        /// </summary>
        public static bool IsValid<T>(this ApiResponse<T> response)
        {
            return response.IsSuccess && response.StatusCode >= 200 && response.StatusCode < 300;
        }

        /// <summary>
        /// Get error messages from response
        /// </summary>
        public static List<string> GetErrorMessages<T>(this ApiResponse<T> response)
        {
            var errors = new List<string>();

            if (!response.IsSuccess && !string.IsNullOrEmpty(response.Message))
                errors.Add(response.Message);

            if (response.Errors != null)
                errors.AddRange(response.Errors.Values.SelectMany(e => e));

            return errors;
        }
    }

    /// <summary>
    /// Global exception handling middleware for standardized error responses
    /// </summary>
    public class ApiExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly bool _isDevelopment;

        public ApiExceptionHandlingMiddleware(RequestDelegate next, bool isDevelopment = false)
        {
            _next = next;
            _isDevelopment = isDevelopment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var traceId = context.TraceIdentifier;

            ApiResponse errorResponse;

            if (exception is ArgumentException argEx)
            {
                context.Response.StatusCode = 400;
                errorResponse = ApiResponse.Failure(argEx.Message, 400, traceId);
            }
            else if (exception is UnauthorizedAccessException)
            {
                context.Response.StatusCode = 401;
                errorResponse = ApiResponse.Failure("Unauthorized", 401, traceId);
            }
            else if (exception is KeyNotFoundException)
            {
                context.Response.StatusCode = 404;
                errorResponse = ApiResponse.Failure("Resource not found", 404, traceId);
            }
            else
            {
                context.Response.StatusCode = 500;
                var message = "An internal server error occurred";
                errorResponse = ApiResponse.Failure(message, 500, traceId);
            }

            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            await context.Response.WriteAsJsonAsync(errorResponse, options);
        }
    }
}
