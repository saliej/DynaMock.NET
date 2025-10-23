using DemoWebApp.Repositories;
using DemoWebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSingleton<IDemoService, DemoService>();
builder.Services.AddTransient<IRepository, DemoRepository>();
builder.Services.AddTransient<IStorageService, StorageService>();
builder.Services.AddTransient<INameGenerationService, NameGenerationService>();
builder.Services.AddTransient<BaseNameGenerator, ConcreteNameGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Configure URLs to use port 5000
app.Urls.Add("http://localhost:5000");
app.Urls.Add("https://localhost:5001");

app.Run();


namespace DemoWebApp
{
	public partial class Program
	{
		// Expose the Program class for IntegrationTesting
	}
}