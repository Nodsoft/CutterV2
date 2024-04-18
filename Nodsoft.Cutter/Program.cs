using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Nodsoft.Cutter.Components;
using Nodsoft.Cutter.Data;
using OpenIddict.Validation.AspNetCore;
using Serilog;
using Serilog.Events;

namespace Nodsoft.Cutter;

public class Program
{
    private static IConfiguration _configuration = null!;
    
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console(applyThemeToRedirectedOutput: true)
            .CreateBootstrapLogger();
        
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        _configuration = builder.Configuration;
        
        // Add services to the container.
        ConfigureServices(builder.Services);

        WebApplication app = builder.Build();
        Configure(app);

        await using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
        {
            // Migrate the database
            await scope.ServiceProvider.GetRequiredService<CutterDbContext>().Database.MigrateAsync();
        }
        
        await app.RunAsync();
    }
    
    /// <summary>
    /// Adds services to the application's service collection.
    /// </summary>
    /// <param name="services">The services collection to add services to.</param>
    /// <returns>The updated services collection.</returns>
    public static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddSerilog(static (services, lc) => lc
            .ReadFrom.Configuration(services.GetRequiredService<IConfiguration>())
            .ReadFrom.Services(services)
        );
        
        // API
        services.AddControllers();
        
        // Blazor
        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        services.AddHttpContextAccessor();
        
        // EF Core / Postgres
        // Add from connection strings
        services.AddDbContext<CutterDbContext>(static (services, options) =>
        {
            options.UseNpgsql(services.GetRequiredService<IConfiguration>().GetConnectionString("Database"));
            options.UseOpenIddict<Guid>();
            options.UseSnakeCaseNamingConvention();
        });
        
        // Auth / OpenIddict using GitHub
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie();
        
        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<CutterDbContext>()
                    .ReplaceDefaultEntities<Guid>();
            })
            .AddClient(static options =>
            {
                // Allow the OpenIddict client to negotiate the authorization code flow.
                options.AllowAuthorizationCodeFlow();

                // Register the signing and encryption credentials used to protect
                // sensitive data like the state tokens produced by OpenIddict.
                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                options.UseAspNetCore()
                    .EnableRedirectionEndpointPassthrough();

                // Register the GitHub integration.
                options.UseWebProviders()
                    .AddGitHub(static options => options
                        .SetClientId(_configuration["Auth:GitHub:ClientId"] ?? throw new InvalidOperationException("GitHub Client ID not set."))
                        .SetClientSecret(_configuration["Auth:GitHub:ClientSecret"] ?? throw new InvalidOperationException("GitHub Client Secret not set."))
                        .SetRedirectUri("callback/login/github"));
            });
        
        // Services
        services.AddScoped<UserService>();
        
        return services;
    }
    
    /// <summary>
    /// Configures the application's request pipeline.
    /// </summary>
    /// <param name="app">The application to configure.</param>
    /// <returns>The updated application.</returns>
    private static WebApplication Configure(WebApplication app)
    {
        // Request Logging
        app.UseSerilogRequestLogging(options =>
        {
            // Customize the message template
            options.MessageTemplate = "{RequestScheme} {RequestMethod} {RequestPath} by {RequestClient} responded {StatusCode} in {Elapsed:0.0000} ms";
    
            // Attach additional properties to the request completion event
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestClient", httpContext.Connection.RemoteIpAddress);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme.ToUpperInvariant());
            };
        });
        
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapDefaultControllerRoute();
        
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        
        return app;
    }
}