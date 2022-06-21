## REST Based Services
As much as possible, we will be trying to follow REST based standards for our services.  
For more information - [REST API Tutorial](https://restfulapi.net/http-methods/)   

## Versioning
Versioning has been implemented via a querystring parameter.

The query string parameter that needs to be provided is in the following format
`api-version=1.0`

If no api-version parameter is supplied, then the app will use the DefaultApiVersion configured in the Startup.cs file

There is no need to create a new version, unless you create a breaking change.
If you need to create a new version, copy the v[MostRecentVersion] folders and create a v[Next]  

The service needs to be versioned as a whole, if you create a v[Next] for one controller, you need a v[next] for all the others as well.

For more information - [Versioning Quick Start](https://github.com/Microsoft/aspnet-api-versioning/wiki/New-Services-Quick-Start#aspnet-core)

## Add ConfigSetting
- Create a Setting node in appsettings.json. eg: ExampleConfig
- Create a ExampleConfig.cs map properties with the code name in json
- Register ExampleConfig.cs in Startup.cs eg: `services.Configure<ExampleConfig>(Configuration.GetSection("ExampleConfig"))`
- Access the ExampleConfig

eg: in ExampleController.cs constructor
```
public ExampleController(IOptions<ExampleConfig> config)
{
    Config = config;
}
```

For more information - [Configuration in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.2)

## Health Check
We use Microsoft.AspNetCore.Diagnostics.HealthChecks package for health checks

- If you would like to implement additional health checks, you can create a new class that inherits from the `IHealthCheck` interface. This will then be called automatically when the `/health` endpoint is called.

For more information - 
[Health checks in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-2.2)  



## Validation - FluentValidation
Fluentvalidation has been implemented to  validate objects that are passed in to controller actions by the model binding infrastructure.

- See `GetCustomersByAgeRequestValidator.cs` for a validator example  
- See `GetCustomersByAgeRequestValidatorTest` for an example validator test.

For more information - 
[ASP.NET Integration](https://fluentvalidation.net/aspnet)  