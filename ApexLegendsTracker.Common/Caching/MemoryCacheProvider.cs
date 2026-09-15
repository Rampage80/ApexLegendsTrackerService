using Microsoft.Extensions.Caching.Memory;

namespace ApexLegendsTracker.Common.Caching;

public sealed class MemoryCacheProvider : ICacheProvider
{
	private readonly IMemoryCache _memoryCache;

	public MemoryCacheProvider(IMemoryCache memoryCache)
	{
		_memoryCache = memoryCache;
	}

	public Task<T> GetOrCreateAsync<T>(
		string key,
		TimeSpan duration,
		Func<CancellationToken, Task<T>> factory,
		CancellationToken cancellationToken = default)
	{
		return _memoryCache.GetOrCreateAsync(
			key,
			entry =>
			{
				entry.AbsoluteExpirationRelativeToNow = duration;
				return factory(cancellationToken);
			})!;
	}
}
