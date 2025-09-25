using M_SAVA_API.Filters;
using M_SAVA_API.Handlers;
using M_SAVA_API.Middleware;
using M_SAVA_BLL.Services;
using M_SAVA_BLL.Services.Interfaces;
using M_SAVA_DAL.Contexts;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using M_SAVA_INF.Environment;
using M_SAVA_INF.Managers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using Serilog;
using Serilog.Events;
using M_SAVA_BLL.Loggers;
using M_SAVA_BLL.Services.Fetch;
using M_SAVA_BLL.Services.Persistence;
using M_SAVA_BLL.Services.Retrieval;
using M_SAVA_BLL.Services.Access;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Register local environment
builder.Services.AddSingleton<ILocalEnvironment, LocalEnvironment>();
var env = LocalEnvironment.Instance;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
    .MinimumLevel.Is(env.Values.SerilogInformationLevel)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProcessId()
    .WriteTo.Async(a => a.File(
        path: "Logs/serilog-.txt",
        rollingInterval: env.Values.SerilogRollingInterval,
        retainedFileCountLimit: env.Values.SerilogRetainedFileCountLimit,
        fileSizeLimitBytes: env.Values.SerilogFileSizeLimitBytes,
        rollOnFileSizeLimit: env.Values.SerilogRollOnFileSizeLimit,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
    ))
    .CreateLogger();
builder.Host.UseSerilog();

Log.Information("Serilog configured with Information level: {InformationLevel}, Rolling Interval: {RollingInterval}, Retained File Count Limit: {RetainedFileCountLimit}, File Size Limit Bytes: {FileSizeLimitBytes}, Roll On File Size Limit: {RollOnFileSizeLimit}",
    env.Values.SerilogInformationLevel, env.Values.SerilogRollingInterval, env.Values.SerilogRetainedFileCountLimit, env.Values.SerilogFileSizeLimitBytes, env.Values.SerilogRollOnFileSizeLimit);

// Register controllers
builder.Services.AddControllers(options =>
{
    options.Filters.Add<TaintedPathFilter>();
});

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddRequirements(new NotBannedRequirement())
        .Build();
});

// Swagger setup WITH JWT SUPPORT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "M-SAVA-API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
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
            new string[] {}
        }
    });

    c.OperationFilter<OctetStreamOperationFilter>();
    c.OperationFilter<AuthorizeCheckOperationFilter>();
});

// Register main database context
string baseDbConnectionString = $"Host={env.Values.PostgresBaseDbHost};Port={env.Values.PostgresBaseDbPort};Database={env.Values.PostgresBaseDbDbName};Username={env.Values.PostgresBaseDbUser};Password={env.Values.PostgresBaseDbPassword};Ssl Mode={env.Values.PostgresBaseDbSslMode}";
builder.Services.AddDbContext<BaseDataContext>(options =>
    options.UseNpgsql(baseDbConnectionString));

// Register HTTP services
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IReturnFileService, FileReturnService>();
builder.Services.AddScoped<IStoreFileService, FileUploadService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<ISearchFileService, FileSearchService>();
builder.Services.AddScoped<ISeedingService, SeedingService>();
builder.Services.AddScoped<FileWriteService>();
builder.Services.AddScoped<AccessGroupService>();
builder.Services.AddScoped<InviteCodeService>();
builder.Services.AddScoped<FetchYouTubeFileService>();
builder.Services.AddScoped<FetchGoogleDriveService>();
// builder.Services.AddScoped<FetchTwitterFileService>(); // Not ready yet

// Register custom loggers
builder.Services.AddScoped<ServiceLogger>();

// Register singletons
builder.Services.AddSingleton<IAuthorizationHandler, NotBannedHandler>();

// Register repositories
builder.Services.AddScoped<IIdentifiableRepository<UserDB>, IdentifiableRepository<UserDB>>();
builder.Services.AddScoped<IIdentifiableRepository<InviteCodeDB>, IdentifiableRepository<InviteCodeDB>>();
builder.Services.AddScoped<IIdentifiableRepository<AccessCodeDB>, IdentifiableRepository<AccessCodeDB>>();
builder.Services.AddScoped<IIdentifiableRepository<AccessGroupDB>, IdentifiableRepository<AccessGroupDB>>();
builder.Services.AddScoped<IIdentifiableRepository<JwtDB>, IdentifiableRepository<JwtDB>>();
builder.Services.AddScoped<IIdentifiableRepository<SavedFileDataDB>, IdentifiableRepository<SavedFileDataDB>>();
builder.Services.AddScoped<IIdentifiableRepository<SavedFileReferenceDB>, IdentifiableRepository<SavedFileReferenceDB>>();
// Log repositories
builder.Services.AddScoped<IIdentifiableRepository<UserLogDB>, IdentifiableRepository<UserLogDB>>();
builder.Services.AddScoped<IIdentifiableRepository<AccessLogDB>, IdentifiableRepository<AccessLogDB>>();
builder.Services.AddScoped<IIdentifiableRepository<ErrorLogDB>, IdentifiableRepository<ErrorLogDB>>();
builder.Services.AddScoped<IIdentifiableRepository<GroupLogDB>, IdentifiableRepository<GroupLogDB>>();
builder.Services.AddScoped<IIdentifiableRepository<InviteLogDB>, IdentifiableRepository<InviteLogDB>>();

// Register managers
builder.Services.AddScoped<FileManager>();

// JWT Authentication setup
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = env.Values.JwtIssuerName,
            ValidateAudience = true,
            ValidAudience = env.Values.JwtIssuerAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero, 
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(LocalEnvironment.Instance.GetSigningKeyBytes())
        };
    });

WebApplication app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "Handled {RequestPath}";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
    };
});

app.UseRouting();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, "Data")),
    RequestPath = "/api/files/public",
    OnPrepareResponse = ctx =>
    {
        var filePath = ctx.File.PhysicalPath;
        var metaPath = filePath + ".meta.json";

        if (!System.IO.File.Exists(metaPath))
        {
            ctx.Context.Response.StatusCode = StatusCodes.Status403Forbidden;
            ctx.Context.Abort();
            return;
        }

        try
        {
            var metaJson = System.IO.File.ReadAllText(metaPath);
            var metaDoc = JsonDocument.Parse(metaJson);
            bool anyPublic= false;
            if (metaDoc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in metaDoc.RootElement.EnumerateArray())
                {
                    if (element.TryGetProperty("PublicDownload", out var publicProp) && publicProp.GetBoolean())
                    {
                        anyPublic = true;
                        break;
                    }
                }
            }
            if (!anyPublic)
            {
                ctx.Context.Response.StatusCode = StatusCodes.Status403Forbidden;
                ctx.Context.Abort();
            }
        }
        catch
        {
            ctx.Context.Response.StatusCode = StatusCodes.Status403Forbidden;
            ctx.Context.Abort();
        }
    }
});

app.UseMiddleware<ExceptionCatcherMiddleware>();

app.UseAuthentication();
app.UseMiddleware<RequestContextMiddleware>();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<ISeedingService>();
    seeder.Seed();
}

app.Run();
