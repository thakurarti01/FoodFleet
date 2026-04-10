using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Hubs;
using NotificationService.Interfaces;
using NotificationService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// 1. DbContext
builder.Services.AddDbContext<NotificationDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. SignalR
builder.Services.AddSignalR();

// Email service
builder.Services.AddScoped<IEmailService, EmailService>();

// 3. Dependency Injection for NotificationService
builder.Services.AddScoped<INotificationService, NotificationService.Services.NotificationService>();

// 4. Controllers
builder.Services.AddControllers();

// 5. Swagger (optional, recommended for testing APIs)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map SignalR hub
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();