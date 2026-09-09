using ApexLegendsTracker.Shared;
using ApexLegendsTracker.Service.Options;
using ApexLegendsTracker.Service.Services;
using Scalar.AspNetCore;

const string WebClientCorsPolicy = "WebClient";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
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

builder.Services
	.Configure<ApexApiOptions>(builder.Configuration.GetSection(ApexApiOptions.SectionName));

builder.Services.AddHttpClient<IApexApiClient, ApexApiClient>();
builder.Services.AddTransient<ApexTrackerService>();
builder.Services.AddTransient<IApexPlayerContract>(services => services.GetRequiredService<ApexTrackerService>());
builder.Services.AddTransient<IApexGlobalContract>(services => services.GetRequiredService<ApexTrackerService>());

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

app.MapGet("/api/v1/health", () => Results.Ok(new { status = "Healthy" }));
app.MapControllers();

app.Run();
