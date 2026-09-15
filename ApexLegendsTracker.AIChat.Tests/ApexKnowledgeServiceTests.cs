using ApexLegendsTracker.Common.Caching;
using ApexLegendsTracker.AIChat.Options;
using ApexLegendsTracker.AIChat.Service;
using ApexLegendsTracker.AIChat.ChatClients;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ApexLegendsTracker.AIChat.Tests;

public sealed class ApexAIChatServiceTests
{
	[Fact]
	public async Task AskAsync_SendsAnyQuestionToTheModel()
	{
		FakeApexChatClient client = new("Here is a useful answer.");
		ApexAIChatService service = CreateService(client);

		string reply = await service.AskAsync("what's the weather like today");

		Assert.Equal("Here is a useful answer.", reply);
		Assert.Equal(1, client.CallCount);
	}

	[Fact]
	public async Task AskAsync_CachesRepeatedQuestion_OnlyCallsModelOnce()
	{
		FakeApexChatClient client = new("Rotate early and hold high ground.");
		ApexAIChatService service = CreateService(client);

		string first = await service.AskAsync("What is the best way to rotate in ranked?");
		string second = await service.AskAsync("what is the best way to rotate in ranked?");

		Assert.Equal("Rotate early and hold high ground.", first);
		Assert.Equal(first, second);
		Assert.Equal(1, client.CallCount);
	}

	[Fact]
	public async Task AskAsync_SurfacesUpstreamFailureAsHttpRequestException()
	{
		FakeApexChatClient client = new(exception: new HttpRequestException("boom"));
		ApexAIChatService service = CreateService(client);

		HttpRequestException exception = await Assert.ThrowsAsync<HttpRequestException>(
			() => service.AskAsync("what's the best loadout for close range fights"));

		Assert.Equal("boom", exception.Message);
	}

	[Fact]
	public async Task AskAsync_PassesQuestionAndOutputLimitToChatClient()
	{
		FakeApexChatClient client = new("Rotate early.");
		ApexAIChatService service = CreateService(client, options => new ApexAIChatOptions
		{
			ApiKey = options.ApiKey,
			MaxOutputTokens = 123
		});

		await service.AskAsync("what is the best way to rotate in ranked?");

		Assert.Equal("what is the best way to rotate in ranked?", client.UserPrompt);
		Assert.Equal(123, client.MaxOutputTokens);
	}

	private static ApexAIChatService CreateService(
		IChatClient client,
		Func<ApexAIChatOptions, ApexAIChatOptions>? configure = null)
	{
		ICacheProvider cacheProvider = new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions()));
		ApexAIChatOptions optionsValue = new() { ApiKey = "test-key" };
		if (configure is not null)
		{
			optionsValue = configure(optionsValue);
		}
		IOptions<ApexAIChatOptions> options = Microsoft.Extensions.Options.Options.Create(optionsValue);

		return new ApexAIChatService(
			client,
			cacheProvider,
			options);
	}

	private sealed class FakeApexChatClient : IChatClient
	{
		private readonly string? _reply;
		private readonly Exception? _exception;

		public FakeApexChatClient(string? reply = null, Exception? exception = null)
		{
			_reply = reply;
			_exception = exception;
		}

		public int CallCount { get; private set; }

		public string UserPrompt { get; private set; } = string.Empty;

		public int MaxOutputTokens { get; private set; }

		public Task<string> CompleteAsync(
			string systemPrompt,
			string userPrompt,
			int maxOutputTokens,
			CancellationToken cancellationToken)
		{
			CallCount++;
			UserPrompt = userPrompt;
			MaxOutputTokens = maxOutputTokens;
			return _exception is not null
				? Task.FromException<string>(_exception)
				: Task.FromResult(_reply ?? string.Empty);
		}
	}
}
