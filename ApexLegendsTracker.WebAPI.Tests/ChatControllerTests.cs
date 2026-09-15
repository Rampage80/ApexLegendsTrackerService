using ApexLegendsTracker.AIChat.Service;
using ApexLegendsTracker.Shared;
using ApexLegendsTracker.WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApexLegendsTracker.WebAPI.Tests;

public sealed class ChatControllerTests
{
	[Fact]
	public async Task PostAsync_RejectsEmptyMessage()
	{
		ChatController controller = CreateController(new StubAIChatService("unused"));

		ActionResult<ChatResponse> result = await controller.PostAsync(new ChatRequest(" "), CancellationToken.None);

		Assert.IsType<BadRequestObjectResult>(result.Result);
	}

	[Fact]
	public async Task PostAsync_RejectsMessageOverMaxLength()
	{
		ChatController controller = CreateController(new StubAIChatService("unused"));
		string longMessage = new('a', 501);

		ActionResult<ChatResponse> result = await controller.PostAsync(new ChatRequest(longMessage), CancellationToken.None);

		Assert.IsType<BadRequestObjectResult>(result.Result);
	}

	[Fact]
	public async Task PostAsync_ReturnsKnowledgeReplyOnSuccess()
	{
		ChatController controller = CreateController(new StubAIChatService("Rotate early and hold high ground."));

		ActionResult<ChatResponse> result = await controller.PostAsync(
			new ChatRequest("how should I rotate in ranked?"),
			CancellationToken.None);

		OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
		ChatResponse response = Assert.IsType<ChatResponse>(ok.Value);
		Assert.Equal("Rotate early and hold high ground.", response.Reply);
		Assert.Equal(ChatSource.AIChat, response.Source);
	}

	[Fact]
	public async Task PostAsync_MapsUpstreamFailureToStatusCode()
	{
		ChatController controller = CreateController(new StubAIChatService(
			exception: new HttpRequestException("boom", null, System.Net.HttpStatusCode.BadGateway)));

		ActionResult<ChatResponse> result = await controller.PostAsync(
			new ChatRequest("what's the best loadout for close range?"),
			CancellationToken.None);

		ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
		Assert.Equal(StatusCodes.Status502BadGateway, objectResult.StatusCode);
	}

	private static ChatController CreateController(StubAIChatService aIChatService)
	{
		return new ChatController(aIChatService)
		{
			ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext()
			}
		};
	}

	private sealed class StubAIChatService : IApexAIChatService
	{
		private readonly string? _reply;
		private readonly Exception? _exception;

		public StubAIChatService(string? reply = null, Exception? exception = null)
		{
			_reply = reply;
			_exception = exception;
		}

		public Task<string> AskAsync(string message, CancellationToken cancellationToken = default)
		{
			return _exception is not null
				? Task.FromException<string>(_exception)
				: Task.FromResult(_reply ?? string.Empty);
		}
	}
}
