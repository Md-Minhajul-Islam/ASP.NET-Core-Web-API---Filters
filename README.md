# ASP.NET Core Web API — Filters

A hands-on reference repository demonstrating every built-in filter type available in **ASP.NET Core Web API**. Each filter is isolated on its own Git branch so you can explore them independently without noise.

---

## 📑 Table of Contents

1. [What Are Filters?](#what-are-filters)
2. [Request Processing Lifecycle](#request-processing-lifecycle)
3. [Filter Execution Order](#filter-execution-order)
4. [Branch Guide](#branch-guide)
5. [Authorization Filter](#1-authorization-filter)
6. [Resource Filter](#2-resource-filter)
7. [Action Filter](#3-action-filter)
8. [Result Filter](#4-result-filter)
9. [Exception Filter](#5-exception-filter)
10. [Filter Scope & Registration](#filter-scope--registration)
11. [ServiceFilter vs TypeFilter](#servicefilter-vs-typefilter)
12. [How to Run](#how-to-run)

---

## What Are Filters?

Filters in ASP.NET Core allow you to run code **before or after specific stages** of the request processing pipeline. They are ideal for implementing **cross-cutting concerns** — logic that needs to be applied uniformly across multiple controllers or actions without duplicating code inside each one.

Common use cases include:

- **Authentication & Authorization** — blocking unauthorized requests before they reach business logic
- **Caching** — short-circuiting the pipeline to return a cached response
- **Logging & Auditing** — recording what happened before and after an action runs
- **Input Validation** — rejecting malformed requests early
- **Response Modification** — adding headers or compressing output
- **Exception Handling** — catching unhandled exceptions and returning structured error responses

---

## Request Processing Lifecycle

When a client sends an HTTP request to your ASP.NET Core Web API, it travels through the following stages before a response is returned:

```
Client (HTTP Request)
        │
        ▼
┌───────────────────┐
│    Middleware      │  ← Routing, CORS, Authentication, Static Files,
│    Pipeline        │    Exception Handling (global)
└────────┬──────────┘
         │
         ▼
┌───────────────────┐
│   Routing Engine  │  ← Selects the Controller & Action based on URL + HTTP Method
└────────┬──────────┘
         │
         ▼
┌───────────────────┐
│ Authorization      │  ← Checks identity & permissions (401/403 on failure)
│ Filter             │
└────────┬──────────┘
         │
         ▼
┌───────────────────┐
│ Resource Filter    │  ← Caching, short-circuiting (OnResourceExecuting)
│ (Before)           │
└────────┬──────────┘
         │
         ▼
┌───────────────────┐
│ Action Filter      │  ← Input validation, logging (OnActionExecuting)
│ (Before)           │
└────────┬──────────┘
         │
         ▼
┌───────────────────┐
│  Controller Action │  ← Your business logic lives here
└────────┬──────────┘
         │
         ▼
┌───────────────────┐
│ Action Filter      │  ← Post-action logging, auditing (OnActionExecuted)
│ (After)            │
└────────┬──────────┘
         │
         ▼
┌───────────────────┐
│ Result Filter      │  ← Response modification, add headers (OnResultExecuting)
│ (Before)           │
└────────┬──────────┘
         │
         ▼
┌───────────────────┐
│ Result Filter      │  ← Compression, final tweaks (OnResultExecuted)
│ (After)            │
└────────┬──────────┘
         │
         ▼
┌───────────────────┐
│ Resource Filter    │  ← Cache the response for future requests (OnResourceExecuted)
│ (After)            │
└────────┬──────────┘
         │
         ▼
┌───────────────────┐
│ Exception Filter   │  ← Catches unhandled exceptions anywhere in the pipeline
└────────┬──────────┘
         │
         ▼
Client (HTTP Response)
```

> **Exception Filters** can intercept exceptions thrown at any point during filter or action execution and short-circuit the remainder of the pipeline to return a structured error response.

---

## Filter Execution Order

| Order | Filter Type          | Interface(s)                                   | Runs Before Action? | Runs After Action? |
|-------|----------------------|------------------------------------------------|---------------------|--------------------|
| 1     | Authorization Filter | `IAuthorizationFilter`                         | ✅ Yes              | ❌ No              |
| 2     | Resource Filter      | `IResourceFilter`                              | ✅ Yes              | ✅ Yes             |
| 3     | Action Filter        | `IActionFilter` / `IAsyncActionFilter`         | ✅ Yes              | ✅ Yes             |
| 4     | Result Filter        | `IResultFilter` / `IAsyncResultFilter`         | ✅ Yes              | ✅ Yes             |
| 5     | Exception Filter     | `IExceptionFilter` / `IAsyncExceptionFilter`   | ❌ No               | ✅ Yes (on error)  |

---

## Branch Guide

Each filter type is demonstrated on its own branch. Switch between them as you study each concept.

| Branch                 | Filter Covered       |
|------------------------|----------------------|
| `main`                 | Base project setup   |
| `Authorization-Filter` | Authorization Filter |
| `Resource-Filter`      | Resource Filter      |
| `Action-Filter`        | Action Filter        |
| `Result-Filter`        | Result Filter        |
| `Exception-Filter`     | Exception Filter     |

```bash
# Switch to a specific filter branch
git checkout Resource-Filter
git checkout Action-Filter
# etc.
```

---

## 1. Authorization Filter

**Branch:** `Authorization-Filter`

### What It Does

The Authorization Filter is the **first filter** to execute after routing resolves the target action. Its sole responsibility is to check whether the incoming request is from an authenticated and authorized user. If the check fails, the entire pipeline is **short-circuited** — no resource, action, or result filters run — and a `401 Unauthorized` or `403 Forbidden` response is returned directly to the client.

Think of it like a **passport and visa check** at an airport: if you can't produce the right documents, you never reach the boarding gate.

### Key Characteristics

- Runs **before all other filters** in the pipeline
- Can **short-circuit** the pipeline completely on failure
- Does **not** have an "after" phase (unlike action/resource/result filters)
- The built-in `[Authorize]` attribute is the most common usage

### Interface

```csharp
public interface IAuthorizationFilter
{
    void OnAuthorization(AuthorizationFilterContext context);
}
```

For async support:

```csharp
public interface IAsyncAuthorizationFilter
{
    Task OnAuthorizationAsync(AuthorizationFilterContext context);
}
```

### Implementation Example

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class CustomAuthorizationFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Example: check for a custom API key header
        var apiKey = context.HttpContext.Request.Headers["X-API-Key"].FirstOrDefault();

        if (string.IsNullOrEmpty(apiKey) || apiKey != "my-secret-key")
        {
            // Short-circuit the pipeline — nothing else runs
            context.Result = new UnauthorizedObjectResult(new
            {
                StatusCode = 401,
                Message = "Invalid or missing API key."
            });
        }
    }
}
```

### Applying the Filter

**On a specific action:**
```csharp
[ServiceFilter(typeof(CustomAuthorizationFilter))]
[HttpGet("{id}")]
public IActionResult GetById(int id) { ... }
```

**Globally (all controllers):**
```csharp
// Program.cs
builder.Services.AddControllers(options =>
{
    options.Filters.Add<CustomAuthorizationFilter>();
});
```

**Registration:**
```csharp
builder.Services.AddScoped<CustomAuthorizationFilter>();
```

### When to Use It

Use a custom Authorization Filter when the built-in `[Authorize]` attribute isn't sufficient — for example, when validating API keys in headers, enforcing IP whitelists, or implementing custom token schemes.

---

## 2. Resource Filter

**Branch:** `Resource-Filter`

### What It Does

Resource Filters run **immediately after authorization** and **wrap the entire remainder of the pipeline** — they have both a *before* (`OnResourceExecuting`) and an *after* (`OnResourceExecuted`) phase. This makes them uniquely positioned to:

- **Short-circuit** the pipeline and return a cached response before model binding or action execution even begins
- **Cache** the response after execution so future identical requests can be served without re-running business logic

Think of it like a **dedicated customs lane for frequent travelers**: if your identity is already cached from a previous clearance, you bypass the main queue entirely.

### Key Characteristics

- Runs **after Authorization, before model binding and Action Filters**
- The `OnResourceExecuting` method can **short-circuit** the pipeline by setting `context.Result`
- The `OnResourceExecuted` method runs after the entire inner pipeline (including Action and Result filters) has completed
- Ideal for **response caching** and **request throttling**

### Interface

```csharp
public interface IResourceFilter
{
    void OnResourceExecuting(ResourceExecutingContext context);
    void OnResourceExecuted(ResourceExecutedContext context);
}
```

For async support:

```csharp
public interface IAsyncResourceFilter
{
    Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next);
}
```

### Implementation Example — Simple Cache

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class CacheResourceFilter : IResourceFilter
{
    private static readonly Dictionary<string, IActionResult> _cache = new();

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var cacheKey = context.HttpContext.Request.Path;

        if (_cache.TryGetValue(cacheKey, out var cachedResult))
        {
            Console.WriteLine($"[ResourceFilter] Cache HIT for {cacheKey}. Short-circuiting pipeline.");
            context.Result = cachedResult; // Short-circuit — skips action execution entirely
        }
        else
        {
            Console.WriteLine($"[ResourceFilter] Cache MISS for {cacheKey}. Proceeding to action.");
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        var cacheKey = context.HttpContext.Request.Path;

        if (!_cache.ContainsKey(cacheKey) && context.Result != null)
        {
            Console.WriteLine($"[ResourceFilter] Caching response for {cacheKey}.");
            _cache[cacheKey] = context.Result;
        }
    }
}
```

### Implementation Example — Async (wrapping the inner pipeline)

```csharp
public class AsyncCacheResourceFilter : IAsyncResourceFilter
{
    private static readonly Dictionary<string, IActionResult> _cache = new();

    public async Task OnResourceExecutionAsync(
        ResourceExecutingContext context,
        ResourceExecutionDelegate next)
    {
        var key = context.HttpContext.Request.Path;

        if (_cache.TryGetValue(key, out var cached))
        {
            context.Result = cached;
            return; // Short-circuit — `next` is never called
        }

        // Execute the rest of the pipeline
        var executedContext = await next();

        if (executedContext.Result != null)
            _cache[key] = executedContext.Result;
    }
}
```

### Applying the Filter

```csharp
[ServiceFilter(typeof(CacheResourceFilter))]
[HttpGet]
public IActionResult GetAll() { ... }
```

**Registration:**
```csharp
builder.Services.AddScoped<CacheResourceFilter>();
builder.Services.AddScoped<AsyncCacheResourceFilter>();
```

### When to Use It

Use a Resource Filter when you need to intercept the request as early as possible **after authorization**, particularly for caching expensive operations, request-level throttling, or validating request context before model binding runs.

---

## 3. Action Filter

**Branch:** `Action-Filter`

### What It Does

Action Filters are the most commonly used filter type. They run in **two phases**:

- `OnActionExecuting` — runs **before** the action method executes (pre-processing)
- `OnActionExecuted` — runs **after** the action method executes (post-processing)

Think of it like a **flight crew's boarding check**: staff verify your boarding pass one last time before you board (`OnActionExecuting`), and after the flight lands, crew do a final checklist before you disembark (`OnActionExecuted`).

### Key Characteristics

- Has both **before** and **after** hooks around the action method
- `OnActionExecuting` can **cancel** the action by setting `context.Result` (short-circuits Result Filters but still runs Resource Filter's `OnResourceExecuted`)
- Provides access to **action arguments** via `context.ActionArguments`
- Ideal for **validation**, **logging**, and **auditing**

### Interface

```csharp
public interface IActionFilter
{
    void OnActionExecuting(ActionExecutingContext context);
    void OnActionExecuted(ActionExecutedContext context);
}
```

For async support:

```csharp
public interface IAsyncActionFilter
{
    Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next);
}
```

### Implementation Example — Logging & Validation

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class LoggingActionFilter : IActionFilter
{
    private readonly ILogger<LoggingActionFilter> _logger;

    public LoggingActionFilter(ILogger<LoggingActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var actionName = context.ActionDescriptor.DisplayName;
        _logger.LogInformation("[ActionFilter] Executing action: {ActionName}", actionName);

        // Example: validate that model state is valid before action runs
        if (!context.ModelState.IsValid)
        {
            _logger.LogWarning("[ActionFilter] Invalid model state. Short-circuiting.");
            context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var actionName = context.ActionDescriptor.DisplayName;
        _logger.LogInformation("[ActionFilter] Finished action: {ActionName}", actionName);

        if (context.Exception != null)
        {
            _logger.LogError(context.Exception, "[ActionFilter] Action threw an exception.");
        }
    }
}
```

### Attribute-Based Action Filter

For simpler cases, inherit from `ActionFilterAttribute` to apply the filter as a decorator:

```csharp
public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = new UnprocessableEntityObjectResult(context.ModelState);
        }
    }
}
```

Apply it directly on a controller or action:

```csharp
[ValidateModel]
[HttpPost]
public IActionResult Create([FromBody] ProductDto dto) { ... }
```

### Applying via ServiceFilter

```csharp
[ServiceFilter(typeof(LoggingActionFilter))]
[HttpGet]
public IActionResult GetAll() { ... }
```

**Registration:**
```csharp
builder.Services.AddScoped<LoggingActionFilter>();
```

### When to Use It

Use an Action Filter for anything that needs to run **immediately before or after business logic**: model state validation, request logging, performance timing, or audit trails.

---

## 4. Result Filter

**Branch:** `Result-Filter`

### What It Does

Result Filters execute after the action method has completed and **wrap the result execution** — the point where the `IActionResult` is converted into an HTTP response. They have two phases:

- `OnResultExecuting` — runs **before** the result is written to the response
- `OnResultExecuted` — runs **after** the response has been written

Think of it like **in-flight services**: the flight crew prepares your meal and announcements before you land (`OnResultExecuting`), and wraps up service after landing (`OnResultExecuted`).

### Key Characteristics

- Runs **after the action** and **after Action Filters**
- `OnResultExecuting` can still modify or replace the result before it's written
- Often used to add **response headers**, apply **output formatting**, or perform **response compression**
- Only runs if the action completed **without being short-circuited** by an Authorization or Resource filter

### Interface

```csharp
public interface IResultFilter
{
    void OnResultExecuting(ResultExecutingContext context);
    void OnResultExecuted(ResultExecutedContext context);
}
```

For async:

```csharp
public interface IAsyncResultFilter
{
    Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next);
}
```

### Implementation Example — Add Custom Response Headers

```csharp
using Microsoft.AspNetCore.Mvc.Filters;

public class ResponseHeaderResultFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        // Add custom headers before the response is written
        context.HttpContext.Response.Headers["X-Custom-Header"] = "MyApiValue";
        context.HttpContext.Response.Headers["X-Processed-At"] =
            DateTime.UtcNow.ToString("o");

        Console.WriteLine("[ResultFilter] Adding response headers.");
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        Console.WriteLine("[ResultFilter] Response has been written to the client.");
    }
}
```

### Implementation Example — Async (wrapping result execution)

```csharp
public class TimingResultFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(
        ResultExecutingContext context,
        ResultExecutionDelegate next)
    {
        var start = DateTime.UtcNow;

        context.HttpContext.Response.Headers["X-Result-Start"] = start.ToString("o");

        await next(); // Write the result

        var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
        Console.WriteLine($"[ResultFilter] Result executed in {elapsed:F2}ms");
    }
}
```

### Applying the Filter

```csharp
[ServiceFilter(typeof(ResponseHeaderResultFilter))]
[HttpGet]
public IActionResult GetAll() { ... }
```

**Registration:**
```csharp
builder.Services.AddScoped<ResponseHeaderResultFilter>();
builder.Services.AddScoped<TimingResultFilter>();
```

### When to Use It

Use a Result Filter when you need to **modify the HTTP response** just before it leaves your API: adding tracking headers, applying content transformation, measuring response write time, or enforcing a consistent response envelope.

---

## 5. Exception Filter

**Branch:** `Exception-Filter`

### What It Does

Exception Filters provide a **centralized mechanism for handling unhandled exceptions** thrown during action execution, model binding, or other filters. If an exception bubbles up through the pipeline without being caught, the Exception Filter intercepts it and can return a structured error response instead of the default error page.

Think of it like **emergency protocols on a flight**: if something goes wrong mid-flight (an unhandled exception), the crew follows established emergency procedures (`OnException`) to bring passengers (the client) to safety (a structured error response) rather than crashing uncontrolled.

### Key Characteristics

- Intercepts **unhandled exceptions** from action methods, model binding, and other filters
- Can **suppress the exception** by setting `context.ExceptionHandled = true`
- Can **set a result** (`context.Result`) to return a controlled error response
- Only handles exceptions — does **not** run on successful requests
- Runs **before** the exception propagates to the middleware error handler

### Interface

```csharp
public interface IExceptionFilter
{
    void OnException(ExceptionContext context);
}
```

For async:

```csharp
public interface IAsyncExceptionFilter
{
    Task OnExceptionAsync(ExceptionContext context);
}
```

### Implementation Example — Global Exception Handler

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception,
            "[ExceptionFilter] Unhandled exception caught: {Message}",
            context.Exception.Message);

        // Return a consistent error response
        context.Result = context.Exception switch
        {
            KeyNotFoundException => new NotFoundObjectResult(new
            {
                StatusCode = 404,
                Message = context.Exception.Message
            }),
            UnauthorizedAccessException => new ObjectResult(new
            {
                StatusCode = 403,
                Message = "You do not have permission to perform this action."
            }) { StatusCode = 403 },
            _ => new ObjectResult(new
            {
                StatusCode = 500,
                Message = "An unexpected error occurred. Please try again later.",
                Detail = context.Exception.Message
            }) { StatusCode = 500 }
        };

        // Mark the exception as handled so it doesn't propagate further
        context.ExceptionHandled = true;
    }
}
```

### Applying the Filter

**Globally (recommended for exception filters):**
```csharp
// Program.cs
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});

builder.Services.AddScoped<GlobalExceptionFilter>();
```

**On a specific controller:**
```csharp
[ServiceFilter(typeof(GlobalExceptionFilter))]
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase { ... }
```

### Exception Filter vs Middleware

| Aspect                  | Exception Filter          | Middleware (`UseExceptionHandler`) |
|-------------------------|---------------------------|-------------------------------------|
| Scope                   | Action pipeline only      | Entire HTTP pipeline                |
| Access to action context| ✅ Yes                    | ❌ No                               |
| Handles routing errors  | ❌ No                     | ✅ Yes                              |
| Best for                | Business logic exceptions | Infrastructure / global errors      |

For the most robust error handling, use **both**: Exception Filters for action-level exceptions and `UseExceptionHandler` middleware as a final safety net.

---

## Filter Scope & Registration

Filters can be applied at three scopes. When multiple scopes apply, they execute in the order below:

### 1. Global Scope (all controllers & actions)

```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();      // by type
    options.Filters.Add(new ResponseHeaderResultFilter()); // by instance
});
```

### 2. Controller Scope

```csharp
[ServiceFilter(typeof(LoggingActionFilter))]
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase { ... }
```

### 3. Action Scope

```csharp
[ServiceFilter(typeof(CacheResourceFilter))]
[HttpGet("{id}")]
public IActionResult GetById(int id) { ... }
```

### Execution Order within the Same Scope

By default, filters at the same scope level run in **registration order**. You can control the order by implementing `IOrderedFilter`:

```csharp
public class MyFilter : IActionFilter, IOrderedFilter
{
    public int Order => -1; // Lower numbers run first

    public void OnActionExecuting(ActionExecutingContext context) { ... }
    public void OnActionExecuted(ActionExecutedContext context) { ... }
}
```

---

## ServiceFilter vs TypeFilter

Both `[ServiceFilter]` and `[TypeFilter]` allow you to apply filters that have **constructor dependencies** (i.e., they need DI). They differ in one important way:

| Feature                   | `[ServiceFilter]`                          | `[TypeFilter]`                             |
|---------------------------|--------------------------------------------|--------------------------------------------|
| DI Container Required     | ✅ Must be registered in `IServiceCollection` | ❌ Not required — framework creates it     |
| Lifetime                  | Respects the registered lifetime (Scoped, Transient, etc.) | Always transient (new instance per use) |
| Pass constructor arguments| ❌ Not directly                            | ✅ Yes, via `Arguments` property           |
| Best for                  | Filters with shared state or long lifetime | Filters with parameters or one-off use     |

### ServiceFilter Example

```csharp
// Registration required
builder.Services.AddScoped<LoggingActionFilter>();

// Usage
[ServiceFilter(typeof(LoggingActionFilter))]
public IActionResult GetAll() { ... }
```

### TypeFilter Example

```csharp
// No registration needed
[TypeFilter(typeof(LoggingActionFilter))]
public IActionResult GetAll() { ... }

// With constructor arguments
[TypeFilter(typeof(CustomHeaderFilter), Arguments = new object[] { "X-Trace-Id", "abc-123" })]
public IActionResult GetById(int id) { ... }
```

---

## How to Run

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) or later
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or VS Code with the C# extension
- [Postman](https://www.postman.com/) or any HTTP client to test endpoints

### Steps

```bash
# 1. Clone the repository
git clone https://github.com/Md-Minhajul-Islam/ASP.NET-Core-Web-API---Filters.git
cd ASP.NET-Core-Web-API---Filters

# 2. Switch to the branch you want to explore
git checkout Resource-Filter   # or any other branch

# 3. Restore dependencies
dotnet restore

# 4. Run the project
dotnet run --project FiltersInWebApi

# 5. Open Swagger UI in your browser
# https://localhost:{port}/swagger
```

Each branch is self-contained. The filter under study is already registered and applied to the relevant controller or action — just run the project and hit the endpoints with Postman or Swagger to observe the filter behavior in the console logs.

---

## Reference

- [ASP.NET Core Request Processing Life Cycle — Dot Net Tutorials](https://dotnettutorials.net/lesson/asp-net-core-request-processing-life-cycle/)
- [Filters in ASP.NET Core — Microsoft Docs](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters)

---

*Made with ❤️ by [Md-Minhajul-Islam](https://github.com/Md-Minhajul-Islam)*
