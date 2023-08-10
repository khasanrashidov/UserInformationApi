using Microsoft.AspNetCore.Mvc;
using UserInformation.API.Context;
using UserInformation.API.Entities;
using UserInformation.API.Models;
using UserInformation.API.Services;

namespace UserInformation.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UserController : ControllerBase
	{
		private readonly UserInfoDbContext _context;
		private readonly IUserService _userService;
		private readonly ILogger<UserController> _logger;

		public UserController(UserInfoDbContext context, IUserService userService, ILogger<UserController> logger)
		{
			_context = context;
			_userService = userService;
			_logger = logger;
		}

		/// <summary>
		/// UploadUserInfoCsv is used to upload a CSV file that contains information about users.
		/// </summary>
		/// <param name="file">
		/// file is the CSV file that contains information about users.
		/// The structure of the file should be as follows: username,useridentifier,age,city,phonenumber,email.
		/// </param>
		/// <returns>
		/// Returns a 400 Bad Request if the file is null or empty, or if the file is not a CSV file.
		/// Returns a 200 OK if the file is successfully uploaded.
		/// </returns>
		[HttpPost]
		[Route("upload-csv")]
		public async Task<IActionResult> UploadUserInfoCsvAsync([FromForm] IFormFile file)
		{
			var resultMessage = await _userService.UploadUserInfoCsvAsync(file);

			if (resultMessage.StartsWith("Invalid") || resultMessage == "No file was uploaded")
			{
				_logger.LogError("Invalid file or no file was uploaded");
				return BadRequest(resultMessage);
			}

			_logger.LogInformation("File was successfully uploaded");
			return Ok(resultMessage);
		}

		/// <summary>
		/// GetUserInfo is used to get a set of user objects.
		/// GetUserInfo endpoint provides sorting and limitation features.
		/// </summary>
		/// <param name="sort">
		/// sort parameter is used to specify the sort direction (ascending or descending).
		/// Default value is ascending.
		/// </param>
		/// <param name="limit">
		/// limit parameter is used to specify the maximum number of users(objects) included in an API response.
		/// Default value is 10.
		/// </param>
		/// <returns>
		/// Returns a 200 OK with a list of user objects if the request is successful.
		/// Returns a 400 Bad Request if the limit is not a positive integer.
		/// </returns>
		[HttpGet]
		public IActionResult GetUserInfo([FromQuery] SortingFormat sort = SortingFormat.Ascending,
										 [FromQuery] int limit = 10)
		{
			if (limit <= 0)
			{
				_logger.LogError("Limit must be a positive integer");
				return BadRequest("Limit must be a positive integer.");
			}

			IQueryable<User> query = _context.Users;

			if (sort == SortingFormat.Ascending)
			{
				_logger.LogInformation("Sorting users in ascending order");
				query = query.OrderBy(u => u.Username);
			}
			else
			{
				_logger.LogInformation("Sorting users in descending order");
				query = query.OrderByDescending(u => u.Username);
			}

			_logger.LogInformation("Getting users");
			var users = query.Take(limit).ToList();

			return Ok(users);
		}
	}
}