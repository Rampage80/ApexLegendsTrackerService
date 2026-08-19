using ApexLegendsTracker.Application.Players;
using ApexLegendsTracker.Service.Options;
using Microsoft.Extensions.Options;

namespace ApexLegendsTracker.Service.Services;

public sealed class ApexTrackerService : IApexTrackerService
{
	private readonly HttpClient _httpClient;
	private readonly ApexApiOptions _options;

	public ApexTrackerService(HttpClient httpClient, IOptions<ApexApiOptions> options)
	{
		_httpClient = httpClient;
		_options = options.Value;

		if (_httpClient.BaseAddress is null)
		{
			_httpClient.BaseAddress = new Uri(_options.BaseUrl);
		}
	}

	public async Task<PlayerLookupResult> QueryByNameAsync(
		string playerName,
		string platform,
		CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(_options.ApiKey))
		{
			throw new InvalidOperationException("Apex API key is not configured.");
		}

		string encodedPlayer = Uri.EscapeDataString(playerName);
		string encodedPlatform = Uri.EscapeDataString(platform);
		string requestUri = $"bridge?player={encodedPlayer}&platform={encodedPlatform}&version=5";

		using HttpRequestMessage request = new(HttpMethod.Get, requestUri);
		request.Headers.TryAddWithoutValidation("Authorization", _options.ApiKey);

		using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
		string body = await response.Content.ReadAsStringAsync(cancellationToken);

		if (!response.IsSuccessStatusCode)
		{
			throw new HttpRequestException(
				$"Apex API request failed with status {(int)response.StatusCode}.",
				null,
				response.StatusCode);
		}

		return new PlayerLookupResult(playerName, platform, body);
	}
}