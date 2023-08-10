using System.ComponentModel.DataAnnotations;

namespace UserInformation.API.Entities
{
	public class User
	{
		[Key]
		public string UserId { get; set; }
		public string Username { get; set; }
		public int Age { get; set; }
		public string City { get; set; }
		public string PhoneNumber { get; set; }
		public string Email { get; set; }

	}
}
