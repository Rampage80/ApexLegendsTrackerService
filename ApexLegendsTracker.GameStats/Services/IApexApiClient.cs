namespace ApexLegendsTracker.GameStats.Service;

public interface IApexApiClient
{
	Task<T> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default);

	Task<T> GetCachedAsync<T>(string requestUri, CancellationToken cancellationToken = default);
}