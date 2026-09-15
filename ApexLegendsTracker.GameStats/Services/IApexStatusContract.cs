using ApexLegendsTracker.Shared;

namespace ApexLegendsTracker.GameStats.Service;

public interface IApexGlobalContract
{
	Task<MapRotationResult> GetMapRotationAsync(string? version, CancellationToken cancellationToken = default);

	Task<PredatorResult> GetPredatorThresholdsAsync(CancellationToken cancellationToken = default);
}