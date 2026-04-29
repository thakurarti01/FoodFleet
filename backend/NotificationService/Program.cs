using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NotificationService.Data;
using NotificationService.Interfaces;
using NotificationService.Services;

/// <summary>
/// NotificationService startup.
/// Responsibilities:
///   - Persists in-app notifications to SQL Server via NotificationDbContext
///   - Sends transactional emails via EmailService (MailKit/Gmail SMTP)
///   - Consumes RabbitMQ events from UserService and OrderService
///     to trigger notifications on: new registration, order placed, order delivered
///   - Exposes REST endpoints for fetching and marking notifications as read
/// </summary>
var builder = WebApplication.CreateBuilder(args);

// JWT — same key/issuer as all other services
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = builder.Configuration["Jwt:Issuer"],
        ValidAudience            = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(key),
        ClockSkew                = TimeSpan.FromMinutes(5),
        RoleClaimType            = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
        NameClaimType            = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
    };
});

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<INotificationService, NotificationServiceImp>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<RabbitMQConsumer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization", Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer", BearerFormat = "JWT", In = Microsoft.OpenApi.ParameterLocation.Header
    });
    options.AddSecurityRequirement(_ =>
    {
        var req = new Microsoft.OpenApi.OpenApiSecurityRequirement();
        req.Add(new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer"), new List<string>());
        return req;
    });
});

builder.Services.AddCors(o => o.AddPolicy("AllowAll",
    p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Start consuming RabbitMQ queues at startup
var consumer = app.Services.GetRequiredService<RabbitMQConsumer>();
consumer.StartListening();

app.Run();
