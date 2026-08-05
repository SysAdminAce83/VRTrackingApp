using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;
using VRTrackingApp.Web.Services;
using VRTrackingApp.Web.Services.Compliance;
using VRTrackingApp.Web.Services.Exceptions;
using VRTrackingApp.Web.Services.MSRC;
using VRTrackingApp.Web.Services.NVD;
using VRTrackingApp.Web.Services.Remediation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// -----------------------------------------------------------------------------
// Authentication: on-prem Active Directory via Windows Integrated Auth (Kerberos)
// -----------------------------------------------------------------------------
// The user's identity is established by IIS/Windows (Single Sign-On) - the app
// never sees or stores a password. Access control + role still come from the
// UserAccount table in the database (see DomainRoleClaimsTransformation):
//   - user must exist in UserAccounts and be active  -> access granted, role applied
//   - user not found (production/SQL mode)            -> Access Denied
builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme);

// After Windows authenticates the user, map the domain account to the DB user
// record to attach the application role + an "enrolled" marker claim.
builder.Services.AddScoped<IClaimsTransformation, DomainRoleClaimsTransformation>();

// Only domain users that are enrolled in the app database may pass. Windows can
// authenticate anyone on the domain, so we additionally require the marker claim.
builder.Services.AddAuthorization(o =>
    o.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireClaim(DomainRoleClaimsTransformation.EnrolledClaimType, "true")
        .Build());

// Database: use EF Core InMemory by default (zero external dependencies, seeded with demo data)
// so the GUI runs anywhere. Set ConnectionStrings:UseInMemory=false to use SQL Server.
var useInMemory = builder.Configuration.GetValue("ConnectionStrings:UseInMemory", "true") != "false";
var connectionString = builder.Configuration.GetConnectionString("SqlServer");
if (useInMemory || string.IsNullOrWhiteSpace(connectionString))
{
    // A shared root makes the InMemory store behave like a real database: the
    // seeded data and every request's changes live in one place (otherwise each
    // scoped DbContext gets its own isolated copy and cross-request queries see 0 rows).
    var inMemoryRoot = new Microsoft.EntityFrameworkCore.Storage.InMemoryDatabaseRoot();
    builder.Services.AddSingleton(inMemoryRoot);
    builder.Services.AddDbContext<VRTrackingAppContext>(opt =>
        opt.UseInMemoryDatabase("VRTrackingAppDemo", inMemoryRoot));
}
else
{
    builder.Services.AddDbContext<VRTrackingAppContext>(opt =>
        opt.UseSqlServer(connectionString, sql => sql.MigrationsAssembly("VRTrackingApp.Data")));
}

builder.Services.AddScoped<ScanImportService>();
builder.Services.AddScoped<ScanIngestionService>();

// MSRC Enrichment Services
builder.Services.AddHttpClient<IMsrcService, MsrcService>(client =>
{
    client.BaseAddress = new Uri("https://api.msrc.microsoft.com/");
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddScoped<IVulnerabilityEnrichmentService, VulnerabilityEnrichmentService>();

// NVD Enrichment Services
builder.Services.AddHttpClient<INvdService, NvdService>(client =>
{
    client.BaseAddress = new Uri("https://services.nvd.nist.gov/rest/json/cves/2.0");
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddScoped<INvdEnrichmentService, NvdEnrichmentService>();

// Compliance / GRC Services
builder.Services.AddScoped<IComplianceControlService, ComplianceControlService>();
builder.Services.AddScoped<IFindingComplianceLinkService, FindingComplianceLinkService>();
builder.Services.AddScoped<IComplianceReportService, ComplianceReportService>();

// Exception module V2 services
builder.Services.AddScoped<VRTrackingApp.Web.Services.Exceptions.ExceptionRoutingService>();
builder.Services.AddScoped<VRTrackingApp.Web.Services.Exceptions.ExceptionWorkflowService>();
builder.Services.AddScoped<VRTrackingApp.Web.Services.Exceptions.ExceptionLifecycleService>();

// Notifications (in-app + optional email)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<VRTrackingApp.Web.Services.Notifications.UserNotificationService>();
builder.Services.AddSingleton<VRTrackingApp.Web.Services.Notifications.EmailTemplateService>();
var smtpHost = builder.Configuration["Email:Smtp:Host"];
if (!string.IsNullOrWhiteSpace(smtpHost))
    builder.Services.AddScoped<VRTrackingApp.Web.Services.Notifications.INotificationChannel>(
        _ => new VRTrackingApp.Web.Services.Notifications.EmailChannel(builder.Configuration));
builder.Services.AddScoped<VRTrackingApp.Web.Services.Notifications.NotificationService>();
builder.Services.AddScoped<VRTrackingApp.Web.Services.Notifications.INotificationService>(sp =>
    sp.GetRequiredService<VRTrackingApp.Web.Services.Notifications.NotificationService>());

// -----------------------------------------------------------------------------
// Automated remediation (check / install a missing patch on the target host).
// Provider is chosen at runtime: Simulated (default, safe for dev/demo) or the
// live WinRM/SSH providers when Remediation:Mode = "Live".
// -----------------------------------------------------------------------------
builder.Services.Configure<RemediationOptions>(
    builder.Configuration.GetSection(RemediationOptions.SectionName));
builder.Services.AddSingleton<RegistryPlaybookStore>();
builder.Services.AddScoped<PatchIdentifierParser>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<IRemediationProvider, SimulatedRemediationProvider>();
builder.Services.AddScoped<IRemediationProvider, WinRmWindowsUpdateProvider>();
builder.Services.AddScoped<IRemediationProvider, LinuxSshRemediationProvider>();
builder.Services.AddScoped<RemediationEngine>();
builder.Services.AddSingleton<IRemediationQueue, RemediationQueue>();
builder.Services.AddHostedService<RemediationBackgroundService>();
builder.Services.AddHostedService<VRTrackingApp.Web.Services.Exceptions.ExceptionLifecycleHostedService>();
builder.Services.AddHostedService<MsrcSyncBackgroundService>();
builder.Services.AddHostedService<NvdSyncBackgroundService>();

var app = builder.Build();

// Seed demo data for the console.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VRTrackingAppContext>();
    await DbSeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// A domain user who is authenticated but not enrolled in the app receives a 403;
// send them to a friendly Access Denied page instead of a blank error.
app.UseStatusCodePages(context =>
{
    var response = context.HttpContext.Response;
    if (response.StatusCode == StatusCodes.Status403Forbidden &&
        !context.HttpContext.Request.Path.StartsWithSegments("/Account/AccessDenied"))
    {
        response.Redirect("/Account/AccessDenied");
    }
    return Task.CompletedTask;
});

app.UseRouting();

app.UseAuthentication();

// DEV ONLY: when running under plain Kestrel (`dotnet run`) there is no IIS/Windows
// authentication, so inject a local admin identity so the UI is still usable.
// This never runs in Production and never runs once a real Windows user is present.
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            var claims = new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "Local Developer"),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "0"),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin"),
                new System.Security.Claims.Claim(DomainRoleClaimsTransformation.EnrolledClaimType, "true"),
            };
            context.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(claims, "DevFallback"));
        }
        await next();
    });
}

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();

