using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using System.Text.Json.Serialization;
using UserInformation.API.Context;
using UserInformation.API.Middlewares;
using UserInformation.API.Services;

var logger = NLog.LogManager.Setup()
	.LoadConfigurationFromAppSettings()
	.GetCurrentClassLogger();

try
{
	logger.Debug("init main");
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

	// NLog: Setup NLog for Dependency injection
	builder.Logging.ClearProviders();
	builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
	builder.Host.UseNLog();

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
	// Adding middleware to the pipeline to handle exceptions
	app.UseCustomExceptionHandlerMiddleware();

	app.MapControllers();

	app.Run();
}
catch (Exception ex)
{
	logger.Error(ex, "Stopped program because of exception");
	throw;
}
finally
{
	NLog.LogManager.Shutdown();
}
