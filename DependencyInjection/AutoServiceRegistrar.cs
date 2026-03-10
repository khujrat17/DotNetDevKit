using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetDevKit.DependencyInjection
{
    /// <summary>
    /// Attribute to mark services for automatic registration
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AutoRegisterServiceAttribute : Attribute
    {
        /// <summary>
        /// Service lifetime
        /// </summary>
        public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;

        /// <summary>
        /// Explicitly define the interface/base class to register (optional)
        /// </summary>
        public Type? ServiceType { get; set; }

        /// <summary>
        /// Register as itself instead of interface
        /// </summary>
        public bool AsSelf { get; set; }

        public AutoRegisterServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            Lifetime = lifetime;
        }

        public AutoRegisterServiceAttribute(Type serviceType, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            ServiceType = serviceType;
            Lifetime = lifetime;
        }
    }

    /// <summary>
    /// Automatic service registration scanner
    /// </summary>
    public static class AutoServiceRegistrar
    {
        /// <summary>
        /// Automatically register all services marked with [AutoRegisterService]
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="scanAssemblies">Assemblies to scan (if null, scans all loaded assemblies)</param>
        /// <returns>Fluent IServiceCollection</returns>
        public static IServiceCollection AddAutoRegisteredServices(
            this IServiceCollection services,
            params Assembly[] scanAssemblies)
        {
            var assemblies = scanAssemblies.Length > 0 ? scanAssemblies : AppDomain.CurrentDomain.GetAssemblies();
            var registeredServices = new List<(Type Implementation, Type? Service, ServiceLifetime Lifetime)>();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.GetCustomAttribute<AutoRegisterServiceAttribute>() != null);

                    foreach (var type in types)
                    {
                        var attribute = type.GetCustomAttribute<AutoRegisterServiceAttribute>();
                        if (attribute == null) continue;

                        Type? serviceType = attribute.ServiceType;

                        // Auto-detect interface if not explicitly set
                        if (serviceType == null && !attribute.AsSelf)
                        {
                            // Look for IServiceName pattern
                            serviceType = type.GetInterfaces()
                                .FirstOrDefault(i => i.Name == $"I{type.Name}");

                            // Fallback to any interface
                            if (serviceType == null && type.GetInterfaces().Length > 0)
                            {
                                serviceType = type.GetInterfaces().First();
                            }
                        }

                        // Register service
                        if (serviceType != null)
                        {
                            RegisterService(services, serviceType, type, attribute.Lifetime);
                            registeredServices.Add((type, serviceType, attribute.Lifetime));
                        }
                        else if (attribute.AsSelf)
                        {
                            RegisterService(services, type, type, attribute.Lifetime);
                            registeredServices.Add((type, null, attribute.Lifetime));
                        }
                    }
                }
                catch
                {
                    // Silently skip problematic assemblies
                }
            }

            // Store registration info for debugging
            services.AddSingleton(new AutoRegistrationInfo(registeredServices));

            return services;
        }

        /// <summary>
        /// Register a single service
        /// </summary>
        private static void RegisterService(IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            var descriptor = new ServiceDescriptor(serviceType, implementationType, lifetime);
            services.Add(descriptor);
        }

        /// <summary>
        /// Scan and register services from a specific namespace
        /// </summary>
        public static IServiceCollection AddAutoRegisteredServicesFromNamespace(
            this IServiceCollection services,
            string @namespace,
            params Assembly[] scanAssemblies)
        {
            var assemblies = scanAssemblies.Length > 0 ? scanAssemblies : AppDomain.CurrentDomain.GetAssemblies();
            var registeredServices = new List<(Type Implementation, Type? Service, ServiceLifetime Lifetime)>();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.Namespace?.StartsWith(@namespace) == true &&
                                    t.GetCustomAttribute<AutoRegisterServiceAttribute>() != null);

                    foreach (var type in types)
                    {
                        var attribute = type.GetCustomAttribute<AutoRegisterServiceAttribute>();
                        if (attribute == null) continue;

                        Type? serviceType = attribute.ServiceType;

                        if (serviceType == null && !attribute.AsSelf)
                        {
                            serviceType = type.GetInterfaces()
                                .FirstOrDefault(i => i.Name == $"I{type.Name}") ??
                                        type.GetInterfaces().FirstOrDefault();
                        }

                        if (serviceType != null)
                        {
                            RegisterService(services, serviceType, type, attribute.Lifetime);
                            registeredServices.Add((type, serviceType, attribute.Lifetime));
                        }
                        else if (attribute.AsSelf)
                        {
                            RegisterService(services, type, type, attribute.Lifetime);
                            registeredServices.Add((type, null, attribute.Lifetime));
                        }
                    }
                }
                catch
                {
                    // Silently skip problematic assemblies
                }
            }

            services.AddSingleton(new AutoRegistrationInfo(registeredServices));

            return services;
        }
    }

    /// <summary>
    /// Information about auto-registered services
    /// </summary>
    public class AutoRegistrationInfo
    {
        public List<(Type Implementation, Type? Service, ServiceLifetime Lifetime)> RegisteredServices { get; }

        public AutoRegistrationInfo(List<(Type Implementation, Type? Service, ServiceLifetime Lifetime)> registeredServices)
        {
            RegisteredServices = registeredServices;
        }

        public int GetCount() => RegisteredServices.Count;

        public List<string> GetDetails()
        {
            return RegisteredServices.Select(s =>
                $"{s.Implementation.Name} -> {(s.Service?.Name ?? "Self")} ({s.Lifetime})"
            ).ToList();
        }
    }
}
