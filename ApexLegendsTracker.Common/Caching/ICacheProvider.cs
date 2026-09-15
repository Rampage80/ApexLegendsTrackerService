namespace ApexLegendsTracker.Common.Caching;

// Single seam for cache storage; swap the registered implementation to move from in-process to distributed later.
public interface ICacheProvider
{
	Task<T> GetOrCreateAsync<T>(
		string key,
		TimeSpan duration,
		Func<CancellationToken, Task<T>> factory,
		CancellationToken cancellationToken = default);
}
