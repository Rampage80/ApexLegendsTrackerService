namespace ApexLegendsTracker.WebAPI;

public static class PlatformParser
{
	public static bool TryNormalize(string? platform, out string normalizedPlatform)
	{
		normalizedPlatform = (platform ?? string.Empty).Trim().ToUpperInvariant();

		return normalizedPlatform is "PC" or "PS4" or "X1";
	}
}