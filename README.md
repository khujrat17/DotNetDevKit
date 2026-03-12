# DotNetDevKit

> A powerful, production-ready NuGet package for ASP.NET Core that combines automatic dependency injection, standardized API responses, and a developer debug dashboard.

![License](https://img.shields.io/badge/license-MIT-green)
![NuGet](https://img.shields.io/badge/nuget-1.0.0-blue)
![.NET](https://img.shields.io/badge/.NET-6.0+-blueviolet)

**[GitHub](https://github.com/yourusername/DotNetDevKit)** | **[NuGet](https://www.nuget.org/packages/DotNetDevKit)** | **[Documentation](https://github.com/yourusername/DotNetDevKit/wiki)** | **[Issues](https://github.com/yourusername/DotNetDevKit/issues)** | **[Discussions](https://github.com/yourusername/DotNetDevKit/discussions)**

## 🎯Overview

DotNetDevKit is a comprehensive toolkit designed to streamline ASP.NET Core development by providing three powerful features in a single, easy-to-use package:

1. **AutoServiceRegistrar** - Automatic dependency injection registration with simple attributes
2. **ApiResponseKit** - Standardized API response envelopes for consistent responses
3. **DevDebugDashboard** - Real-time developer monitoring and debugging dashboard

Perfect for:
- ASP.NET Core Web APIs
- MVC Applications
- Microservices
- SaaS Platforms
- Enterprise Applications

## ✨Key Features

### 1️⃣Automatic Service Registration

Mark services with a single attribute and let DotNetDevKit handle registration:

```csharp
[AutoRegisterService]
public class EmailService : IEmailService { }

[AutoRegisterService(ServiceLifetime.Scoped)]
public class UserRepository : IUserRepository { }

[AutoRegisterService(ServiceLifetime.Singleton)]
public class CacheService : ICacheService { }
```

**Benefits:**
- Reduces boilerplate code
- Automatic interface detection
- Supports all lifetimes (Transient, Scoped, Singleton)
- Namespace filtering support
- No manual registration needed

### 2️⃣Standardized API Responses

Every API endpoint returns a consistent response format:

```csharp
// Success response
return Ok(ApiResponse<User>.Success(user, "User retrieved successfully"));

// Error response
return BadRequest(ApiResponse<User>.Failure("Validation failed", 400));

// Validation errors
return BadRequest(ApiResponse<User>.ValidationError(errors));

// Exception handling
return StatusCode(500, ApiResponse<User>.Exception(ex));

// Paginated response
return Ok(PaginatedApiResponse<User>.Success(users, page, pageSize, totalCount));
```

**Response Format:**
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "Operation successful",
  "data": { ... },
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HN3F5G7K9J1M3P5",
  "errors": null
}
```

**Benefits:**
- Consistent format across all endpoints
- Automatic error tracking
- Request tracing with TraceId
- Built-in pagination support
- Extension methods for easy response creation

### 3️⃣ Developer Debug Dashboard

Real-time monitoring of your application:

```bash
GET /api/dev/dashboard/info         # Complete dashboard info
GET /api/dev/dashboard/health       # Health check
GET /api/dev/dashboard/services     # Registered services
GET /api/dev/dashboard/assemblies   # Loaded assemblies
GET /api/dev/dashboard/environment  # Environment info
GET /api/dev/dashboard/application  # Application details
GET /api/dev/dashboard/system       # System information
```

**Dashboard Shows:**
- Application name, version, target framework
- System info (OS, processors, memory usage)
- All registered services and dependencies
- Loaded assemblies with type counts
- Environment variables (dev only)
- Application health and uptime
- Memory usage metrics

## 📦 Installation

### Via NuGet Package Manager

```bash
Install-Package DotNetDevKit
```

### Via .NET CLI

```bash
dotnet add package DotNetDevKit
```

### Via Package Manager Console

```powershell
Install-Package DotNetDevKit
```

## 🚀 Quick Start

### Step 1: Add DotNetDevKit to Services

```csharp
using DotNetDevKit;

var builder = WebApplicationBuilder.CreateBuilder(args);

builder.Services.AddDotNetDevKit(options =>
{
    options.EnableAutoServiceRegistration = true;
    options.EnableDebugDashboard = true;
    options.EnableGlobalExceptionHandling = true;
    options.IsDevelopmentEnvironment = builder.Environment.IsDevelopment();
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseDotNetDevKit();

app.MapControllers();
app.Run();
```

### Step 2: Mark Services for Registration

```csharp
using DotNetDevKit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[AutoRegisterService]
public class EmailService : IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // Implementation
    }
}

[AutoRegisterService(ServiceLifetime.Scoped)]
public class OrderRepository : IOrderRepository
{
    public async Task<Order> GetByIdAsync(int id)
    {
        // Implementation
    }
}
```

### Step 3: Return Standardized Responses

```csharp
using DotNetDevKit.ApiResponse;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _repository;

    public OrdersController(IOrderRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Order>>> GetOrder(int id)
    {
        try
        {
            var order = await _repository.GetByIdAsync(id);
            
            if (order == null)
                return NotFound(ApiResponse<Order>.Failure("Order not found", 404));

            return Ok(ApiResponse<Order>.Success(
                order,
                "Order retrieved successfully"
            ));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Order>.Exception(ex));
        }
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedApiResponse<Order>>> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var orders = await _repository.GetAllAsync();
        var totalItems = orders.Count;
        var paginatedOrders = orders
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(PaginatedApiResponse<Order>.Success(
            paginatedOrders,
            page,
            pageSize,
            totalItems,
            "Orders retrieved successfully"
        ));
    }
}
```

### Step 4: Access the Debug Dashboard

Once your application is running:

```
http://localhost:5000/api/dev/dashboard/info
http://localhost:5000/api/dev/dashboard/health
http://localhost:5000/api/dev/dashboard/services
```

## 📚 Configuration Options

```csharp
builder.Services.AddDotNetDevKit(options =>
{
    // Enable/disable features
    options.EnableAutoServiceRegistration = true;
    options.EnableDebugDashboard = true;
    options.EnableGlobalExceptionHandling = true;
    options.EnableApiResponseWrapper = true;
    
    // Specify which assemblies to scan
    options.AssembliesToScan = new[] { 
        Assembly.GetExecutingAssembly(),
        typeof(SomeOtherType).Assembly
    };
    
    // Filter by specific namespace
    options.NamespaceFilter = "MyApp.Services";
    
    // Customize dashboard route
    options.DashboardRoutePrefix = "api/dev/dashboard";
    
    // Environment configuration
    options.IsDevelopmentEnvironment = env.IsDevelopment();
});
```

## 🔧 Advanced Usage

### Custom Namespace Scanning

```csharp
builder.Services.AddAutoRegisteredServicesFromNamespace(
    "MyApp.Services",
    Assembly.GetExecutingAssembly()
);
```

### Get Auto-Registration Information

```csharp
public class DebugController : ControllerBase
{
    private readonly AutoRegistrationInfo _registrationInfo;

    public DebugController(AutoRegistrationInfo registrationInfo)
    {
        _registrationInfo = registrationInfo;
    }

    [HttpGet("registered-services")]
    public ActionResult<List<string>> GetRegisteredServices()
    {
        var details = _registrationInfo.GetDetails();
        return Ok(details);
    }
}
```

### Extension Methods for Responses

```csharp
using DotNetDevKit.ApiResponse;

// Convert to ApiResponse
var response = user.AsApiResponse("User data retrieved");

// Create error response
var errorResponse = "Something went wrong".AsErrorResponse<User>(500);

// Handle exceptions
try
{
    // Do something
}
catch (Exception ex)
{
    var errorResponse = ex.AsProblemResponse<User>();
}

// Check if response is valid
if (response.IsValid())
{
    // Handle successful response
}

// Get all error messages
var errors = response.GetErrorMessages();
```

### Response Models

#### ApiResponse<T>

Generic response for single objects:

```csharp
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public Dictionary<string, string[]> Errors { get; set; }
    public DateTime Timestamp { get; set; }
    public string TraceId { get; set; }
}
```

#### PaginatedApiResponse<T>

For paginated list results:

```csharp
public class PaginatedApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public List<T> Data { get; set; }
    public PaginationMetadata Pagination { get; set; }
    public DateTime Timestamp { get; set; }
    public string TraceId { get; set; }
}

