using ApexLegendsTracker.Application.Players;
using ApexLegendsTracker.Service.Options;
using ApexLegendsTracker.Service.Services;

const string WebClientCorsPolicy = "WebClient";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services
	.Configure<ApexApiOptions>(builder.Configuration.GetSection(ApexApiOptions.SectionName));

builder.Services.AddHttpClient<IApexTrackerService, ApexTrackerService>();

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
}

app.UseCors(WebClientCorsPolicy);

app.MapGet("/api/v1/health", () => Results.Ok(new { status = "Healthy" }));
app.MapControllers();

app.Run();
