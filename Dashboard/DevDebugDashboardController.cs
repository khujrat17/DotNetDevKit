using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using DotNetDevKit.DependencyInjection;
using DotNetDevKit.ApiResponse;

namespace DotNetDevKit.Dashboard
{
    /// <summary>
    /// Developer debug dashboard information
    /// </summary>
    public class DevelopmentDashboardInfo
    {
        public ApplicationInfo Application { get; set; } = new();
        public SystemInfo System { get; set; } = new();
        public List<ServiceInfo> RegisteredServices { get; set; } = new();
        public List<AssemblyInfo> LoadedAssemblies { get; set; } = new();
        public EnvironmentInfo Environment { get; set; } = new();
    }

    /// <summary>
    /// Application information
    /// </summary>
    public class ApplicationInfo
    {
        public string? Name { get; set; }
        public string? Version { get; set; }
        public string? TargetFramework { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Uptime => DateTime.UtcNow - StartTime;
    }

    /// <summary>
    /// System information
    /// </summary>
    public class SystemInfo
    {
        public string? OSVersion { get; set; }
        public int ProcessorCount { get; set; }
        public long TotalMemory { get; set; }
        public long WorkingSet { get; set; }
        public double MemoryUsagePercent => TotalMemory > 0 ? (WorkingSet / (double)TotalMemory) * 100 : 0;
    }

    /// <summary>
    /// Registered service information
    /// </summary>
    public class ServiceInfo
    {
        public string? ServiceType { get; set; }
        public string? ImplementationType { get; set; }
        public string? Lifetime { get; set; }
    }

    /// <summary>
    /// Loaded assembly information
    /// </summary>
    public class AssemblyInfo
    {
        public string? Name { get; set; }
        public string? Version { get; set; }
        public int TypeCount { get; set; }
    }

    /// <summary>
    /// Environment information
    /// </summary>
    public class EnvironmentInfo
    {
        public string? AspNetCoreEnvironment { get; set; }
        public bool IsDevelopment { get; set; }
        public Dictionary<string, string>? EnvironmentVariables { get; set; }
    }

    /// <summary>
    /// Dashboard controller for development debugging
    /// </summary>
    [ApiController]
    [Route("api/dev/dashboard")]
    public class DevDebugDashboardController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AutoRegistrationInfo? _autoRegistrationInfo;
        private static readonly DateTime ApplicationStartTime = DateTime.UtcNow;

        public DevDebugDashboardController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _autoRegistrationInfo = serviceProvider.GetService<AutoRegistrationInfo>();
        }

        /// <summary>
        /// Get complete dashboard information
        /// </summary>
        [HttpGet("info")]
        public ActionResult<ApiResponse<DevelopmentDashboardInfo>> GetDashboardInfo()
        {
            var info = new DevelopmentDashboardInfo
            {
                Application = BuildApplicationInfo(),
                System = BuildSystemInfo(),
                RegisteredServices = BuildRegisteredServices(),
                LoadedAssemblies = BuildLoadedAssemblies(),
                Environment = BuildEnvironmentInfo()
            };

            return Ok(ApiResponse<DevelopmentDashboardInfo>.Success(
                info,
                "Dashboard information retrieved successfully"
            ));
        }

        /// <summary>
        /// Get application information
        /// </summary>
        [HttpGet("application")]
        public ActionResult<ApiResponse<ApplicationInfo>> GetApplicationInfo()
        {
            var info = BuildApplicationInfo();
            return Ok(ApiResponse<ApplicationInfo>.Success(info, "Application info retrieved"));
        }

        /// <summary>
        /// Get system information
        /// </summary>
        [HttpGet("system")]
        public ActionResult<ApiResponse<SystemInfo>> GetSystemInfoAction()
        {
            var info = BuildSystemInfo();
            return Ok(ApiResponse<SystemInfo>.Success(info, "System info retrieved"));
        }

        /// <summary>
        /// Get registered services
        /// </summary>
        [HttpGet("services")]
        public ActionResult<ApiResponse<List<ServiceInfo>>> GetServicesInfo()
        {
            var services = BuildRegisteredServices();
            return Ok(ApiResponse<List<ServiceInfo>>.Success(
                services,
                $"Found {services.Count} registered services"
            ));
        }

