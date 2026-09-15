using ApexLegendsTracker.Shared;

namespace ApexLegendsTracker.GameStats.Service;

/// <summary>Internal query signature implemented by <see cref="ApexTrackerService"/> and consumed by the WebAPI's players controller.</summary>
public interface IApexPlayerContract
{
	Task<PlayerLookupResult> QueryByNameAsync(string playerName, string platform, CancellationToken cancellationToken = default);
}
