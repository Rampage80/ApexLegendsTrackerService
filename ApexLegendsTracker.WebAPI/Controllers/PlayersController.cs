using ApexLegendsTracker.Application.Players;
using Microsoft.AspNetCore.Mvc;

namespace ApexLegendsTracker.WebAPI.Controllers;

[ApiController]
[Route("api/v1/players")]
public sealed class PlayersController : ControllerBase
{
	private readonly IApexTrackerService _apexTrackerService;

	public PlayersController(IApexTrackerService apexTrackerService)
	{
		_apexTrackerService = apexTrackerService;
	}

	[HttpGet("{platform}/{playerName}")]
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