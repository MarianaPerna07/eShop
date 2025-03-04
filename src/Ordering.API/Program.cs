using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Instrumentation.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApplicationServices();
builder.Services.AddProblemDetails();

// Configuração do OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Ordering.API"))
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true; // Registrar exceções no tracing
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
            .AddOtlpExporter(options =>      
            {
                options.Endpoint = new Uri("http://localhost:14317");
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc; // Garantir que usa gRPC
            });
    });

var withApiVersioning = builder.Services.AddApiVersioning();

builder.AddDefaultOpenApi(withApiVersioning);

var app = builder.Build();

// Mapear os endpoints padrão
app.MapDefaultEndpoints();

// Adicionar logs para saber se o serviço iniciou corretamente
app.Logger.LogInformation("🚀 Ordering.API está rodando e enviando traces para {Endpoint}", "otel-collector:4317");

var orders = app.NewVersionedApi("Orders");

orders.MapOrdersApiV1()
      .RequireAuthorization();

app.UseDefaultOpenApi();
app.Run();
