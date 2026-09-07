using System.Text.Json;
using ApexLegendsTracker.Shared;

namespace ApexLegendsTracker.Service.Tests;

public sealed class MapRotationResponseTests
{
	[Fact]
	public void CapturedResponse_MapsModesAndCurrentNextEntries()
	{
		MapRotationResponse response = Deserialize<MapRotationResponse>("MapRotation_APIReturns.json");

		Assert.Equal("Storm Point", response.BattleRoyale?.Current?.Map);
		Assert.Equal("E-District", response.BattleRoyale?.Next?.Map);
		Assert.Equal("TDM", response.Ltm?.Current?.EventName);
		Assert.Equal("Control", response.Ltm?.Next?.EventName);
		Assert.Equal(78, response.Ranked?.Current?.RemainingMins);
		Assert.Equal("Kings Canyon", response.Wildcard?.Current?.Map);
		Assert.Equal(5400, response.BattleRoyale?.Current?.DurationInSeconds);
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