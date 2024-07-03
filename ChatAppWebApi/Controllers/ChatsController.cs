using ChatAppWebApi.Context;
using ChatAppWebApi.Dtos;
using ChatAppWebApi.Hubs;
using ChatAppWebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatAppWebApi.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public sealed class ChatsController(ApplicationDbContext context,IHubContext<ChatHub> hubContext) : ControllerBase
	{
		[HttpGet]
		public async Task<IActionResult> GetChats(Guid userId,Guid toUserId,CancellationToken cancellationToken)
		{
			List<Chat> chats = await context.Chats.Where
				               (p => p.UserId == userId && p.ToUserId == toUserId
							   || p.ToUserId == userId && p.UserId == toUserId)
							   .OrderBy(p => p.Date)
							   .ToListAsync(cancellationToken);
			return Ok(chats);
		}
		[HttpPost]
		public async Task<IActionResult> SendMessage(SendMessageDto request,CancellationToken cancellationToken)
		{
			Chat chat = new Chat()
			{
				UserId = request.UserId,
				ToUserId = request.ToUserId,
				Message = request.Message,
				Date=DateTime.Now
			};

			await context.Chats.AddAsync(chat,cancellationToken);
			await context.SaveChangesAsync(cancellationToken);

			string connectionId = ChatHub.Users.First(p => p.Value == chat.ToUserId).Key;

			await hubContext.Clients.Client(connectionId).SendAsync("Messages", chat);

			return Ok();
		}

	}
}
