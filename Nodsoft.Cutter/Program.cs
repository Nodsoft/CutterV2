using Nodsoft.Cutter.Components;

namespace Nodsoft.Cutter;

public class Program
{
    public static async Task Main(string[] args)
    {
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
        services.AddRazorComponents()
            .AddInteractiveServerComponents();
        
        return services;
    }
    
    /// <summary>
    /// Configures the application's request pipeline.
    /// </summary>
    /// <param name="app">The application to configure.</param>
    /// <param name="env">The hosting environment.</param>
    /// <returns>The updated application.</returns>
    public static WebApplication Configure(WebApplication app)
    {
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