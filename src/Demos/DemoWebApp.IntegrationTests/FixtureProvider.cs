using System;

namespace DemoWebApp.IntegrationTests;

// Note that this isn't typically how you'd structure fixtures or DI in real projects
// but it's like this here to demonstrate how to certain features.
public static class FixtureProvider
{
	public static DemoWebAppFixture Factory { get; }

	static FixtureProvider()
	{
		Factory = new DemoWebAppFixture();
	}
}