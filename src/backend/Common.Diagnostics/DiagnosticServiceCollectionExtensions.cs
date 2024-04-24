using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Azure.Monitor.OpenTelemetry.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class DiagnosticServiceCollectionExtensions
{
    public static IServiceCollection AddObservability(this IServiceCollection services,
        string serviceName,
        IConfiguration configuration,
        string[]? meeterNames = null)
    {
        var resource = ResourceBuilder.CreateDefault().AddService(serviceName: serviceName, serviceVersion: "1.0");

        // did they ask for azure monitor?
        var otelBuilder = services.AddOpenTelemetry();

        var zipkinUrl = configuration["ZIPKIN_URL"];
        var applicationInsightsConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

        if (!string.IsNullOrEmpty(applicationInsightsConnectionString))
        {
            otelBuilder.UseAzureMonitor();
        }

        // add the metrics and traces
        otelBuilder
            .WithMetrics(metrics =>
            {
                metrics
              .SetResourceBuilder(resource)
              .AddRuntimeInstrumentation()
              .AddAspNetCoreInstrumentation()
              .AddHttpClientInstrumentation()
              .AddEventCountersInstrumentation(c =>
                {
                    c.AddEventSources(
                        "Microsoft.AspNetCore.Hosting",
                        "Microsoft-AspNetCore-Server-Kestrel",
                        "System.Net.Http",
                        "System.Net.Sockets");
                })
              .AddView("request-duration", new ExplicitBucketHistogramConfiguration
                {
                    Boundaries = new double[] { 0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 }
                })
              .AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel")
              .AddPrometheusExporter(o => o.DisableTotalNameSuffixForCounters = true);

                // add any additional meters provided by the caller
                if (meeterNames is not null)
                {
                    foreach (var name in meeterNames)
                    {
                        metrics.AddMeter(name);
                    }
                }

            })
            .WithTracing(tracing =>
            {
                tracing.SetResourceBuilder(resource)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation();

                if(!string.IsNullOrEmpty(zipkinUrl))
                {
                    tracing.AddZipkinExporter(zipkin =>
                    {

                        zipkin.Endpoint = new Uri($"{zipkinUrl}/api/v2/spans");
                    });
                }       
            });

        return services;
    }

    public static void MapObservability(this IEndpointRouteBuilder routes)
    {
        routes.MapPrometheusScrapingEndpoint();
    }
}