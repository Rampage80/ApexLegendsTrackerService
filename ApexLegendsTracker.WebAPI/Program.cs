using ApexLegendsTracker.Shared;
using ApexLegendsTracker.Service.Options;
using ApexLegendsTracker.Service.Services;
using Scalar.AspNetCore;

const string WebClientCorsPolicy = "WebClient";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services
	.Configure<ApexApiOptions>(builder.Configuration.GetSection(ApexApiOptions.SectionName));

builder.Services.AddHttpClient<IPlayerLookupContract, ApexTrackerService>();

// Origins the client app (ApexLegendsTrackerWeb) is served from; configure via Cors:AllowedOrigins.
string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
	options.AddPolicy(WebClientCorsPolicy, policy =>
	{
		if (allowedOrigins.Length > 0)
		{
			policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
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
