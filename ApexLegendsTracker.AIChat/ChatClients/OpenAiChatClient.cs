using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using ApexLegendsTracker.AIChat.Options;
using Microsoft.Extensions.Options;

namespace ApexLegendsTracker.AIChat.ChatClients;

public sealed class OpenAiChatClient : IChatClient
{
	private readonly ChatClient _client;

	public OpenAiChatClient(IOptions<ApexAIChatOptions> options)
	{
		ApexAIChatOptions settings = options.Value;
		if (string.IsNullOrWhiteSpace(settings.ApiKey))
		{
			throw new InvalidOperationException("Apex knowledge API key is not configured.");
		}

		if (!Uri.TryCreate(settings.BaseUrl, UriKind.Absolute, out Uri? endpoint))
		{
			throw new InvalidOperationException("Apex knowledge endpoint is not a valid absolute URI.");
		}

		_client = new ChatClient(
			settings.Model,
			new ApiKeyCredential(settings.ApiKey),
			new OpenAIClientOptions { Endpoint = endpoint });
	}

	public async Task<string> CompleteAsync(
		string systemPrompt,
		string userPrompt,
		int maxOutputTokens,
		CancellationToken cancellationToken = default)
	{
		ChatCompletionOptions completionOptions = new()
		{
			MaxOutputTokenCount = maxOutputTokens,
			Temperature = 0.2f
		};

		ClientResult<ChatCompletion> result = await _client.CompleteChatAsync(
			[
				new SystemChatMessage(systemPrompt),
				new UserChatMessage(userPrompt)
			],
			completionOptions,
			cancellationToken);

		return string.Concat(result.Value.Content.Select(contentPart => contentPart.Text)).Trim();
	}
}