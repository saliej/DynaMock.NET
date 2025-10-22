using System;
using System.Globalization;
using System.Net.Http.Json;
using AwesomeAssertions;
using DemoWebApp.Services;
using DynaMock;
using NSubstitute;
using NSubstitute.ClearExtensions;
using Reqnroll;

namespace DemoWebApp.IntegrationTests.Reqnroll.Steps;

[Binding]
[Scope(Feature = "DemoService")]

public class DemoServiceSteps
{
	private readonly HttpClient _httpClient;
	private readonly IDemoService _mockDemoService;

	private ScenarioContext _scenarioContext;

	public DemoServiceSteps(ScenarioContext scenarioContext)
	{
		_scenarioContext = scenarioContext;
		_mockDemoService = Substitute.For<IDemoService>();
		_httpClient = FixtureProvider.Factory.CreateClient();
		_httpClient.BaseAddress = new Uri("https://localhost:5001");
	}

	[When("I call the ServiceName endpoint")]
	public void WhenICallTheServiceNameEndpoint()
	{
		var response = _httpClient.GetAsync("/Demo/ServiceName").Result;
		_scenarioContext["ServiceName"] = response.Content.ReadAsStringAsync().Result;
	}

	[Then("the service name should be {string}")]
	public void ThenTheServiceNameShouldBe(string expectedResponse)
	{
		_scenarioContext["ServiceName"].ToString().Should().Be(expectedResponse);
	}


	[When("I call the CurrentDate endpoint")]
	public void WhenICallTheCurrentDateEndpoint()
	{
		var response = _httpClient.GetAsync("/Demo/CurrentDate").Result;
		_scenarioContext["CurrentDate"] = response.Content.ReadFromJsonAsync<DateTime>().Result;
	}

	[Then("the response should be today's date")]
	public void ThenTheResponseShouldBeTodaysDate()
	{
		var actualDate = (DateTime)(_scenarioContext["CurrentDate"]);
		
		actualDate.Date.Should().Be(DateTime.Now.Date);
	}

	[Then("the current date should be today's date")]
	public void ThenTheCurrentDateShouldBeTodaysDate()
	{
		var actualDate = (DateTime)(_scenarioContext["CurrentDate"]);

		actualDate.Date.Should().Be(DateTime.Now.Date);
	}

	[Then("the current date should be {string}")]
	public void ThenTheCurrentDateShouldBe(string expectedDate)
	{
		var actualDate = (DateTime)(_scenarioContext["CurrentDate"]);

		if (DateTime.TryParseExact(expectedDate, "yyyy-MM-dd", CultureInfo.InvariantCulture,
			DateTimeStyles.None, out var result))
		{
			actualDate.Date.Should().Be(result.Date);
		}
		else
		{
			throw new FormatException($"Expected date in format 'yyyy-MM-dd', but got '{expectedDate}'");
		}
	}

	[When("I call the RandomInt endpoint")]
	public void WhenICallTheRandomIntEndpoint()
	{
		var response = _httpClient.GetAsync("/Demo/RandomInt").Result;
		_scenarioContext["RandomInt"] = response.Content.ReadFromJsonAsync<int>().Result;
	}

	[Then("the random integer should be between {int} and {int} inclusive")]
	public void ThenTheRandomIntegerShouldBeBetweenAndInclusive(int low, int high)
	{
		var actualInt = (int)(_scenarioContext["RandomInt"]);
		actualInt.Should().BeInRange(low, high);
	}

	[Then("the random integer should be {int}")]
	public void ThenTheRandomIntegerShouldBe(int expectedInt)
	{
		var actualInt = (int)(_scenarioContext["RandomInt"]);
		actualInt.Should().Be(expectedInt);
	}

	[Given("the DemoService is mocked to return the service name {string}")]
	public void GivenTheDemoServiceIsMockedToReturnTheServiceName(string expectedResponse)
	{
		_mockDemoService.GetName().Returns(expectedResponse);

		DefaultMockProvider<IDemoService>.SetMock(_mockDemoService, config =>
		{
			config.MockMethod(m => m.GetName());
		});
	}

	[Given("the DemoService is mocked to return the current date {string}")]
	public void GivenTheDemoServiceIsMockedToReturnTheCurrentDate(string expectedResponse)
	{
		if (DateTime.TryParseExact(expectedResponse, "yyyy-MM-dd", CultureInfo.InvariantCulture, 
			DateTimeStyles.None, out var result))
		{
			_mockDemoService.GetCurrentDate().Returns(result);

			DefaultMockProvider<IDemoService>.SetMock(_mockDemoService, config =>
			{
				config.MockMethod(m => m.GetCurrentDate());
			});
		}
		else
			throw new FormatException($"Expected date in format 'yyyy-MM-dd', but got '{expectedResponse}'");
	}

	[Given("the DemoService is mocked to return the random integer {int}")]
	public void GivenTheDemoServiceIsMockedToReturnTheRandomInteger(int expectedInt)
	{
		_mockDemoService.GetRandomNumberAsync().Returns(Task.FromResult(expectedInt));

		DefaultMockProvider<IDemoService>.SetMock(_mockDemoService, config =>
		{
			config.MockMethod(m => m.GetRandomNumberAsync());
		});
	}

	[BeforeScenario]
	public void BeforeScenario()
	{
		_mockDemoService.ClearSubstitute();
		DefaultMockProvider<IDemoService>.RemoveMock();
	}
}
