using ChatAppWebApi.Context;
using ChatAppWebApi.Dtos;
using ChatAppWebApi.Models;
using GenericFileService.Files;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatAppWebApi.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public sealed class AuthController(ApplicationDbContext context) : ControllerBase
	{
		[HttpPost]
		public async Task<IActionResult> Register([FromForm] RegisterDto request,CancellationToken cancellationToken)
		{
			bool isNameExists= await context.Users.AnyAsync(p=>p.Name==request.Name,cancellationToken);
			if(isNameExists)
			{
				return BadRequest(new {Message="Bu kullanıcı adı daha önce kullanılmış."});
			}

			string avatar = FileService.FileSaveToServer(request.File, "wwwroot/avatar/");

			User user = new User()
			{
				Name = request.Name,
				Avatar = avatar
			};
			await context.Users.AddAsync(user,cancellationToken);
			await context.SaveChangesAsync();	

			return Ok(user);	
		}
		[HttpGet]
		public async Task<IActionResult> Login(string name,CancellationToken cancellationToken)
		{
			User? user=await context.Users.FirstOrDefaultAsync(p=>p.Name == name,cancellationToken);	
			if(user is null)
			{
				return BadRequest(new {Message="Kullaıcı bulunamadı."});
			}
			user.Status = "online";
			await context.SaveChangesAsync(cancellationToken);

			return Ok(user);
		}
	}
}
