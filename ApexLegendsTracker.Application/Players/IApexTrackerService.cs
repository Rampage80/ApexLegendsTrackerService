namespace ApexLegendsTracker.Application.Players;

public interface IApexTrackerService
{
	Task<PlayerLookupResult> QueryByNameAsync(string playerName, string platform, CancellationToken cancellationToken = default);
}