using ApexLegendsTracker.Service.Services;
using ApexLegendsTracker.Shared;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace ApexLegendsTracker.Service.Tests;

public sealed class ApexTrackerServiceTests
{
	private static readonly TelemetryClient TelemetryClient = new(TelemetryConfiguration.CreateDefault());

	[Fact]
	public async Task QueryByNameAsync_EncodesRequestValuesAndEnrichesResult()
	{
		FakeApexApiClient apiClient = new(new PlayerLookupResult());
		ApexTrackerService service = new(apiClient, TelemetryClient);

		PlayerLookupResult result = await service.QueryByNameAsync("Player One", "PS4");

		Assert.Equal("Player One", result.PlayerName);
		Assert.Equal("PS4", result.Platform);
		Assert.Equal("bridge?player=Player%20One&platform=PS4&version=5", apiClient.RequestUri);
	}

	[Fact]
	public async Task GetMapRotationAsync_UsesDefaultPathWhenVersionIsMissing()
	{
		FakeApexApiClient apiClient = new(new MapRotationResponse());
		ApexTrackerService service = new(apiClient, TelemetryClient);

		await service.GetMapRotationAsync(" ");

		Assert.Equal("maprotation", apiClient.RequestUri);
	}

	[Fact]
	public async Task GetMapRotationAsync_IncludesVersionWhenProvided()
	{
		FakeApexApiClient apiClient = new(new MapRotationResponse());
		ApexTrackerService service = new(apiClient, TelemetryClient);

		await service.GetMapRotationAsync("2");

		Assert.Equal("maprotation?version=2", apiClient.RequestUri);
	}

	[Fact]
	public async Task GetPredatorThresholdsAsync_UsesPredatorPath()
	{
		FakeApexApiClient apiClient = new(new PredatorResponse());
		ApexTrackerService service = new(apiClient, TelemetryClient);

		await service.GetPredatorThresholdsAsync();

		Assert.Equal("predator", apiClient.RequestUri);
	}

	private sealed class FakeApexApiClient : IApexApiClient
	{
		private readonly object _response;

		public FakeApexApiClient(object response)
		{
			_response = response;
		}

		public string? RequestUri { get; private set; }

		public Task<T> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default)
		{
			RequestUri = requestUri;
			return Task.FromResult((T)_response);
		}

		public Task<T> GetCachedAsync<T>(string requestUri, CancellationToken cancellationToken = default)
		{
			RequestUri = requestUri;
			return Task.FromResult((T)_response);
		}
	}
}