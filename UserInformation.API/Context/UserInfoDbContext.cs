using Microsoft.EntityFrameworkCore;
using UserInformation.API.Entities;

namespace UserInformation.API.Context
{
	public class UserInfoDbContext: DbContext
	{
		public UserInfoDbContext()
		{
		}

		public UserInfoDbContext(DbContextOptions<UserInfoDbContext> options): base(options)
		{
		}

		public DbSet<User> Users { get; set; }
	}
}
