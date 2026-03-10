using System;
using System.Collections.Generic;

namespace DotNetDevKit.ApiResponse
{
    /// <summary>
    /// Standard API Response envelope for all API responses
    /// </summary>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Response data payload
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Validation errors (if any)
        /// </summary>
        public Dictionary<string, string[]>? Errors { get; set; }

        /// <summary>
        /// Timestamp of the response
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Unique request identifier for tracking
        /// </summary>
        public string? TraceId { get; set; }

        /// <summary>
        /// Create a successful response
        /// </summary>
        public static ApiResponse<T> Success(T data, string message = "Operation successful", int statusCode = 200, string? traceId = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                StatusCode = statusCode,
                Message = message,
                Data = data,
                TraceId = traceId
            };
        }

        /// <summary>
        /// Create a failed response
        /// </summary>
        public static ApiResponse<T> Failure(string message, int statusCode = 400, string? traceId = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                StatusCode = statusCode,
                Message = message,
                TraceId = traceId
            };
        }

        /// <summary>
        /// Create a failed response with validation errors
        /// </summary>
        public static ApiResponse<T> ValidationError(Dictionary<string, string[]> errors, string message = "Validation failed", string? traceId = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                StatusCode = 422,
                Message = message,
                Errors = errors,
                TraceId = traceId
            };
        }

        /// <summary>
        /// Create a failed response with exception
        /// </summary>
        public static ApiResponse<T> Exception(Exception ex, string message = "An error occurred", int statusCode = 500, string? traceId = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                StatusCode = statusCode,
                Message = message,
                Errors = new Dictionary<string, string[]>
                {
                    { "Exception", new[] { ex.Message } }
                },
                TraceId = traceId
            };
        }
    }

    /// <summary>
    /// Non-generic version of ApiResponse
    /// </summary>
    public class ApiResponse
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? TraceId { get; set; }

        public static ApiResponse Success(string message = "Operation successful", int statusCode = 200, string? traceId = null)
        {
            return new ApiResponse
            {
                IsSuccess = true,
                StatusCode = statusCode,
                Message = message,
                TraceId = traceId
            };
        }

        public static ApiResponse Failure(string message, int statusCode = 400, string? traceId = null)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                StatusCode = statusCode,
                Message = message,
                TraceId = traceId
            };
        }

        public static ApiResponse ValidationError(Dictionary<string, string[]> errors, string message = "Validation failed", string? traceId = null)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                StatusCode = 422,
                Message = message,
                Errors = errors,
                TraceId = traceId
            };
        }
    }

    /// <summary>
    /// Paginated API response for list results
    /// </summary>
    public class PaginatedApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public List<T>? Data { get; set; }
        public PaginationMetadata? Pagination { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? TraceId { get; set; }

        public static PaginatedApiResponse<T> Success(List<T> data, int page, int pageSize, int totalItems, string message = "Operation successful", string? traceId = null)
        {
            return new PaginatedApiResponse<T>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = message,
                Data = data,
                Pagination = new PaginationMetadata
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
                },
                TraceId = traceId
            };
        }
    }

    /// <summary>
    /// Pagination metadata
    /// </summary>
    public class PaginationMetadata
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }
}
