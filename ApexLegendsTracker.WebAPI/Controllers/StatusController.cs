using ApexLegendsTracker.Shared;
using ApexLegendsTracker.GameStats.Service;
using Microsoft.AspNetCore.Mvc;

namespace ApexLegendsTracker.WebAPI.Controllers;

[ApiController]
[Route("api/v1")]
public sealed class StatusController : ControllerBase
{
	private readonly IApexGlobalContract _statusContract;

	public StatusController(IApexGlobalContract statusContract)
	{
		_statusContract = statusContract;
	}

	[HttpGet("map-rotation")]
	[ProducesResponseType(typeof(MapRotationResult), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status502BadGateway)]
	public async Task<ActionResult<MapRotationResult>> GetMapRotation(
		[FromQuery] string? version,
		CancellationToken cancellationToken)
	{
		if (!string.IsNullOrWhiteSpace(version) && version is not ("1" or "2"))
		{
			return BadRequest(new
			{
				code = "invalid_map_rotation_version",
				message = "version must be 1 or 2."
			});
		}

		return await ExecuteAsync(
			() => _statusContract.GetMapRotationAsync(version, cancellationToken));
	}

	[HttpGet("predator-thresholds")]
	[ProducesResponseType(typeof(PredatorResult), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status502BadGateway)]
	public Task<ActionResult<PredatorResult>> GetPredatorThresholds(CancellationToken cancellationToken)
	{
		return ExecuteAsync(
			() => _statusContract.GetPredatorThresholdsAsync(cancellationToken));
	}

	private async Task<ActionResult<T>> ExecuteAsync<T>(Func<Task<T>> operation)
	{
		try
		{
			return Ok(await operation());
		}
		catch (HttpRequestException exception) when (exception.StatusCode is not null)
		{
			return StatusCode((int)exception.StatusCode.Value, new
			{
				code = "apex_upstream_error",
				message = exception.Message,
				traceId = HttpContext.TraceIdentifier
			});
		}
	}
}