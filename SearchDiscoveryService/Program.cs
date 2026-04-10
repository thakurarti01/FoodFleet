using Microsoft.EntityFrameworkCore;
using SearchDiscoveryService.Data;
using SearchDiscoveryService.Interfaces;
using SearchDiscoveryService.Services;
using SearchDiscoveryService.Mappings;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);

//  Controllers
builder.Services.AddControllers();

//  Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//  DbContext
builder.Services.AddDbContext<SearchDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//  AutoMapper (v16 manual configuration)
builder.Services.AddSingleton<IMapper>(sp =>
{
	var config = new MapperConfiguration(cfg =>
	{
		cfg.AddProfile<AutoMapperProfiles>();
	});

	return config.CreateMapper();
});

//  Dependency Injection
builder.Services.AddScoped<ISearchService, SearchService>();

var app = builder.Build();

//  Seed Data
using (var scope = app.Services.CreateScope())
{
	var context = scope.ServiceProvider.GetRequiredService<SearchDbContext>();
	SeedData.Initialize(context);
}

//  Middleware
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();