using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VRTrackingApp.Data.Models;

/// <summary>
/// Used by the EF Core tools at design time so migrations are generated against
/// SQL Server regardless of the runtime provider (the app defaults to InMemory
/// in demo mode). The runtime app registers the context itself in Program.cs.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<VRTrackingAppContext>
{
    public VRTrackingAppContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("VRT_SQL_CONNECTION") ??
            "Server=(localdb)\\mssqllocaldb;Database=VRTrackingApp;Trusted_Connection=True;MultipleActiveResultSets=true";

        var optionsBuilder = new DbContextOptionsBuilder<VRTrackingAppContext>();
        optionsBuilder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly("VRTrackingApp.Data"));
        return new VRTrackingAppContext(optionsBuilder.Options);
    }
}