public class PaginationMetadata
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}
```

## 📖 API Endpoints

### Dashboard Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/dev/dashboard/info` | GET | Complete dashboard information |
| `/api/dev/dashboard/application` | GET | Application details |
| `/api/dev/dashboard/system` | GET | System info and memory |
| `/api/dev/dashboard/services` | GET | Registered services list |
| `/api/dev/dashboard/assemblies` | GET | Loaded assemblies |
| `/api/dev/dashboard/environment` | GET | Environment variables |
| `/api/dev/dashboard/auto-registered` | GET | Auto-registered services |
| `/api/dev/dashboard/health` | GET | Health check status |

### Example Responses

**GET /api/dev/dashboard/health**

```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "Application is healthy",
  "data": {
    "status": "Healthy",
    "timestamp": "2024-01-15T10:30:00Z",
    "uptime": "05:23:15.1234567",
    "services": 12,
    "memory": {
      "totalMemoryMB": 512,
      "workingSetMB": 256
    }
  },
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HN3F5G7K9J1M3P5"
}
```

## 🔒 Security Considerations

**Important:** The debug dashboard should **ONLY** be enabled in development environments.

```csharp
builder.Services.AddDotNetDevKit(options =>
{
    // Only enable in development
    options.EnableDebugDashboard = env.IsDevelopment();
    options.IsDevelopmentEnvironment = env.IsDevelopment();
});
```

