# DotNetDevKit - Quick Reference Cheat Sheet

## Installation

```bash
dotnet add package DotNetDevKit
```

## Setup in Program.cs

```csharp
using DotNetDevKit;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add DotNetDevKit
builder.Services.AddDotNetDevKit(options =>
{
    options.EnableAutoServiceRegistration = true;
    options.EnableDebugDashboard = true;
    options.EnableGlobalExceptionHandling = true;
});

builder.Services.AddControllers();

var app = builder.Build();

// Use DotNetDevKit middleware
app.UseDotNetDevKit();

app.MapControllers();
app.Run();
```

## Auto-Register Services

```csharp
using DotNetDevKit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

// Default (Transient)
[AutoRegisterService]
public class EmailService : IEmailService { }

// Scoped
[AutoRegisterService(ServiceLifetime.Scoped)]
public class UserRepository : IUserRepository { }

// Singleton
[AutoRegisterService(ServiceLifetime.Singleton)]
public class CacheService : ICacheService { }

// Explicit interface
[AutoRegisterService(typeof(ICustomService))]
public class CustomServiceImpl : ICustomService { }

// Register as self (no interface)
[AutoRegisterService(AsSelf = true)]
public class UtilityService { }
```

## API Responses

### Success Response

```csharp
using DotNetDevKit.ApiResponse;

[HttpGet("{id}")]
public ActionResult<ApiResponse<User>> GetUser(int id)
{
    var user = GetUserData(id);
    return Ok(ApiResponse<User>.Success(
        user, 
        "User retrieved successfully",
        200
    ));
}
```

### Error Response

```csharp
[HttpGet("{id}")]
public ActionResult<ApiResponse<User>> GetUser(int id)
{
    if (id <= 0)
        return BadRequest(ApiResponse<User>.Failure(
            "Invalid user ID", 
            400
        ));
    
    return Ok(ApiResponse<User>.Success(data));
}
```

### Validation Errors

```csharp
var errors = new Dictionary<string, string[]>
{
    { "Email", new[] { "Email is required", "Invalid format" } },
    { "Password", new[] { "Password must be at least 8 characters" } }
};

return BadRequest(ApiResponse<User>.ValidationError(errors));
```

### Exception Response

```csharp
try
{
    // Do something
}
catch (Exception ex)
{
    return StatusCode(500, ApiResponse<User>.Exception(
        ex,
        "An error occurred",
        500
    ));
}
```

### Paginated Response

```csharp
[HttpGet]
public ActionResult<PaginatedApiResponse<User>> GetUsers(
    [FromQuery] int page = 1, 
    [FromQuery] int pageSize = 10)
{
    var users = _repository.GetAll();
    
    return Ok(PaginatedApiResponse<User>.Success(
        users.Skip((page-1)*pageSize).Take(pageSize).ToList(),
        page,
        pageSize,
        users.Count
    ));
}
```

## Extension Methods

```csharp
// Convert to ApiResponse
var response = user.AsApiResponse("Success");

// Error response
var errorResponse = "Error message".AsErrorResponse<User>(500);

// Exception response
var exceptionResponse = ex.AsProblemResponse<User>();

// Check if valid
if (response.IsValid()) { }

// Get all errors
var errors = response.GetErrorMessages();
```

## Dashboard Endpoints

