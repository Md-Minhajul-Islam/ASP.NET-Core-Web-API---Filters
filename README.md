# ASP.NET-Core-Web-API---Filters

https://dotnettutorials.net/lesson/exception-filters-in-asp-net-core-web-api/


Exception Filters are part of the MVC pipeline. They only catch exceptions thrown inside controller actions and the related filters. Middleware, on the other hand, runs very early in the ASP.NET Core pipeline and can catch exceptions thrown anywhere downstream, including MVC actions, other middleware, static file requests, or any part of the pipeline.

## When to choose which:
-Use middleware when you want consistent, global exception handling across the entire app, including non-MVC routes or static files.

-Use exception filters if you want MVC-specific error handling with access to action context, or want to handle exceptions on a per-controller or per-action basis.

In most real-world ASP.NET Core apps, exception middleware is preferred for global error handling because it covers all cases consistently and runs early in the pipeline. Exception filters are useful when you need MVC-specific control or want to customize error handling.
