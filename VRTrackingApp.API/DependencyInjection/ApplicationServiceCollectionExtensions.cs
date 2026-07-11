using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VRTrackingApp.API.Services;

namespace VRTrackingApp.API.DependencyInjection
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add application services
            services.AddScoped<IFileUploadService, FileUploadService>();

            return services;
        }
    }
}