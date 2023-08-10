using Microsoft.AspNetCore.Mvc;
using UserInformation.API.Context;
using UserInformation.API.Entities;
using UserInformation.API.Models;

namespace UserInformation.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UserController : ControllerBase
	{
		private readonly UserInfoDbContext _context;

		public UserController(UserInfoDbContext context)
		{
			_context = context;
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
		public async Task<IActionResult> UploadUserInfoCsvAsync([FromForm] IFormFile file)
		{
			if (file == null || file.Length == 0)
			{
				return BadRequest("No file was uploaded");
			}

			if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
			{
				return BadRequest("Invalid file type");
			}

			var users = new List<UserDto>();

			using (var reader = new StreamReader(file.OpenReadStream()))
			{
				while (!reader.EndOfStream)
				{
					var line = await reader.ReadLineAsync();
					var values = line!.Split(',');

					if (values.Length != 6)
					{
						return BadRequest("Invalid CSV format");
					}

					var user = new UserDto
					{
						Username = values[0],
						UserId = values[1],
						Age = int.Parse(values[2]),
						City = values[3],
						PhoneNumber = values[4],
						Email = values[5]
					};

					var existingUser = _context.Users.FirstOrDefault(u => u.UserId == user.UserId);

					if (existingUser == null)
					{
						users.Add(user);
					}
					else
					{
						// Update the existing user without adding to users list
						existingUser.Username = user.Username;
						existingUser.Age = user.Age;
						existingUser.City = user.City;
						existingUser.PhoneNumber = user.PhoneNumber;
						existingUser.Email = user.Email;
					}
				}

				foreach (var userDto in users)
				{
					var userEntity = new User
					{
						Username = userDto.Username,
						UserId = userDto.UserId,
						Age = userDto.Age,
						City = userDto.City,
						PhoneNumber = userDto.PhoneNumber,
						Email = userDto.Email
					};

					_context.Users.Add(userEntity);
				}

				await _context.SaveChangesAsync();
			}

			return Ok("CSV file uploaded and processed successfully");
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
				return BadRequest("Limit must be a positive integer.");
			}

			IQueryable<User> query = _context.Users;

			if (sort == SortingFormat.Ascending)
			{
				query = query.OrderBy(u => u.Username);
			}
			else
			{
				query = query.OrderByDescending(u => u.Username);
			}

			var users = query.Take(limit).ToList();

			return Ok(users);
		}
	}
}