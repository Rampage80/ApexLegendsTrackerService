using ApexLegendsTracker.Application.Players;
using ApexLegendsTracker.Infrastructure.Options;
using ApexLegendsTracker.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services
	.Configure<ApexApiOptions>(builder.Configuration.GetSection(ApexApiOptions.SectionName));

builder.Services.AddHttpClient<IApexTrackerService, ApexTrackerService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.MapGet("/api/v1/health", () => Results.Ok(new { status = "Healthy" }));
app.MapControllers();

app.Run();