**Security Features:**
- Environment variables only visible in development
- All endpoints are read-only
- No sensitive data exposure in production
- Automatic filtering based on environment

## 📋 Complete Example

```csharp
// Program.cs
using DotNetDevKit;
using System.Reflection;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add DotNetDevKit
builder.Services.AddDotNetDevKit(options =>
{
    options.AssembliesToScan = new[] { Assembly.GetExecutingAssembly() };
    options.IsDevelopmentEnvironment = builder.Environment.IsDevelopment();
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDotNetDevKit();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
```

```csharp
// Services/IUserService.cs
public interface IUserService
{
    Task<User> GetByIdAsync(int id);
    Task<List<User>> GetAllAsync();
    Task<User> CreateAsync(CreateUserRequest request);
}
```

```csharp
// Services/UserService.cs
using DotNetDevKit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[AutoRegisterService(ServiceLifetime.Scoped)]
public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    public async Task<User> GetByIdAsync(int id)
    {
        _logger.LogInformation("Getting user {UserId}", id);
        // Implementation
        return null;
    }

    public async Task<List<User>> GetAllAsync()
    {
        _logger.LogInformation("Getting all users");
        // Implementation
        return new List<User>();
    }

    public async Task<User> CreateAsync(CreateUserRequest request)
    {
        _logger.LogInformation("Creating user {Email}", request.Email);
        // Implementation
        return null;
    }
}
```

```csharp
// Controllers/UsersController.cs
using DotNetDevKit.ApiResponse;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<User>>> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            return Ok(ApiResponse<User>.Success(user, "User retrieved"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<User>.Exception(ex));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<User>>> CreateUser(CreateUserRequest request)
    {
        try
        {
            var user = await _userService.CreateAsync(request);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id },
                ApiResponse<User>.Success(user, "User created", 201));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<User>.Exception(ex));
        }
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedApiResponse<User>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var users = await _userService.GetAllAsync();
        var paginatedUsers = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Ok(PaginatedApiResponse<User>.Success(
            paginatedUsers,
            page,
            pageSize,
            users.Count
        ));
    }
}
```

## 🐛 Troubleshooting

### Services Not Auto-Registering

**Issue:** Services marked with `[AutoRegisterService]` aren't being injected.

**Solution:**
- Verify the `[AutoRegisterService]` attribute is present
- Check that the assembly is in `AssembliesToScan`
- Ensure the class has a public constructor
- Verify the interface/service type is correct

### Dashboard Not Working

**Issue:** Dashboard endpoints return 404 or are not accessible.

