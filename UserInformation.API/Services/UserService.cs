using UserInformation.API.Context;
using UserInformation.API.Entities;
using UserInformation.API.Models;

namespace UserInformation.API.Services
{
	public class UserService : IUserService
	{
		private readonly UserInfoDbContext _context;
		private readonly ILogger<UserService> _logger;

		public UserService(UserInfoDbContext context, ILogger<UserService> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task<string> UploadUserInfoCsvAsync(IFormFile file)
		{
			if (file == null || file.Length == 0)
			{
				_logger.LogError("No file was uploaded");
				return "No file was uploaded";
			}

			if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
			{
				_logger.LogError("Invalid file type");
				return "Invalid file type";
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
						_logger.LogError("Invalid CSV format");
						return "Invalid CSV format";
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
						_logger.LogInformation("Adding new user with user identifier {UserId}", user.UserId);
						users.Add(user);
					}
					else
					{
						_logger.LogInformation("Updating existing user with user identifier {UserId}", user.UserId);
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

				_logger.LogInformation("Saving changes to database");
				await _context.SaveChangesAsync();
			}

			_logger.LogInformation("CSV file uploaded and processed successfully");

			return "CSV file uploaded and processed successfully";
		}
	}
}
