﻿using ChatAppWebApi.Context;
using ChatAppWebApi.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Client;

namespace ChatAppWebApi.Hubs
{
	public sealed class ChatHub(ApplicationDbContext context):Hub
	{
		public static Dictionary<string, Guid> Users = new();

        public async Task Connect(Guid userId)
		{
			Users.Add(Context.ConnectionId, userId);	
			User? user=await context.Users.FindAsync(userId);
			if(user is not null)
			{
				user.Status = "online";
				await context.SaveChangesAsync();

				await Clients.All.SendAsync("Users", user);
			}
		}
		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			Guid userId;
			Users.TryGetValue(Context.ConnectionId, out userId);

			User? user = await context.Users.FindAsync(userId);
			if(user is not null)
			{
				user.Status = "offline";
				await context.SaveChangesAsync();

				await Clients.All.SendAsync("Users", user);
			}
		}
	}
}
