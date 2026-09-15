namespace ApexLegendsTracker.AIChat.Options;

//TODO: Move BaseURL to env variable
public sealed class ApexAIChatOptions
{
	public const string SectionName = "ApexAIChatService";

	public string? ApiKey { get; set; }

	// Use the OpenAI-compatible endpoint, including /openai/v1/ for Azure OpenAI.
	public string BaseUrl { get; init; } = "https://bceag-mu1qsf53-southcentralus.cognitiveservices.azure.com/openai/v1/";

	// For Azure AI Foundry/Azure OpenAI deployments, set this to the deployment name (not the underlying model family).
	public string Model { get; init; } = "gpt-4.1-mini";

	public int MaxOutputTokens { get; init; } = 250;

	// Retained for configuration compatibility; the generic OpenAI SDK uses the v1 endpoint instead.
	public string? ApiVersion { get; init; }
}
