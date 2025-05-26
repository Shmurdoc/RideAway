using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;
using RideAway.Application.Services;
using RideAway.Application.IServices;

namespace RideAway.Application.DI
{
    public static class DependencyInjection
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            services.AddScoped<IRideMatchingService, RideMatchingService>();
            services.AddScoped<IPaymentProcessingService, PaymentProcessingService>();
            services.AddScoped<IFareCalculationService, FareCalculationService>();

        }
    }
}
