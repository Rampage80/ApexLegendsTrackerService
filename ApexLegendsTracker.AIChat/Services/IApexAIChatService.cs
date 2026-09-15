namespace ApexLegendsTracker.AIChat.Service;

public interface IApexAIChatService
{
	Task<string> AskAsync(string message, CancellationToken cancellationToken = default);
}
