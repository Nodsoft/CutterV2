using Nodsoft.Cutter.Components;
using Serilog;
using Serilog.Events;

namespace Nodsoft.Cutter;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console(applyThemeToRedirectedOutput: true)
            .CreateBootstrapLogger();
        
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        
        // Add services to the container.
        ConfigureServices(builder.Services);

        WebApplication app = builder.Build();
        Configure(app);
        
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
        
        // Blazor
        services.AddRazorComponents()
            .AddInteractiveServerComponents();
        
        return services;
    }
    
    /// <summary>
    /// Configures the application's request pipeline.
    /// </summary>
    /// <param name="app">The application to configure.</param>
    /// <returns>The updated application.</returns>
    public static WebApplication Configure(WebApplication app)
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

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        
        return app;
    }
}