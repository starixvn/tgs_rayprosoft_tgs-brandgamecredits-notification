using System.Net.Mime;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Mvc;

using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Tgs.BrandGameCredits.Handler.Common;
using Tgs.BrandGameCredits.Handler.Messages;

var filterInstrumentations = new[] { "html", "ico", "png", "jpg" };

var builder = WebApplication.CreateBuilder(args);

builder.Logging
    .AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.ParseStateValues = true;
        _ = options.AddFluentdForwardExporter(fluentOptions =>
        {
            var nodeIp = builder.Configuration.GetValue<string>("NODE_IP")!;
            fluentOptions.Host = nodeIp;
            fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
            fluentOptions.UseMessagePack();
        });
    });

// Add services to the container.

builder.Services
    .AddRouting(options => options.LowercaseUrls = true)
    .AddControllers(options =>
    {
        options.Filters.Add(new ProducesAttribute(MediaTypeNames.Application.Json));
        options.Filters.Add(new ConsumesAttribute(MediaTypeNames.Application.Json));
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFileNames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.xml");
    foreach (var xmlFileName in xmlFileNames) c.IncludeXmlComments(xmlFileName);
});

builder.Services.AddBrandGameCredits(
    (options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        options.ConnectionString = configuration.GetConnectionString("Redis")!;
    },
    (options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        options.BaseUrl = configuration.GetValue<Uri>("BrandGameCreditsSetting:BrandGameCreditsClientBaseUrl")!;
    },
    (options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        options.BaseUrl = configuration.GetValue<Uri>("BrandGameCreditsSetting:NotificationContactsClientBaseUrl")!;
    },
    (options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        options.BaseUrl = configuration.GetValue<Uri>("BrandGameCreditsSetting:NotificationEmailClientBaseUrl")!;
    },
    (options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        configuration.GetSection("EmailSettings").Bind(options);
    });

builder.Services
    .AddMessageQueue()
    .AddNatsMessageQueue(config => config
    .ConfigQueueOptions((options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        options.Url = configuration.GetConnectionString("Nats")!;
    })
    .AddHandler<NotificationMessageHandler>(BrandGameCreditsHandlerNatsConst.Update, string.Concat(BrandGameCreditsHandlerNatsConst.Update, ".notification"))
    .AddHandler<NotificationMessageHandler>("brand.game.credits.notification.manual", "brand.game.credits.notification.manual"))
    .ClearAllExchanges()
    .AddNatsGlobPatternExchange(BrandGameCreditsHandlerNatsConst.Update)
    .AddNatsGlobPatternExchange("brand.game.credits.notification.manual");

builder.Services
    .AddOpenTelemetry()
    .WithTracing(builder => builder
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddEnvironmentVariableDetector())
        .AddSource("Tgs.*")
        .AddSource("TGS.*")
        .AddSource("Valhalla.*")
        .AddAspNetCoreInstrumentation(options => options.Filter =
        httpContext => !httpContext.Request.Path.HasValue
            || !httpContext.Request.Path.StartsWithSegments("/swagger")
            && !httpContext.Request.Path.StartsWithSegments("/healthz")
            && !httpContext.Request.Path.StartsWithSegments("/profiler/")
            && !httpContext.Request.Path.StartsWithSegments("/hangfire")
            && !httpContext.Request.Path.StartsWithSegments("/metrics")
            && !filterInstrumentations.Any(ext => httpContext.Request.Path.Value.EndsWith($".{ext}")))
        .AddHttpClientInstrumentation()
        .AddGrpcClientInstrumentation()
        .AddGrpcCoreInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
        .AddOtlpExporter())
    .WithMetrics(meterBuiler => meterBuiler
    .SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddEnvironmentVariableDetector())
        .AddRuntimeInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter())
    .Services
    .AddInterceptionService()
    .AddServiceUtiltiyInterceptors();

builder.Services.AddHealthChecks();

var app = builder.Build();

_ = app.UseSwagger();
_ = app.UseSwaggerUI();

_ = app.UseAuthorization();

_ = app.MapControllers();

app.MapHealthChecks("/healthz");

app.Run();
