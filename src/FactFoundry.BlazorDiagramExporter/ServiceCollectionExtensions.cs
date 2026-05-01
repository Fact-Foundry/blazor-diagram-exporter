using Microsoft.Extensions.DependencyInjection;

namespace FactFoundry.BlazorDiagramExporter;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorDiagramExporter(this IServiceCollection services)
    {
        services.AddScoped<ExporterJsInterop>();
        services.AddScoped(sp => new DiagramExporter(sp.GetRequiredService<ExporterJsInterop>()));
        return services;
    }
}
