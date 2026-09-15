namespace ApexLegendsTracker.AIChat.ChatClients;

public interface IChatClient
{
	Task<string> CompleteAsync(
		string systemPrompt,
		string userPrompt,
		int maxOutputTokens,
		CancellationToken cancellationToken = default);
}