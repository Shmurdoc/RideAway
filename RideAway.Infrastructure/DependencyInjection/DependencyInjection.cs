using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
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
            services.AddHttpClient<IGoogleMapsApi, GoogleMapsApiService>();
            services.AddHttpClient<IGeoCodingService, GoogleGeocodingService>();

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
            if (!string.IsNullOrEmpty(jwtKey))
            {
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = false,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = configuration["Jwt:Issuer"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                        };
                    });
            }

            services.AddAuthorization();
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
        }
    }
}
