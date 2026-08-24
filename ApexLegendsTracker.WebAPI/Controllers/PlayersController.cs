using ApexLegendsTracker.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ApexLegendsTracker.WebAPI.Controllers;

[ApiController]
[Route("api/v1/players")]
public sealed class PlayersController : ControllerBase
{
	private readonly IPlayerLookupContract _apexTrackerService;

	public PlayersController(IPlayerLookupContract apexTrackerService)
	{
		_apexTrackerService = apexTrackerService;
	}

	/// <summary>
	/// Looks up an Apex Legends player's stats by platform and player name.
	/// </summary>
	/// <param name="platform">The player's platform: <c>PC</c>, <c>PS4</c>, or <c>X1</c> (case-insensitive).</param>
	/// <param name="playerName">The exact in-game player name to search for.</param>
	/// <param name="cancellationToken">Cancellation token for the request.</param>
	/// <response code="200">The player was found and their stats are returned.</response>
	/// <response code="400">The player name is missing or the platform is not one of the supported values.</response>
	/// <response code="404">No player with the given name was found on the given platform.</response>
	[HttpGet("{platform}/{playerName}")]
	[ProducesResponseType(typeof(PlayerLookupResult), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status502BadGateway)]
	public async Task<ActionResult<PlayerLookupResult>> GetByName(
		string platform,
		string playerName,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(playerName))
		{
			return BadRequest(new { code = "invalid_player_name", message = "playerName is required." });
		}

		if (!PlatformParser.TryNormalize(platform, out string normalizedPlatform))
		{
			return BadRequest(new
			{
				code = "invalid_platform",
				message = "platform must be one of: PC, PS4, X1."
			});
		}

		try
		{
			PlayerLookupResult result = await _apexTrackerService.QueryByNameAsync(
				playerName.Trim(),
				normalizedPlatform,
				cancellationToken);

			return Ok(result);
		}
		catch (HttpRequestException ex) when (ex.StatusCode is not null)
		{
			int statusCode = (int)ex.StatusCode.Value;
			return StatusCode(statusCode, new
			{
				code = "apex_upstream_error",
				message = ex.Message,
				traceId = HttpContext.TraceIdentifier
			});
		}
	}
}