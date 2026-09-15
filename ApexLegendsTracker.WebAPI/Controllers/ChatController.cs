using ApexLegendsTracker.Shared;
using ApexLegendsTracker.AIChat.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ApexLegendsTracker.Common.Constants;

namespace ApexLegendsTracker.WebAPI.Controllers;

[ApiController]
[Route("api/v1/chat")]
[EnableRateLimiting(RateLimiterPolicies.Chat)]
public sealed class ChatController : ControllerBase
{
	private const int MaxMessageLength = 500;

	private readonly IApexAIChatService _AIChatService;

	public ChatController(IApexAIChatService AIChatService)
	{
		_AIChatService = AIChatService;
	}

	/// <summary>
	/// Answers a chat question using the configured language model.
	/// </summary>
	/// <response code="200">The chat reply was generated.</response>
	/// <response code="400">The message is missing or too long.</response>
	/// <response code="429">Too many requests; try again later.</response>
	/// <response code="502">The upstream model request failed.</response>
	[HttpPost]
	[ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status429TooManyRequests)]
	[ProducesResponseType(StatusCodes.Status502BadGateway)]
	public async Task<ActionResult<ChatResponse>> PostAsync(
		[FromBody] ChatRequest request,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(request.Message))
		{
			return BadRequest(new { code = "invalid_message", message = "message is required." });
		}

		if (request.Message.Length > MaxMessageLength)
		{
			return BadRequest(new
			{
				code = "message_too_long",
				message = $"message must be {MaxMessageLength} characters or fewer."
			});
		}

		try
		{
			string reply = await _AIChatService.AskAsync(request.Message, cancellationToken);
			return Ok(new ChatResponse(reply, ChatSource.Knowledge));
		}
		catch (HttpRequestException exception) when (exception.StatusCode is not null)
		{
			return StatusCode((int)exception.StatusCode.Value, new
			{
				code = "chat_upstream_error",
				message = exception.Message,
				traceId = HttpContext.TraceIdentifier
			});
		}
	}
}
