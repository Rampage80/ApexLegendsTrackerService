using ApexLegendsTracker.Shared;
using ApexLegendsTracker.GameStats.Options;
using ApexLegendsTracker.GameStats.Service;
using ApexLegendsTracker.AIChat.Options;
using ApexLegendsTracker.AIChat.ChatClients;
using ApexLegendsTracker.AIChat.Service;
using ApexLegendsTracker.Common.Caching;
using ApexLegendsTracker.Common.Constants;
using ApexLegendsTracker.WebAPI.Controllers;
using Microsoft.AspNetCore.RateLimiting;
using Scalar.AspNetCore;

const string WebClientCorsPolicy = "WebClient";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers().AddJsonOptions(options =>
{
	// Chat responses expose ChatSource as a readable string (e.g. "Knowledge") rather than a raw number.
	options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.AddMemoryCache();

// Custom env var name (not the SDK default APPLICATIONINSIGHTS_CONNECTION_STRING); no-ops if unset.
string? appInsightsConnectionString = builder.Configuration["APEXSERVICE_APPINSIGHTS_CONNECTION_STRING"]
	?? Environment.GetEnvironmentVariable("APEXSERVICE_APPINSIGHTS_CONNECTION_STRING");

if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
{
	builder.Services.AddApplicationInsightsTelemetry(options =>
	{
		options.ConnectionString = appInsightsConnectionString;
	});
}

// Custom env var name, kept out of appsettings/source; no-ops if unset.
string? apexAIChatApiKey = builder.Configuration["APEXSERVICE_OPENAIKEY"]
	?? Environment.GetEnvironmentVariable("APEXSERVICE_OPENAIKEY");

if (!string.IsNullOrWhiteSpace(apexAIChatApiKey))
{
	builder.Services.PostConfigure<ApexAIChatOptions>(options => options.ApiKey = apexAIChatApiKey);
}

builder.Services
	.Configure<ApexApiOptions>(builder.Configuration.GetSection(ApexApiOptions.SectionName));
builder.Services
	.Configure<ApexAIChatOptions>(builder.Configuration.GetSection(ApexAIChatOptions.SectionName));


builder.Services.AddHttpClient<IApexApiClient, ApexApiClient>();
builder.Services.AddTransient<ApexTrackerService>();
builder.Services.AddTransient<IApexPlayerContract>(services => services.GetRequiredService<ApexTrackerService>());
builder.Services.AddTransient<IApexGlobalContract>(services => services.GetRequiredService<ApexTrackerService>());

builder.Services.AddSingleton<ICacheProvider, MemoryCacheProvider>();
builder.Services.AddSingleton<IChatClient, OpenAiChatClient>();
builder.Services.AddTransient<IApexAIChatService, ApexAIChatService>();

builder.Services.AddRateLimiter(options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
	options.AddFixedWindowLimiter(RateLimiterPolicies.Chat, limiterOptions =>
	{
		limiterOptions.PermitLimit = 10;
		limiterOptions.Window = TimeSpan.FromMinutes(1);
		limiterOptions.QueueLimit = 0;
	});
});

// Origins the client app (ApexLegendsTrackerWeb) is served from; configure via Cors:AllowedOrigins.
string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
	options.AddPolicy(WebClientCorsPolicy, policy =>
	{
		if (allowedOrigins.Length > 0)
		{
			// Request-Context exposure lets the Web tier's App Insights JS SDK correlate its calls with this API's telemetry.
			policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("Request-Context");
		}
	});
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	// Swagger-style interactive docs at /scalar/v1, backed by the /openapi/v1.json document.
	app.MapScalarApiReference();
}

app.UseCors(WebClientCorsPolicy);
app.UseRateLimiter();

app.MapGet("/api/v1/health", () => Results.Ok(new { status = "Healthy" }));
app.MapControllers();

app.Run();
