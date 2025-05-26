using Microsoft.Extensions.DependencyInjection;
using RideAway.Infrastructure.Authentication;
using RideAway.Infrastructure.Notifications;
using RideAway.Infrastructure.Persistence.Repositories;
using RideAway.Application.IRepositories;
using Microsoft.Extensions.Configuration;
using RideAway.Application.IServices;
using RideAway.Application.Services;
using System.Reflection;
using RideAway.Application.IServices.INotification;
using RideAway.Application.IServices.IAuthentication;
using RideAway.Infrastructure.Mappers;
using RideAway.Application.Features.Rides.Handlers.Queries;
using MediatR;
using RideAway.Application.Common.Behaviors;
using RideAway.Application.Features.Rides.Handlers.Commands;
using RideAway.Domain.Service;

namespace RideAway.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            

            services.AddHttpClient<IGoogleMapsApi, GoogleMapsApiService>();

            // 3rd Party Services
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ISmsService, SmsService>();
            services.AddScoped<IStripePaymentService, StripePaymentService>();
            services.AddScoped<ILocationService, GoogleMapsLocationService>();
            services.AddScoped<IGoogleMapsApi, GoogleMapsApiService>();
            services.AddScoped<IGeoCodingService, GoogleGeocodingService>();

            // Repository Area

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRideRepository, RideRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();

            // Service Area

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IRideMatchingService, RideMatchingService>();
            services.AddScoped<IPaymentProcessingService, PaymentProcessingService>();
            services.AddScoped<IFareCalculationService, FareCalculationService>();
            services.AddScoped<IRideFactory, RideFactory>();

            //Mapper Area
            services.AddAutoMapper(typeof(PaymentProfile));
            services.AddAutoMapper(typeof(UserProfile));

            // MediatR Area
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAvailableRidesHandler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AcceptRideHandler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CancelRideHandler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CollectRiderHandler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CompleteRideHandler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateUserHandler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProcessPaymentCommandHandler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RequestRideHandler).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UpdateDriverLocationHandler).Assembly));

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));


            //behavior
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));




        }
    }
}
