using UserInformation.API.Context;
using UserInformation.API.Entities;
using UserInformation.API.Models;

namespace UserInformation.API.Services
{
	public class UserService : IUserService
	{
		private readonly UserInfoDbContext _context;

		public UserService(UserInfoDbContext context)
		{
			_context = context;
		}

		public async Task<string> UploadUserInfoCsvAsync(IFormFile file)
		{
			if (file == null || file.Length == 0)
			{
				return "No file was uploaded";
			}

			if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
			{
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

			return "CSV file uploaded and processed successfully";
		}
	}
}
