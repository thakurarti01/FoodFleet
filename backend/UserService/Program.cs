using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserService.Data;
using UserService.Repositories;
using UserService.Services;

var builder = WebApplication.CreateBuilder(args);

var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "THIS_IS_A_TEMPORARY_SECRET_KEY_FOR_EXAM_1234567890");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "FoodFleet",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "FoodFleet",
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.FromMinutes(5),
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
        NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
    };
});

string ConvertPostgres(string conn) {
    try {
        if (!conn.Contains("://")) return conn;
        var uri = new Uri(conn);
        var userInfo = uri.UserInfo.Split(':');
        var user = userInfo[0];
        var pass = userInfo.Length > 1 ? userInfo[1] : "";
        var host = uri.Host;
        var db = uri.PathAndQuery.TrimStart('/');
        var port = uri.Port > 0 ? uri.Port : 5432;
        return $"Host={host};Port={port};Database={db};Username={user};Password={pass};SSL Mode=Require;Trust Server Certificate=true";
    } catch { return conn; }
}

builder.Services.AddDbContext<AppDbContext>(options => {
    var conn = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(conn)) return;
    if (conn.Contains("postgres"))
        options.UseNpgsql(ConvertPostgres(conn));
    else
        options.UseSqlServer(conn);
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<UserService.Services.RabbitMQPublisher>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o => o.AddPolicy("AllowAll",
    p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

try {
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Console.WriteLine("Initializing Database...");
        db.Database.EnsureCreated();
        UserService.Data.SeedData.Initialize(db);
        Console.WriteLine("Database Initialized Successfully.");
    }
} catch (Exception ex) {
    Console.WriteLine($"Database Initialization Failed: {ex.Message}");
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
