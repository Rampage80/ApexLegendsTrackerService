namespace ApexLegendsTracker.Infrastructure.Options;

public sealed class ApexApiOptions
{
	public const string SectionName = "ApexApi";

	public string BaseUrl { get; init; } = "https://api.apexlegendsstatus.com/";

	public string ApiKey { get; init; } = "bacc6e94ea92496371dde78f14bd8ec9";
}