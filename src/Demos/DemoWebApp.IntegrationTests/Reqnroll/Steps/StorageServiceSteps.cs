using System;
using System.Globalization;
using System.Net.Http.Json;
using AwesomeAssertions;
using DemoWebApp.Repositories;
using DemoWebApp.Services;
using DynaMock;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ClearExtensions;
using Reqnroll;

namespace DemoWebApp.IntegrationTests.Reqnroll.Steps;

[Binding]
[Scope(Feature = "StorageService")]

public class StorageServiceSteps
{
	private readonly HttpClient _httpClient;
	private readonly IRepository _mockRepository;

	private ScenarioContext _scenarioContext;

	public StorageServiceSteps(ScenarioContext scenarioContext)
	{
		_scenarioContext = scenarioContext;
		_mockRepository = Substitute.For<IRepository>();
		_httpClient = FixtureProvider.Factory.CreateClient();
		_httpClient.BaseAddress = new Uri("https://localhost:5001");
	}

	[When("I call the AllItems endpoint")]
	public void WhenICallTheAllItemsEndpoint()
	{
		var response = _httpClient.GetAsync("/Storage/AllItems").Result;
		_scenarioContext["AllItems"] = response.Content.ReadFromJsonAsync<List<string>>().Result;
	}

	[Then("the items should be")]
	public void ThenTheItemsShouldBe(DataTable dataTable)
	{
		var result = (List<string>)_scenarioContext["AllItems"];
		dataTable.Rows.Select(r => r[0]).ToList()
			.Should().BeEquivalentTo(result);
	}

	[Given("the Repository is mocked to return the Bar Items")]
	public void GivenTheRepositoryIsMockedToReturnTheBarItems(DataTable dataTable)
	{
		_mockRepository.GetBarItems()
			.Returns(dataTable.Rows.Select(r => r[0]).ToList());

		DefaultMockProvider<IRepository>.SetMock(_mockRepository, config => config
			.MockMethod(x => x.GetBarItems()));
	}

	[BeforeScenario]
	public void BeforeScenario()
	{
		_mockRepository.ClearSubstitute();
		DefaultMockProvider<IRepository>.RemoveMock();
	}
}
