using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using UserInformation.API.Context;
using UserInformation.API.Services;

namespace UserInformation.API
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddControllers();
			// To display enum values as strings in the API
			builder.Services
				.AddControllers()
				.AddJsonOptions(options =>
					options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(c =>
				c.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = "UserInformation.API",
					Version = "v1",
					Description = "API for uploading and retrieving user information",
				})
				);

			builder.Services.AddDbContext<UserInfoDbContext>(options =>
			{
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
			});

			builder.Services.AddScoped<IUserService, UserService>();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			// So that the Swagger UI is available in production
			// To invoke the Swagger UI, go to https://<host>/swagger or https://<host>/swagger/index.html
			if (app.Environment.IsProduction())
			{
				app.UseSwagger();
				app.UseSwaggerUI(c =>
				{
					c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserInformation.API v1");
					c.RoutePrefix = "swagger";
				});
			}
			app.UseRouting();

			app.UseHttpsRedirection();

			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}