# DotNetDevKit - Complete Examples

This document provides complete, production-ready examples of using DotNetDevKit.

## Example 1: Simple ASP.NET Core API

### Project Structure
```
MyApi/
├── Program.cs
├── Controllers/
│   └── UsersController.cs
├── Services/
│   ├── IUserService.cs
│   └── UserService.cs
├── Models/
│   ├── User.cs
│   └── CreateUserRequest.cs
└── MyApi.csproj
```

### Program.cs

```csharp
using DotNetDevKit;
using System.Reflection;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add DotNetDevKit services
builder.Services.AddDotNetDevKit(options =>
{
    options.EnableAutoServiceRegistration = true;
    options.EnableDebugDashboard = true;
    options.EnableGlobalExceptionHandling = true;
    options.AssembliesToScan = new[] { Assembly.GetExecutingAssembly() };
    options.IsDevelopmentEnvironment = builder.Environment.IsDevelopment();
});

// Add standard services
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Use DotNetDevKit middleware
app.UseDotNetDevKit(options =>
{
    options.IsDevelopmentEnvironment = app.Environment.IsDevelopment();
});

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
```

### Services/IUserService.cs

```csharp
namespace MyApi.Services;

public interface IUserService
{
    Task<User> GetByIdAsync(int id);
    Task<List<User>> GetAllAsync();
    Task<User> CreateAsync(CreateUserRequest request);
    Task UpdateAsync(int id, User user);
    Task DeleteAsync(int id);
}
```

### Services/UserService.cs

```csharp
using DotNetDevKit.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace MyApi.Services;

[AutoRegisterService(ServiceLifetime.Scoped)]
public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IMemoryCache _cache;

    // In-memory storage for demo
    private static readonly List<User> Users = new()
    {
        new User { Id = 1, Name = "John Doe", Email = "john@example.com" },
        new User { Id = 2, Name = "Jane Smith", Email = "jane@example.com" }
    };

    public UserService(ILogger<UserService> logger, IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public async Task<User> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching user {UserId}", id);
        
        var user = Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            throw new KeyNotFoundException($"User {id} not found");

        return await Task.FromResult(user);
    }

    public async Task<List<User>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all users");
        return await Task.FromResult(Users);
    }

    public async Task<User> CreateAsync(CreateUserRequest request)
    {
        _logger.LogInformation("Creating user {Email}", request.Email);
        
        var user = new User
        {
            Id = Users.Max(u => u.Id) + 1,
            Name = request.Name,
            Email = request.Email
        };

        Users.Add(user);
        _cache.Remove("all_users");
        
        return await Task.FromResult(user);
    }

    public async Task UpdateAsync(int id, User user)
    {
        _logger.LogInformation("Updating user {UserId}", id);
        
        var existingUser = Users.FirstOrDefault(u => u.Id == id);
        if (existingUser == null)
            throw new KeyNotFoundException($"User {id} not found");

        existingUser.Name = user.Name;
        existingUser.Email = user.Email;
        _cache.Remove("all_users");
        
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting user {UserId}", id);
        
        var user = Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            throw new KeyNotFoundException($"User {id} not found");

        Users.Remove(user);
        _cache.Remove("all_users");
        
        await Task.CompletedTask;
    }
}
```

### Controllers/UsersController.cs

```csharp
using DotNetDevKit.ApiResponse;
using Microsoft.AspNetCore.Mvc;
using MyApi.Services;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<User>>> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            return Ok(ApiResponse<User>.Success(user, "User retrieved successfully"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<User>.Failure("User not found", 404));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(500, ApiResponse<User>.Exception(ex));
        }
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedApiResponse<User>>> GetAllUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var users = await _userService.GetAllAsync();
            var totalItems = users.Count;
            var paginatedUsers = users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(PaginatedApiResponse<User>.Success(
                paginatedUsers,
                page,
                pageSize,
                totalItems,
                "Users retrieved successfully"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, new PaginatedApiResponse<User>
            {
                Success = false,
                StatusCode = 500,
                Message = "An error occurred while retrieving users"
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<User>>> CreateUser(
        [FromBody] CreateUserRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(ApiResponse<User>.ValidationError(
                    new Dictionary<string, string[]>
                    {
                        { "Name", new[] { "Name is required" } }
                    }
                ));

            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(ApiResponse<User>.ValidationError(
                    new Dictionary<string, string[]>
                    {
                        { "Email", new[] { "Email is required" } }
                    }
                ));

            var user = await _userService.CreateAsync(request);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id },
                ApiResponse<User>.Success(user, "User created successfully", 201)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, ApiResponse<User>.Exception(ex));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateUser(
        int id,
        [FromBody] CreateUserRequest request)
    {
        try
        {
            var user = new User { Id = id, Name = request.Name, Email = request.Email };
            await _userService.UpdateAsync(id, user);
            
            return Ok(ApiResponse<object>.Success(
                new { id, message = "User updated" },
                "User updated successfully"
            ));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<object>.Failure("User not found", 404));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, ApiResponse<object>.Exception(ex));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int id)
    {
        try
        {
            await _userService.DeleteAsync(id);
            return Ok(ApiResponse<object>.Success(new { id }, "User deleted successfully"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<object>.Failure("User not found", 404));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, ApiResponse<object>.Exception(ex));
        }
    }
}
```

