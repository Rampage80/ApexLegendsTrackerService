using ApexLegendsTracker.Shared;
using ApexLegendsTracker.Shared.Telemetry;
using Microsoft.ApplicationInsights;

namespace ApexLegendsTracker.GameStats.Service;

public sealed class ApexTrackerService : IApexPlayerContract, IApexGlobalContract
{
	private readonly IApexApiClient _apiClient;
	private readonly TelemetryClient _telemetryClient;

	public ApexTrackerService(IApexApiClient apiClient, TelemetryClient telemetryClient)
	{
		_apiClient = apiClient;
		_telemetryClient = telemetryClient;
	}

	public async Task<PlayerLookupResult> QueryByNameAsync(
		string playerName,
		string platform,
		CancellationToken cancellationToken = default)
	{
		string encodedPlayer = Uri.EscapeDataString(playerName);
		string encodedPlatform = Uri.EscapeDataString(platform);
		string requestUri = $"bridge?player={encodedPlayer}&platform={encodedPlatform}&version=5";

		_telemetryClient.TrackEvent(
			TelemetryEvents.PlayerLookupRequested,
			new Dictionary<string, string> { [TelemetryProperties.Platform] = platform });

		try
		{
			PlayerLookupResult result = await _apiClient.GetAsync<PlayerLookupResult>(requestUri, cancellationToken);

			result.PlayerName = playerName;
			result.Platform = platform;

			_telemetryClient.TrackEvent(
				TelemetryEvents.PlayerLookupSucceeded,
				new Dictionary<string, string> { [TelemetryProperties.Platform] = platform });

			return result;
		}
		catch (Exception exception)
		{
			_telemetryClient.TrackEvent(
				TelemetryEvents.PlayerLookupFailed,
				new Dictionary<string, string>
				{
					[TelemetryProperties.Platform] = platform,
					[TelemetryProperties.ErrorMessage] = exception.Message
				});
			throw;
		}
	}

	public Task<MapRotationResult> GetMapRotationAsync(
		string? version,
		CancellationToken cancellationToken = default)
	{
		string requestUri = string.IsNullOrWhiteSpace(version)
			? "maprotation"
			: $"maprotation?version={Uri.EscapeDataString(version)}";

		_telemetryClient.TrackEvent(
			TelemetryEvents.MapRotationRequested,
			new Dictionary<string, string> { [TelemetryProperties.MapRotationVersion] = version ?? "default" });

		return _apiClient.GetCachedAsync<MapRotationResult>(requestUri, cancellationToken);
	}

	public Task<PredatorResult> GetPredatorThresholdsAsync(CancellationToken cancellationToken = default)
	{
		_telemetryClient.TrackEvent(TelemetryEvents.PredatorThresholdsRequested);

		return _apiClient.GetCachedAsync<PredatorResult>("predator", cancellationToken);
	}
}