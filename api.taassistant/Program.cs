using Api.TaAssistant.Configuration;
using Api.TaAssistant.Middleware;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hellang.Middleware.ProblemDetails;
using Microsoft.OpenApi.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Npgsql;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Data;
using System.Reflection;
using System.Text.Json.Serialization;
using TaAssistant.Data.Repositories;
using TaAssistant.Interfaces.Repositories;
using TaAssistant.Model;
using TaAssistant.Model.Api;
using TaAssistant.Model.Exceptions;
using TaAssistant.Service.V1;
using static LanguageExt.Prelude;

var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var application = new Application(configuration);

var logger = configuration.GetSection("Log").Get<ApplicationLogger>() ?? new ApplicationLogger();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.ControlledBy(new LevelSwitch(logger.MinimumLevel))
    .Enrich.FromLogContext()
    .Enrich.WithThreadName()
    .Enrich.WithThreadId()
    .WriteTo.Conditional(_ => logger.Console.Enabled, c => c.Async(lsc => lsc.Console(restrictedToMinimumLevel: Level(logger.Console.MinimumLevel))))
    .CreateLogger();
AppDomain.CurrentDomain.ProcessExit += (_, _) => Log.CloseAndFlush();

builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
builder.Logging.AddSerilog();

builder.Services.AddLazyCache();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies([typeof(ValidationBehavior<,>).Assembly, typeof(UserHandler).Assembly,]);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services
    .AddControllers()
    .AddNewtonsoftJson()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblies([Assembly.GetExecutingAssembly(),]);
builder.Services.AddHttpContextAccessor();
builder.Services.AddApiVersioning(
    options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });

    options.AddPolicy("default", policy =>
    {
        var corsOrigins = configuration.GetSection("AllowedHosts")
            .AsEnumerable().Where(s => !string.IsNullOrEmpty(s.Value)).Select(s => s.Value!).ToArray();

        if (corsOrigins.Length != 0)
        {
            policy.WithOrigins(corsOrigins)
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

builder.Services.AddProblemDetails(options => ConfigureProblemDetails(options, application.Name, builder.Environment.EnvironmentName, Log.Logger, ErrorSource.TaAssistantApi));

builder.Services.AddSwaggerGen(c =>
{
    c.CustomOperationIds(apiDesc => { return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null; });
    c.UseAllOfToExtendReferenceSchemas();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                System.Array.Empty<string>()
            }
        });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name!}.xml";
    var filePath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(filePath);

    c.CustomSchemaIds(x => x.FullName);
});

// Add Host Services
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(ConfigureContainer);
builder.Host.UseSerilog();

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseCors("AllowAll");
}
else
{
    app.UseExceptionHandler("/error");
    app.UseCors("default");
}

var provider = app.Services.GetService<IApiVersionDescriptionProvider>();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    if (provider != null)
    {
        foreach (var groupName in provider.ApiVersionDescriptions.Select(description => description.GroupName))
            c.SwaggerEndpoint($"/swagger/{groupName}/swagger.json", groupName.ToUpperInvariant());
    }
});

app.UseProblemDetails();
app.Use(CustomMiddleware);
app.UseAuthentication();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();

void ConfigureContainer(ContainerBuilder containerBuilder)
{
    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
    var connectionString = application.DefaultConnectionString;
    //var connectionString = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTIONSTRING");
    //var connectionString = configuration.GetValue<string>("default-c2807");
    Log.Error(connectionString);
    containerBuilder.RegisterInstance(application);
    containerBuilder.RegisterInstance(new Configuration
    {
        EmailConnectionString = application.EmailConnectionString,
        Pepper = application.Pepper,
    });
    containerBuilder.RegisterInstance(Log.Logger);
    containerBuilder.Register((_, _) => new NpgsqlConnection(connectionString)).As<IDbConnection>()
        .InstancePerLifetimeScope();

    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(application.StorageConnectionString);
    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
    CloudBlobContainer container = blobClient.GetContainerReference("taassistant");

    containerBuilder.RegisterInstance(blobClient);
    containerBuilder.RegisterInstance(container);


    containerBuilder.Register<IUserTypeRepository>((c, _) => new UserTypeRepository(c.Resolve<IDbConnection>()));
    containerBuilder.Register<IUserRepository>((c, _) => new UserRepository(c.Resolve<IDbConnection>()));
    containerBuilder.Register<IApplicationRepository>((c, _) => new ApplicationRepository(c.Resolve<IDbConnection>()));
    containerBuilder.Register<ICourseRepository>((c, _) => new CourseRepository(c.Resolve<IDbConnection>()));

}