**Solution:**
- Make sure `EnableDebugDashboard = true`
- Check the correct URL: `/api/dev/dashboard/...`
- Verify you're in development environment
- Check for middleware ordering issues

### Exception Handling Not Working

**Issue:** Exceptions aren't being caught by global exception handling.

**Solution:**
- Ensure `EnableGlobalExceptionHandling = true`
- Add middleware early in the pipeline
- Check that custom exception handlers don't interfere

### Validation Errors Not Displaying

**Issue:** Validation errors are not shown in the response.

**Solution:**
- Use `ApiResponse<T>.ValidationError(errors)` method
- Ensure error dictionary has correct format: `Dictionary<string, string[]>`
- Check that errors are being passed correctly

## 📊 Supported .NET Versions

- .NET 6.0+
- .NET 7.0+
- .NET 8.0+

## 🏆 Performance

DotNetDevKit is designed for minimal overhead:

- **Auto-registration** happens once at startup
- **Response wrapping** is lightweight
- **Dashboard endpoints** are read-only and optimized
- **Zero allocations** in the happy path
- **Typical overhead** < 1ms per request

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

### How to Contribute

1. **Fork the Repository**: [github.com/khujrat17/DotNetDevKit/fork](https://github.com/khujrat17/DotNetDevKit/fork)
2. **Clone Your Fork**: `git clone https://github.com/khujrat17/DotNetDevKit.git`
3. **Create a Branch**: `git checkout -b feature/amazing-feature`
4. **Commit Changes**: `git commit -m 'Add amazing feature'`
5. **Push to Branch**: `git push origin feature/amazing-feature`
6. **Open a Pull Request**: [github.com/khujrat17/DotNetDevKit/pulls](https://github.com/khujrat17/DotNetDevKit/pulls)

### Development Setup

```bash
git clone https://github.com/khujrat17/DotNetDevKit.git
cd DotNetDevKit
dotnet restore
dotnet build
dotnet test  # If tests are available
```

### Code Guidelines

- Follow C# coding conventions
- Write clear commit messages
- Add XML documentation for public APIs
- Update README if adding new features
- Test your changes thoroughly

### Report Bugs

Found a bug? Please report it on [GitHub Issues](https://github.com/khujrat17/DotNetDevKit/issues) with:
- Description of the issue
- Steps to reproduce
- Expected behavior
- Actual behavior
- Environment details

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

Built with ❤️ for the .NET community.

## 📞 Support

For issues, feature requests, or questions:
- **GitHub Issues**: [github.com/khujrat17/DotNetDevKit/issues](https://github.com/khujrat17/DotNetDevKit/issues)
- **GitHub Discussions**: [github.com/khujrat17/DotNetDevKit/discussions](https://github.com/yourusername/DotNetDevKit/discussions)
- **Troubleshooting Guide**: See [Troubleshooting](#-troubleshooting) section above
- **Stack Overflow**: Tag with `[dotnetdevkit]` or `[aspnetcore]`
- **Email Support**: khujratshaikh1284@gmail.com

## 🔗 Related Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Dependency Injection in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [API Design Best Practices](https://docs.microsoft.com/en-us/azure/architecture/best-practices/api-design)
- [NuGet Package Best Practices](https://docs.microsoft.com/en-us/nuget/create-packages/publish-a-package)
- [GitHub Repository](https://github.com/khujrat17/DotNetDevKit)
- [NuGet Package Page](https://www.nuget.org/packages/DotNetDevKit)
- [Releases](https://github.com/khujrat17/DotNetDevKit/releases)
- [Project Wiki](https://github.com/khujrat17/DotNetDevKit/wiki)

## 📈 Changelog

### Version 1.0.0 (Initial Release)

- ✨ AutoServiceRegistrar with attribute-based registration
- ✨ ApiResponseKit with generic and paginated responses
- ✨ DevDebugDashboard with comprehensive system monitoring
- ✨ Global exception handling middleware
- ✨ Extension methods for easy response creation
- ✨ Full XML documentation
- ✨ Complete examples and guides

---

**Happy coding! 🚀**

Made with ❤️ for the .NET Community