using System.Reflection;
using Microsoft.EntityFrameworkCore;
using RideAway.API.Middleware;
using RideAway.Infrastructure.DependencyInjection;
using RideAway.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

ValidateConfiguration(builder.Configuration, builder.Environment);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

builder.Services.ImplementPersistence(builder.Configuration);
builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

static void ValidateConfiguration(IConfiguration configuration, IHostEnvironment environment)
{
    var missing = new List<string>();

    if (string.IsNullOrWhiteSpace(configuration.GetConnectionString("DefaultConnection")))
        missing.Add("ConnectionStrings:DefaultConnection");

    if (string.IsNullOrWhiteSpace(configuration["Jwt:Key"]))
        missing.Add("Jwt:Key");

    if (string.IsNullOrWhiteSpace(configuration["Jwt:Issuer"]))
        missing.Add("Jwt:Issuer");

    if (!environment.IsDevelopment() && configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() is not { Length: > 0 })
        missing.Add("Cors:AllowedOrigins");

    if (missing.Count > 0)
        throw new InvalidOperationException(
            $"The application is not configured correctly. Missing required settings: {string.Join(", ", missing)}.");
}
