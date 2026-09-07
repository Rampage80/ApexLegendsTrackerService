using System.Net;
using System.Text.Json;
using ApexLegendsTracker.Service.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ApexLegendsTracker.Service.Services;

public sealed class ApexApiClient : IApexApiClient
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};

	private readonly HttpClient _httpClient;
	private readonly IMemoryCache _memoryCache;
	private readonly ApexApiOptions _options;
	private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);

	public ApexApiClient(
		HttpClient httpClient,
		IMemoryCache memoryCache,
		IOptions<ApexApiOptions> options)
	{
		_httpClient = httpClient;
		_memoryCache = memoryCache;
		_options = options.Value;

		if (_httpClient.BaseAddress is null)
		{
			_httpClient.BaseAddress = new Uri(_options.BaseUrl);
		}
	}

	public Task<T> GetCachedAsync<T>(string requestUri, CancellationToken cancellationToken = default)
	{
		string cacheKey = $"apex-api:{typeof(T).FullName}:{requestUri}";

		return _memoryCache.GetOrCreateAsync(
			cacheKey,
			entry =>
			{
				entry.AbsoluteExpirationRelativeToNow = CacheDuration;
				return GetAsync<T>(requestUri, cancellationToken);
			})!;
	}

	public async Task<T> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(_options.ApiKey))
		{
			throw new InvalidOperationException("Apex API key is not configured.");
		}

		using HttpRequestMessage request = new(HttpMethod.Get, requestUri);
		request.Headers.TryAddWithoutValidation("Authorization", _options.ApiKey);

		using HttpResponseMessage response = await _httpClient.SendAsync(
			request,
			HttpCompletionOption.ResponseHeadersRead,
			cancellationToken);

		if (!response.IsSuccessStatusCode)
		{
			throw new HttpRequestException(
				$"Apex API request failed with status {(int)response.StatusCode}.",
				null,
				response.StatusCode);
		}

		T? result = await DeserializeResponseAsync<T>(response, cancellationToken);
		if (result is null)
		{
			throw new HttpRequestException(
				"Apex API returned an empty response body.",
				null,
				HttpStatusCode.BadGateway);
		}

		return result;
	}

	private static async Task<T?> DeserializeResponseAsync<T>(
		HttpResponseMessage response,
		CancellationToken cancellationToken)
	{
		try
		{
			return await JsonSerializer.DeserializeAsync<T>(
				await response.Content.ReadAsStreamAsync(cancellationToken),
				JsonOptions,
				cancellationToken);
		}
		catch (JsonException exception)
		{
			throw new HttpRequestException(
				"Apex API returned an invalid JSON response.",
				exception,
				HttpStatusCode.BadGateway);
		}
	}
}