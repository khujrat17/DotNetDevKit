using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using DotNetDevKit.ApiResponse;
using DotNetDevKit.DependencyInjection;
using DotNetDevKit.Dashboard;

namespace DotNetDevKit
{
    /// <summary>
    /// DotNetDevKit configuration options
    /// </summary>
    public class DotNetDevKitOptions
    {
        /// <summary>
        /// Enable automatic service registration
        /// </summary>
        public bool EnableAutoServiceRegistration { get; set; } = true;

        /// <summary>
        /// Enable API response wrapping middleware
        /// </summary>
        public bool EnableApiResponseWrapper { get; set; } = true;

        /// <summary>
        /// Enable development debug dashboard
        /// </summary>
        public bool EnableDebugDashboard { get; set; } = true;

        /// <summary>
        /// Enable global exception handling middleware
        /// </summary>
        public bool EnableGlobalExceptionHandling { get; set; } = true;

        /// <summary>
        /// Assemblies to scan for auto-registration (if null, scans all)
        /// </summary>
        public Assembly[]? AssembliesToScan { get; set; }

        /// <summary>
        /// Namespace to scan for auto-registration (optional)
        /// </summary>
        public string? NamespaceFilter { get; set; }

        /// <summary>
        /// Dashboard route prefix
        /// </summary>
        public string DashboardRoutePrefix { get; set; } = "api/dev/dashboard";

        /// <summary>
        /// Whether this is a development environment
        /// </summary>
        public bool IsDevelopmentEnvironment { get; set; } = true;
    }

    /// <summary>
    /// DotNetDevKit extension methods for IServiceCollection and IApplicationBuilder
    /// </summary>
    public static class DotNetDevKitExtensions
    {
        /// <summary>
        /// Add DotNetDevKit services to the service collection
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="configure">Optional configuration action</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddDotNetDevKit(
            this IServiceCollection services,
            Action<DotNetDevKitOptions>? configure = null)
        {
            var options = new DotNetDevKitOptions();
            configure?.Invoke(options);

            // Auto service registration
            if (options.EnableAutoServiceRegistration)
            {
                if (!string.IsNullOrEmpty(options.NamespaceFilter))
                {
                    services.AddAutoRegisteredServicesFromNamespace(
                        options.NamespaceFilter,
                        options.AssembliesToScan ?? Array.Empty<Assembly>());
                }
                else
                {
                    services.AddAutoRegisteredServices(
                        options.AssembliesToScan ?? Array.Empty<Assembly>());
                }
            }

            // Register dashboard controller if enabled
            if (options.EnableDebugDashboard)
            {
                services.AddControllers().AddApplicationPart(typeof(DevDebugDashboardController).Assembly);
            }

            return services;
        }

        /// <summary>
        /// Use DotNetDevKit middleware and features
        /// </summary>
        /// <param name="app">IApplicationBuilder</param>
        /// <param name="configure">Optional configuration action</param>
        /// <returns>IApplicationBuilder</returns>
        public static IApplicationBuilder UseDotNetDevKit(
            this IApplicationBuilder app,
            Action<DotNetDevKitOptions>? configure = null)
        {
            var options = new DotNetDevKitOptions();
            configure?.Invoke(options);

            // Global exception handling middleware
            if (options.EnableGlobalExceptionHandling)
            {
                app.UseMiddleware<ApiExceptionHandlingMiddleware>(options.IsDevelopmentEnvironment);
            }

            // API response wrapper middleware (optional)
            if (options.EnableApiResponseWrapper)
            {
                app.UseApiResponseWrapper();
            }

            return app;
        }

        /// <summary>
        /// Use API response wrapper middleware
        /// </summary>
        private static IApplicationBuilder UseApiResponseWrapper(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                await next();
            });

            return app;
        }

        /// <summary>
        /// Quick setup with sensible defaults
        /// </summary>
        public static void UseDotNetDevKitQuick(
            this IServiceCollection services,
            IApplicationBuilder app,
            Assembly? assemblyToScan = null)
        {
            var asm = assemblyToScan ?? Assembly.GetCallingAssembly();

            services.AddDotNetDevKit(opts =>
            {
                opts.AssembliesToScan = new[] { asm };
                opts.EnableAutoServiceRegistration = true;
                opts.EnableDebugDashboard = true;
                opts.EnableGlobalExceptionHandling = true;
                opts.IsDevelopmentEnvironment = true;
            });

            app.UseDotNetDevKit(opts =>
            {
                opts.IsDevelopmentEnvironment = true;
            });
        }
    }
}