All endpoints return standardized `ApiResponse`:

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/dev/dashboard/info` | GET | Complete dashboard info |
| `/api/dev/dashboard/application` | GET | Application details |
| `/api/dev/dashboard/system` | GET | System info & memory |
| `/api/dev/dashboard/services` | GET | Registered services |
| `/api/dev/dashboard/assemblies` | GET | Loaded assemblies |
| `/api/dev/dashboard/environment` | GET | Environment variables |
| `/api/dev/dashboard/auto-registered` | GET | Auto-registered services |
| `/api/dev/dashboard/health` | GET | Health check |

### Example Request

```bash
curl -X GET "https://localhost:7000/api/dev/dashboard/health"
```

### Example Response

```json
{
  "success": true,
  "statusCode": 200,
  "message": "Application is healthy",
  "data": {
    "status": "Healthy",
    "timestamp": "2024-01-15T10:30:00Z",
    "uptime": "00:05:23",
    "services": 12,
    "memory": {
      "totalMemoryMB": 512,
      "workingSetMB": 256
    }
  },
  "traceId": "0HN3F5G7K9J1M3P5"
}
```

## Configuration Options

```csharp
builder.Services.AddDotNetDevKit(options =>
{
    // Features
    options.EnableAutoServiceRegistration = true;
    options.EnableApiResponseWrapper = true;
    options.EnableDebugDashboard = true;
    options.EnableGlobalExceptionHandling = true;
    
    // Scanning
    options.AssembliesToScan = new[] { Assembly.GetExecutingAssembly() };
    options.NamespaceFilter = "MyApp.Services";
    
    // Routes
    options.DashboardRoutePrefix = "api/dev/dashboard";
    
    // Environment
    options.IsDevelopmentEnvironment = true;
});
```

## Full Controller Example

```csharp
using DotNetDevKit.ApiResponse;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _service;

    public ItemsController(IItemService service) => _service = service;

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Item>>> Get(int id)
    {
        try
        {
            var item = await _service.GetAsync(id);
            return Ok(item.AsApiResponse());
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<Item>.Failure("Not found", 404));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Item>>> Create(CreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse<Item>.ValidationError(
                new() { { "Name", new[] { "Required" } } }
            ));

        var item = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(Get), new { id = item.Id },
            ApiResponse<Item>.Success(item, message: "Created", statusCode: 201)
        );
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedApiResponse<Item>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var items = await _service.GetAllAsync();
        return Ok(PaginatedApiResponse<Item>.Success(
            items.Skip((page-1)*pageSize).Take(pageSize).ToList(),
            page, pageSize, items.Count
        ));
    }
}
```

## Publishing Quick Steps

```bash
# 1. Build
dotnet build -c Release

# 2. Pack
dotnet pack -c Release

# 3. Push (with your API key)
dotnet nuget push bin/Release/DotNetDevKit.1.0.0.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json

# 4. Verify
# Visit: https://www.nuget.org/packages/DotNetDevKit
```

## Common Patterns

### Try-Catch-Respond Pattern

```csharp
[HttpPost]
public async Task<ActionResult<ApiResponse<T>>> Create(CreateRequest request)
{
    try
    {
        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<T>.Success(result, message: "Created", statusCode: 201));
    }
    catch (ValidationException vex)
    {
        return BadRequest(ApiResponse<T>.ValidationError(vex.Errors));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in Create");
        return StatusCode(500, ApiResponse<T>.Exception(ex));
    }
}
```

### Dependency Injection Pattern

```csharp
public interface IEmailService { Task SendAsync(string to, string body); }

[AutoRegisterService(ServiceLifetime.Singleton)]
public class EmailService : IEmailService
{
    public async Task SendAsync(string to, string body)
    {
        // Implementation
        await Task.Delay(100);
    }
}

public class UserController : ControllerBase
{
    private readonly IEmailService _emailService;

    public UserController(IEmailService emailService)
    {
        _emailService = emailService;
    }
}
```

### Pagination Pattern

```csharp
[HttpGet]
public ActionResult<PaginatedApiResponse<T>> GetList(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
{
    var items = GetItems();
    var total = items.Count;
    var pageItems = items
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    return Ok(PaginatedApiResponse<T>.Success(pageItems, page, pageSize, total));
}
```

## Testing Response

```csharp
[TestFixture]
public class ApiResponseTests
{
    [Test]
    public void Success_ReturnsCorrectStructure()
    {
        var response = ApiResponse<string>.Success("data", "Success");
        
        Assert.IsTrue(response.Success);
        Assert.AreEqual(200, response.StatusCode);
        Assert.AreEqual("data", response.Data);
    }

    [Test]
    public void ValidationError_ContainsErrors()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "field", new[] { "error" } }
        };
        var response = ApiResponse<string>.ValidationError(errors);
        
        Assert.IsFalse(response.Success);
        Assert.AreEqual(422, response.StatusCode);
        Assert.IsNotNull(response.Errors);
    }
}
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Services not registering | Check `[AutoRegisterService]` attribute |
| Dashboard not working | Ensure `EnableDebugDashboard = true` |
| Exception middleware not catching | Add early in middleware pipeline |
| API key invalid | Regenerate from NuGet.org |
| Package not found | Wait 5-10 min for NuGet indexing |

## Useful Links

- 📖 [Full Documentation](README.md)
- 📚 [Examples](EXAMPLES.md)
- 🚀 [Publishing Guide](PUBLISHING.md)
- ⚙️ [Setup Instructions](SETUP.md)
- 📦 [NuGet.org](https://www.nuget.org/packages/DotNetDevKit)

---

**Print this page for quick reference!** 📋
