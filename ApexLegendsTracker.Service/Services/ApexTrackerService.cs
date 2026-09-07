using ApexLegendsTracker.Shared;

namespace ApexLegendsTracker.Service.Services;

public sealed class ApexTrackerService : IApexPlayerContract, IApexGlobalContract
{
	private readonly IApexApiClient _apiClient;

	public ApexTrackerService(IApexApiClient apiClient)
	{
		_apiClient = apiClient;
	}

	public async Task<PlayerLookupResult> QueryByNameAsync(
		string playerName,
		string platform,
		CancellationToken cancellationToken = default)
	{
		string encodedPlayer = Uri.EscapeDataString(playerName);
		string encodedPlatform = Uri.EscapeDataString(platform);
		string requestUri = $"bridge?player={encodedPlayer}&platform={encodedPlatform}&version=5";

		PlayerLookupResult result = await _apiClient.GetAsync<PlayerLookupResult>(requestUri, cancellationToken);

		result.PlayerName = playerName;
		result.Platform = platform;

		return result;
	}

	public Task<MapRotationResponse> GetMapRotationAsync(
		string? version,
		CancellationToken cancellationToken = default)
	{
		string requestUri = string.IsNullOrWhiteSpace(version)
			? "maprotation"
			: $"maprotation?version={Uri.EscapeDataString(version)}";

		return _apiClient.GetCachedAsync<MapRotationResponse>(requestUri, cancellationToken);
	}

	public Task<PredatorResponse> GetPredatorThresholdsAsync(CancellationToken cancellationToken = default)
	{
		return _apiClient.GetCachedAsync<PredatorResponse>("predator", cancellationToken);
	}
}