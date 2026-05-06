using Nimble.Modulith.Users;
using Nimble.Modulith.Products;
using Nimble.Modulith.Customers;
using Nimble.Modulith.Email;
using Serilog;
using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;

var logger = Log.Logger = new LoggerConfiguration()
  .Enrich.FromLogContext()
  .WriteTo.Console()
  .CreateLogger();

logger.Information("Starting web host");

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((_, config) => config.ReadFrom.Configuration(builder.Configuration));

// Add service defaults (Aspire configuration)
builder.AddServiceDefaults();

// Add Mediator (source-generated, scans all referenced assemblies)
builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
});

// Add FastEndpoints with JWT Bearer Authentication and Authorization
builder.Services.AddFastEndpoints()
    .AddAuthenticationJwtBearer(s =>
    {
        s.SigningKey = builder.Configuration["Auth:JwtSecret"];
    })
    .AddAuthorization()
    .SwaggerDocument(o =>
    {
        o.DocumentSettings = s =>
        {
            s.Title = "Nimble Modulith API";
            s.Version = "v1";
        };
    });

// Add module services
builder.AddUsersModuleServices(logger);
builder.AddProductsModuleServices(logger);
builder.AddCustomersModuleServices(logger);
builder.AddEmailModuleServices(logger);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints()
    .UseSwaggerGen();

// Ensure module databases are created/migrated
await app.EnsureUsersModuleDatabaseAsync();
await app.EnsureProductsModuleDatabaseAsync();
await app.EnsureCustomersModuleDatabaseAsync();

app.Run();
