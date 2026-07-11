using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VRTrackingApp.Infrastructure.Data;
using VRTrackingApp.Infrastructure.Interfaces;
using VRTrackingApp.Infrastructure.Repositories;

namespace VRTrackingApp.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add database context
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // Add repositories
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IScanRepository, ScanRepository>();
            services.AddScoped<IAssetRepository, AssetRepository>();
            services.AddScoped<IVulnerabilityRepository, VulnerabilityRepository>();
            services.AddScoped<IAssetVulnerabilityRepository, AssetVulnerabilityRepository>();
            services.AddScoped<IReferenceRepository, ReferenceRepository>();

            return services;
        }
    }
}