using Microsoft.Extensions.DependencyInjection;
using RideAway.Infrastructure.Authentication;
using RideAway.Infrastructure.Notifications;
using RideAway.Infrastructure.Persistence.Repositories;
using RideAway.Application.IRepositories;
using Microsoft.Extensions.Configuration;
using RideAway.Application.IServices;
using RideAway.Application.Services;
using RideAway.Application.IServices.INotification;
using RideAway.Application.IServices.IAuthentication;
using RideAway.Infrastructure.Mappers;
using RideAway.Application.Features.Rides.Handlers.Queries;
using MediatR;
using RideAway.Application.Common.Behaviors;
using RideAway.Domain.Service;

namespace RideAway.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<IGoogleMapsApi, GoogleMapsApiService>();

            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ISmsService, SmsService>();
            services.AddScoped<IStripePaymentService, StripePaymentService>();
            services.AddScoped<ILocationService, GoogleMapsLocationService>();
            services.AddScoped<IGeoCodingService, GoogleGeocodingService>();

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
        }
    }
}