/// <summary>
/// Program class for the application.
/// </summary>
public partial class Program
{

    /// <summary>
    /// Returns a <see cref="LogEventLevel"/> based on the passed in string value. Defaults to <see cref="LogEventLevel.Error"/>.
    /// </summary>
    /// <param name="value">The string value to parse into a <see cref="LogEventLevel"/>.</param>
    /// <returns>a <see cref="LogEventLevel"/>.</returns>
    static LogEventLevel Level(string value) =>
    parseEnum<LogEventLevel>(value).IfNone(LogEventLevel.Error);

    /// <summary>
    /// Method to configure ProblemDetails for the API.
    /// </summary>
    /// <param name="options">The <see cref="Hellang.Middleware.ProblemDetails.ProblemDetailsOptions"/>.</param>
    /// <param name="applicationName">The name of the application.</param>
    /// <param name="environmentName">The name of the environment.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="errorSource">A <see cref="ErrorSource"/> representing the API.</param>
    static void ConfigureProblemDetails(Hellang.Middleware.ProblemDetails.ProblemDetailsOptions options,
        string applicationName,
        string environmentName,
        Serilog.ILogger logger,
        ErrorSource errorSource)
    {
        options.IncludeExceptionDetails = (_, exception) =>
        {
            logger.Error("Exception thrown while processing... ExceptionTime: {DateTime}, Application: {Application}, " +
                "ExceptionType: {Type}, ExceptionMessage: {Message}, StackTrace: {StackTrace}",
                DateTimeOffset.UtcNow, applicationName, exception.GetType(), exception.Message, exception.StackTrace);
            return new List<string> { "dev", "qa" }.Contains(environmentName.ToLower());
        };
        options.Map<ValidationException>(e => new ApiValidationProblemDetails(e, errorSource));

        options.Rethrow<NotSupportedException>();
        options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);
        options.MapToStatusCode<HttpRequestException>(StatusCodes.Status503ServiceUnavailable);
        options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Custom middleware to handle errors.
    /// </summary>
    /// <param name="context">The HttpContext.</param>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <returns>A Task.</returns>
    /// <exception cref="MiddlewareException">An exception that is thrown from this middleware if an error endpoint is browsed to.</exception>
    static Task CustomMiddleware(HttpContext context, Func<Task> next)
    {
        if (context.Request.Path.StartsWithSegments("/middleware", out _, out var remaining))
        {
            if (remaining.StartsWithSegments("/error"))
            {
                throw new MiddlewareException("This is an exception thrown from middleware.");
            }

            if (remaining.StartsWithSegments("/status", out _, out remaining))
            {
                var statusCodeString = remaining.Value?.Trim('/');

                if (int.TryParse(statusCodeString, out var statusCode))
                {
                    context.Response.StatusCode = statusCode;
                    return Task.CompletedTask;
                }

            }
        }

        return next();
    }
}


/// <summary>
/// A class to represent a level switch.
/// </summary>
public class LevelSwitch : Serilog.Core.LoggingLevelSwitch
{
    /// <summary>
    /// A constructor to return a <see cref="LevelSwitch"/> with a minimum level settings.
    /// </summary>
    /// <param name="value">The string to parse the minimum log level.</param>
    public LevelSwitch(string value) => MinimumLevel = parseEnum<LogEventLevel>(value).IfNone(LogEventLevel.Error);
}