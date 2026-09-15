using ApexLegendsTracker.Shared;

namespace ApexLegendsTracker.GameStats.Service;

public interface IApexGlobalContract
{
	Task<MapRotationResponse> GetMapRotationAsync(string? version, CancellationToken cancellationToken = default);

	Task<PredatorResponse> GetPredatorThresholdsAsync(CancellationToken cancellationToken = default);
}