        /// <summary>
        /// Get loaded assemblies
        /// </summary>
        [HttpGet("assemblies")]
        public ActionResult<ApiResponse<List<AssemblyInfo>>> GetAssembliesInfo()
        {
            var assemblies = BuildLoadedAssemblies();
            return Ok(ApiResponse<List<AssemblyInfo>>.Success(
                assemblies,
                $"Found {assemblies.Count} loaded assemblies"
            ));
        }

        /// <summary>
        /// Get environment information
        /// </summary>
        [HttpGet("environment")]
        public ActionResult<ApiResponse<EnvironmentInfo>> GetEnvironmentInfoAction()
        {
            var info = BuildEnvironmentInfo();
            return Ok(ApiResponse<EnvironmentInfo>.Success(info, "Environment info retrieved"));
        }

        /// <summary>
        /// Get auto-registered services
        /// </summary>
        [HttpGet("auto-registered")]
        public ActionResult<ApiResponse<List<string>>> GetAutoRegisteredServices()
        {
            if (_autoRegistrationInfo == null)
                return Ok(ApiResponse<List<string>>.Success(new List<string>(), "No auto-registered services found"));

            var details = _autoRegistrationInfo.GetDetails();
            return Ok(ApiResponse<List<string>>.Success(details, "Auto-registered services"));
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public ActionResult<ApiResponse<object>> HealthCheck()
        {
            var healthData = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Uptime = DateTime.UtcNow - ApplicationStartTime,
                Services = BuildRegisteredServices().Count,
                Memory = new
                {
                    TotalMemoryMB = GC.GetTotalMemory(false) / 1024 / 1024,
                    WorkingSetMB = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024
                }
            };

            return Ok(ApiResponse<object>.Success(healthData, "Application is healthy"));
        }

        // Helper methods
        private static ApplicationInfo BuildApplicationInfo()
        {
            var assembly = Assembly.GetEntryAssembly();
            var version = assembly?.GetName().Version?.ToString() ?? "Unknown";
            var targetFramework = assembly?.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>()
                ?.FrameworkName ?? "Unknown";

            return new ApplicationInfo
            {
                Name = assembly?.GetName().Name ?? "Unknown",
                Version = version,
                TargetFramework = targetFramework,
                StartTime = ApplicationStartTime
            };
        }

        private static SystemInfo BuildSystemInfo()
        {
            var process = Process.GetCurrentProcess();
            return new SystemInfo
            {
                OSVersion = Environment.OSVersion.VersionString,
                ProcessorCount = Environment.ProcessorCount,
                TotalMemory = GC.GetTotalMemory(false),
                WorkingSet = process.WorkingSet64
            };
        }

        private List<ServiceInfo> BuildRegisteredServices()
        {
            var services = new List<ServiceInfo>();

            try
            {
                // Try to get services from service provider (this is a simplified approach)
                var descriptors = _serviceProvider.GetType()
                    .GetProperty("ServiceDescriptors", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(_serviceProvider);

                if (_autoRegistrationInfo != null)
                {
                    foreach (var (impl, service, lifetime) in _autoRegistrationInfo.RegisteredServices)
                    {
                        services.Add(new ServiceInfo
                        {
                            ServiceType = service?.FullName ?? impl.FullName,
                            ImplementationType = impl.FullName,
                            Lifetime = lifetime.ToString()
                        });
                    }
                }
            }
            catch
            {
                // If we can't get services, just return empty list
            }

            return services;
        }

        private static List<AssemblyInfo> BuildLoadedAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .OrderBy(a => a.GetName().Name)
                .Select(a => new AssemblyInfo
                {
                    Name = a.GetName().Name,
                    Version = a.GetName().Version?.ToString(),
                    TypeCount = a.GetTypes().Length
                })
                .ToList();
        }

        private static EnvironmentInfo BuildEnvironmentInfo()
        {
            var env = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            var isDevelopment = env.Equals("Development", StringComparison.OrdinalIgnoreCase);

            return new EnvironmentInfo
            {
                AspNetCoreEnvironment = env,
                IsDevelopment = isDevelopment,
                EnvironmentVariables = isDevelopment
                    ? System.Environment.GetEnvironmentVariables()
                        .Cast<System.Collections.DictionaryEntry>()
                        .Where(e => e.Key.ToString()?.StartsWith("ASPNETCORE", StringComparison.OrdinalIgnoreCase) == true)
                        .ToDictionary(e => e.Key.ToString()!, e => e.Value?.ToString() ?? "")
                    : null
            };
        }
    }
}
