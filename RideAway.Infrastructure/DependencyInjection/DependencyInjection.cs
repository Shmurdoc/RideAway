using System.Net;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Extensions.Http;
using RideAway.Application.Common.Behaviors;
using RideAway.Application.Features.Rides.Handlers.Queries;
using RideAway.Application.IRepositories;
using RideAway.Application.IServices;
using RideAway.Application.IServices.IAuthentication;
using RideAway.Application.IServices.INotification;
using RideAway.Application.Services;
using RideAway.Domain.Service;
using RideAway.Infrastructure.Authentication;
using RideAway.Infrastructure.Mappers;
using RideAway.Infrastructure.Notifications;
using RideAway.Infrastructure.Payments;
using RideAway.Infrastructure.Persistence.Repositories;

namespace RideAway.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<IGoogleMapsApi, GoogleMapsApiService>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(30));
            services.AddHttpClient<IGeoCodingService, GoogleGeocodingService>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(30));

            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ISmsService, SmsService>();
            services.AddScoped<IStripePaymentService, StripePaymentService>();
            services.AddScoped<ILocationService, GoogleMapsLocationService>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRideRepository, RideRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IRideMatchingService, RideMatchingService>();
            services.AddScoped<IPaymentProcessingService, PaymentProcessingService>();
            services.AddScoped<IFareCalculationService, FareCalculationService>();
            services.AddScoped<IRideFactory, RideFactory>();

            services.AddAutoMapper(typeof(PaymentProfile));
            services.AddAutoMapper(typeof(UserProfile));

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAvailableRidesHandler).Assembly));

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            var jwtKey = configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException(
                    "JWT authentication is not configured: set the 'Jwt:Key' setting (a minimum of 32 characters is recommended).");
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // Keep the claim types exactly as issued. With the default inbound
                    // mapping enabled, "role" is rewritten to the long ClaimTypes.Role
                    // URI, which silently broke every [Authorize(Roles = ...)] check.
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.FromMinutes(1),
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        NameClaimType = "email",
                        RoleClaimType = "role"
                    };
                });

            services.AddAuthorization();
            services.AddCors(options =>
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
                options.AddDefaultPolicy(policy =>
                {
                    if (allowedOrigins is { Length: > 0 })
                    {
                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    }
                    else
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    }
                });
            });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
        }
    }
}
