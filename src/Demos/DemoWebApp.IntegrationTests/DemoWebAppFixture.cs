using DemoWebApp.Repositories;
using DemoWebApp.Services;
using DynaMock;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace DemoWebApp.IntegrationTests;

// Specify types to generate mockable wrappers for
[Mockable(
	typeof(IDemoService), 
	typeof(IRepository),
	typeof(BaseNameGenerator)
)]
public class MockableTypes { }

public class DemoWebAppFixture : WebApplicationFactory<DemoWebApp.Program>
{
	public DemoWebAppFixture()
	{
		
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		var configuration = new ConfigurationBuilder()
				.Build();

		builder.ConfigureTestServices(services =>
		{
			// Inject Mockable wrappers for services to be mocked in tests
			services.AddMockable<IDemoService>();
			services.AddMockable<IRepository>();
			services.AddMockable<BaseNameGenerator>();
		});
	}
}