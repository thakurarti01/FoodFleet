using Microsoft.EntityFrameworkCore;
using ReviewService.Data;
using ReviewService.Interfaces;
using ReviewService.Services;

var builder = WebApplication.CreateBuilder(args);

// ------------------- Add Services -------------------

// Add DbContext with SQL Server (update connection string in appsettings.json)
builder.Services.AddDbContext<ReviewDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Review Service
builder.Services.AddScoped<IReviewService, ReviewServiceImp>();

// Add Controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ------------------- Middleware -------------------

// Global exception handling (optional, better than try/catch everywhere)
app.UseExceptionHandler(errorApp =>
{
	errorApp.Run(async context =>
	{
		context.Response.ContentType = "application/json";
		var contextFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
		if (contextFeature != null)
		{
			var statusCode = contextFeature.Error switch
			{
				ReviewService.Exceptions.ReviewAlreadyExistsException => 400,
				ReviewService.Exceptions.ReviewNotFoundException => 404,
				ReviewService.Exceptions.InvalidOwnerResponseException => 400,
				ReviewService.Exceptions.ReviewDeletionException => 400,
				_ => 500
			};

			context.Response.StatusCode = statusCode;
			await context.Response.WriteAsJsonAsync(new
			{
				StatusCode = statusCode,
				Message = contextFeature.Error.Message
			});
		}
	});
});

// Enable Swagger
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// Routing & Controllers
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();