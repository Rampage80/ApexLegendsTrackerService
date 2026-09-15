using System.Net;
using System.Security.Cryptography;
using System.Text;
using ApexLegendsTracker.Common.Caching;
using ApexLegendsTracker.AIChat.Options;
using ApexLegendsTracker.AIChat.ChatClients;
using Microsoft.Extensions.Options;

namespace ApexLegendsTracker.AIChat.Service;

public sealed class ApexAIChatService : IApexAIChatService
{
	private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

	private readonly IChatClient _chatClient;
	private readonly ICacheProvider _cacheProvider;
	private readonly ApexAIChatOptions _options;

	public ApexAIChatService(
		IChatClient chatClient,
		ICacheProvider cacheProvider,
		IOptions<ApexAIChatOptions> options)
	{
		_chatClient = chatClient;
		_cacheProvider = cacheProvider;
		_options = options.Value;
	}

	public async Task<string> AskAsync(string message, CancellationToken cancellationToken = default)
	{
		string normalized = Normalize(message);
		string cacheKey = $"apex-chat:{Hash(normalized)}";

		return await _cacheProvider.GetOrCreateAsync(
			cacheKey,
			CacheDuration,
			ct => CallModelAsync(normalized, ct),
			cancellationToken);
	}

	private async Task<string> CallModelAsync(
		string normalizedMessage,
		CancellationToken cancellationToken)
	{

		//TODO, consider moving into a trained model context instead of adding to each prompt
		string systemPrompt =
			"You are an expert coach on the video game Apex Legends. Answer the user's question directly with concise, practical advice. " +
			"Explain tradeoffs and assumptions when they matter, and distinguish stable gameplay principles from " +
			"current patch-specific facts. If the question is unclear, ask one focused clarification question" +
			"Do not deep think or overanalyze the question. If it is unclear or hard information to get to, ask for clarification or express to the user this limitation depending on your confidence level.";

		string reply = await _chatClient.CompleteAsync(
			systemPrompt,
			normalizedMessage,
			_options.MaxOutputTokens,
			cancellationToken);

		if (string.IsNullOrWhiteSpace(reply))
		{
			throw new HttpRequestException(
				"Apex knowledge model returned an empty response.",
				null,
				HttpStatusCode.BadGateway);
		}

		return reply.Trim();
	}

	private static string Normalize(string message)
	{
		string collapsed = string.Join(' ', message.Trim().Split(
			(char[]?)null,
			StringSplitOptions.RemoveEmptyEntries));
		return collapsed.ToLowerInvariant();
	}

	private static string Hash(string value)
	{
		byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
		return Convert.ToHexString(bytes);
	}

}