### Models/User.cs

```csharp
namespace MyApi;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class CreateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
```

## Example 2: Multi-Layer Application

### Repository Pattern with Auto-Registration

```csharp
// Repositories/IRepository.cs
using DotNetDevKit.DependencyInjection;

public interface IRepository<T>
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

// Repositories/GenericRepository.cs
[AutoRegisterService(typeof(IRepository<>), ServiceLifetime.Scoped)]
public class GenericRepository<T> : IRepository<T> where T : class
{
    private readonly ILogger<GenericRepository<T>> _logger;

    public GenericRepository(ILogger<GenericRepository<T>> logger)
    {
        _logger = logger;
    }

    public async Task<T> GetByIdAsync(int id)
    {
        _logger.LogInformation("Getting {Type} with id {Id}", typeof(T).Name, id);
        // Implementation
        return null!;
    }

    // Other methods...
}

// Services/IProductService.cs
public interface IProductService
{
    Task<Product> GetProductAsync(int id);
    Task<IEnumerable<Product>> GetAllProductsAsync();
}

// Services/ProductService.cs
[AutoRegisterService(ServiceLifetime.Scoped)]
public class ProductService : IProductService
{
    private readonly IRepository<Product> _repository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IRepository<Product> repository,
        ILogger<ProductService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Product> GetProductAsync(int id)
    {
        _logger.LogInformation("Getting product {ProductId}", id);
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        _logger.LogInformation("Getting all products");
        return await _repository.GetAllAsync();
    }
}
```

## Example 3: Advanced Configuration

```csharp
// Program.cs with advanced setup
var builder = WebApplicationBuilder.CreateBuilder(args);

// Configure DotNetDevKit with custom options
builder.Services.AddDotNetDevKit(options =>
{
    // Only scan specific assembly
    options.AssembliesToScan = new[] 
    { 
        typeof(Program).Assembly,
        typeof(SomeOtherAssembly).Assembly 
    };

    // Only scan specific namespace
    options.NamespaceFilter = "MyApp.Services";

    // Enable/disable features selectively
    options.EnableAutoServiceRegistration = true;
    options.EnableDebugDashboard = builder.Environment.IsDevelopment();
    options.EnableGlobalExceptionHandling = true;
    options.EnableApiResponseWrapper = true;

    // Custom dashboard route
    options.DashboardRoutePrefix = "api/dev/dashboard";

    // Environment detection
    options.IsDevelopmentEnvironment = builder.Environment.IsDevelopment();
});

// Add other services
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Use middleware in correct order
app.UseDotNetDevKit(options =>
{
    options.IsDevelopmentEnvironment = app.Environment.IsDevelopment();
});

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
```

## Testing DotNetDevKit

### Using cURL

```bash
# Get dashboard info
curl -X GET "https://localhost:7000/api/dev/dashboard/info" -H "accept: application/json"

# Get registered services
curl -X GET "https://localhost:7000/api/dev/dashboard/services" -H "accept: application/json"

# Get health status
curl -X GET "https://localhost:7000/api/dev/dashboard/health" -H "accept: application/json"

# Get all users (API endpoint)
curl -X GET "https://localhost:7000/api/users" -H "accept: application/json"

# Create user
curl -X POST "https://localhost:7000/api/users" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "email": "john@example.com"
  }'
```

### Using Postman

1. Create a new Collection "DotNetDevKit"
2. Add requests:
   - GET: {{base_url}}/api/dev/dashboard/info
   - GET: {{base_url}}/api/dev/dashboard/services
   - GET: {{base_url}}/api/dev/dashboard/health
   - GET: {{base_url}}/api/users
   - POST: {{base_url}}/api/users

### Unit Testing Example

```csharp
[TestFixture]
public class UsersControllerTests
{
    private Mock<IUserService> _userServiceMock;
    private UsersController _controller;

    [SetUp]
    public void Setup()
    {
        _userServiceMock = new Mock<IUserService>();
        _controller = new UsersController(_userServiceMock.Object, new Mock<ILogger<UsersController>>().Object);
    }

    [Test]
    public async Task GetUser_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var user = new User { Id = userId, Name = "John", Email = "john@example.com" };
        _userServiceMock.Setup(s => s.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result.Result);
        var okResult = result.Result as OkObjectResult;
        var response = okResult?.Value as ApiResponse<User>;
        Assert.IsTrue(response?.Success);
        Assert.AreEqual(userId, response?.Data?.Id);
    }

    [Test]
    public async Task GetUser_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var userId = 999;
        _userServiceMock.Setup(s => s.GetByIdAsync(userId))
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
    }
}
```

---

These examples demonstrate the full power of DotNetDevKit in real-world scenarios!
