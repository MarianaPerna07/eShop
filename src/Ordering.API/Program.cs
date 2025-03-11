using System.Diagnostics.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Instrumentation.EntityFrameworkCore;
using eShop.Ordering.API.Infrastructure.Telemetry;
using eShop.Ordering.API.Infrastructure.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApplicationServices();
builder.Services.AddProblemDetails();

// OpenTelemetry Configuration (Tracing)
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Ordering.API"))
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true; 
            })
            .AddHttpClientInstrumentation(options =>
            {
                options.RecordException = true;
            })
            .AddSqlClientInstrumentation(options =>
            {
                options.SetDbStatementForText = true;
                options.RecordException = true;
            })
            .AddSource("Ordering.API")
            .AddProcessor(new SensitiveDataMaskingProcessor())
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://localhost:14317");
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            });

        Console.WriteLine("✅ OpenTelemetry com SensitiveDataMaskingProcessor carregado!");
    });


var withApiVersioning = builder.Services.AddApiVersioning();
builder.AddDefaultOpenApi(withApiVersioning);

// Metrics Configuration
var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://localhost:14317";
var meter = new Meter("Ordering.API");
builder.Services.AddSingleton(meter);
builder.Services.AddSingleton(meter.CreateCounter<long>("order_placed_count", description: "Number of orders placed"));

builder.Services.AddOpenTelemetry()
    .WithMetrics(metricsBuilder =>
    {
        metricsBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Ordering.API"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMeter("Ordering.API")
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://localhost:14317");
            });
    });

// Logging Configuration
builder.Logging.AddJsonConsole(options =>
{
    options.JsonWriterOptions = new System.Text.Json.JsonWriterOptions
    {
        Indented = true
    };
    options.IncludeScopes = true;
    // If you want to mask sensitive data, uncomment the line below:
    // options.Converters.Add(new SensitiveDataJsonConverter());
});

var app = builder.Build();

app.MapDefaultEndpoints();

app.Logger.LogInformation("Ordering.API está rodando e enviando traces para {Endpoint}", "otel-collector:4317");

var orders = app.NewVersionedApi("Orders");
orders.MapOrdersApiV1();
// If you want to require authorization, uncomment the line below:
//      .RequireAuthorization(); 

app.UseDefaultOpenApi();
app.Run();
