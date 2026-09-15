using System.Text.Json;
using ApexLegendsTracker.Shared;

namespace ApexLegendsTracker.GameStats.Tests;

public sealed class PredatorResponseTests
{
	[Fact]
	public void CapturedResponse_MapsSupportedPlatformsAndExcludesSwitch()
	{
		PredatorResponse response = Deserialize<PredatorResponse>("Predator_APIReturns.json");

		Assert.Equal(31539, response.RP?.PC?.Val);
		Assert.Equal(24887, response.RP?.PS4?.Val);
		Assert.Equal(19343, response.RP?.X1?.Val);
		Assert.DoesNotContain("SWITCH", typeof(PredatorPlatformThresholds).GetProperties().Select(property => property.Name));
	}

	private static T Deserialize<T>(string fileName)
	{
		string path = Path.Combine(AppContext.BaseDirectory, "APIResponseData", fileName);
		return JsonSerializer.Deserialize<T>(
			File.ReadAllText(path),
			new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
			?? throw new InvalidOperationException($"Fixture {fileName} was empty.");
	}
}