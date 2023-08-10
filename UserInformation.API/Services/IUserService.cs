namespace UserInformation.API.Services
{
	public interface IUserService
	{
		Task<string> UploadUserInfoCsvAsync(IFormFile file);
	}
}