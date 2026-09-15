using System.Net;
using ApexLegendsTracker.Common.Caching;
using ApexLegendsTracker.GameStats.Options;
using ApexLegendsTracker.GameStats.Service;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ApexLegendsTracker.GameStats.Tests;

public sealed class ApexApiClientTests
{
	[Fact]
	public async Task GetAsync_SendsGetRequestWithAuthorizationAndDeserializesResponse()
	{
		FakeHttpMessageHandler handler = new(HttpStatusCode.OK, "{\"value\":42}");
		ApexApiClient client = CreateClient(handler, "test-key");

		TestResponse response = await client.GetAsync<TestResponse>("maprotation?version=2");

		Assert.Equal(42, response.Value);
		Assert.Equal(HttpMethod.Get, handler.Method);
		Assert.Equal("https://api.example/maprotation?version=2", handler.RequestUri?.ToString());
		Assert.Equal("test-key", handler.Authorization);
	}

	[Fact]
	public async Task GetAsync_ThrowsWithUpstreamStatusCode()
	{
		ApexApiClient client = CreateClient(new FakeHttpMessageHandler(HttpStatusCode.TooManyRequests, "{}"), "test-key");

		HttpRequestException exception = await Assert.ThrowsAsync<HttpRequestException>(
			() => client.GetAsync<TestResponse>("predator"));

		Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
	}

	[Fact]
	public async Task GetAsync_MapsInvalidJsonToBadGateway()
	{
		ApexApiClient client = CreateClient(new FakeHttpMessageHandler(HttpStatusCode.OK, "not-json"), "test-key");

		HttpRequestException exception = await Assert.ThrowsAsync<HttpRequestException>(
			() => client.GetAsync<TestResponse>("predator"));

		Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
	}

	[Fact]
	public async Task GetAsync_RejectsMissingApiKeyBeforeSendingRequest()
	{
		FakeHttpMessageHandler handler = new(HttpStatusCode.OK, "{}");
		ApexApiClient client = CreateClient(handler, string.Empty);

		await Assert.ThrowsAsync<InvalidOperationException>(
			() => client.GetAsync<TestResponse>("predator"));

		Assert.Null(handler.RequestUri);
	}

	[Fact]
	public async Task GetCachedAsync_ReusesResponseForSameRequest()
	{
		FakeHttpMessageHandler handler = new(HttpStatusCode.OK, "{\"value\":42}");
		ApexApiClient client = CreateClient(handler, "test-key");

		await client.GetCachedAsync<TestResponse>("maprotation");
		await client.GetCachedAsync<TestResponse>("maprotation");

		Assert.Equal(1, handler.RequestCount);
	}

	private static ApexApiClient CreateClient(FakeHttpMessageHandler handler, string apiKey)
	{
		HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://api.example/") };
		return new ApexApiClient(
			httpClient,
			new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())),
			Microsoft.Extensions.Options.Options.Create(new ApexApiOptions { ApiKey = apiKey }));
	}

	private sealed class TestResponse
	{
		public int Value { get; init; }
	}

	private sealed class FakeHttpMessageHandler : HttpMessageHandler
	{
		private readonly HttpStatusCode _statusCode;
		private readonly string _body;

		public FakeHttpMessageHandler(HttpStatusCode statusCode, string body)
		{
			_statusCode = statusCode;
			_body = body;
		}

		public HttpMethod? Method { get; private set; }

		public Uri? RequestUri { get; private set; }

		public string? Authorization { get; private set; }

		public int RequestCount { get; private set; }

		protected override Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			RequestCount++;
			Method = request.Method;
			RequestUri = request.RequestUri;
			Authorization = request.Headers.TryGetValues("Authorization", out IEnumerable<string>? values)
				? values.Single()
				: null;

			return Task.FromResult(new HttpResponseMessage(_statusCode)
			{
				Content = new StringContent(_body)
			});
		}
	}
}