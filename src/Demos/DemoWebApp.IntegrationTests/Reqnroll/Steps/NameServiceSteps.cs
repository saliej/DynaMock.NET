using System;
using System.Net.Http.Json;
using AwesomeAssertions;
using DemoWebApp.Repositories;
using DemoWebApp.Services;
using DynaMock;
using NSubstitute;
using NSubstitute.ClearExtensions;
using Reqnroll;

namespace DemoWebApp.IntegrationTests.Reqnroll.Steps;

[Binding]
[Scope(Feature = "NameService")]

public class NameServiceSteps
{
	private readonly HttpClient _httpClient;
	private readonly BaseNameGenerator _mockNameGenerator;

	private readonly ScenarioContext _scenarioContext;

	public NameServiceSteps(ScenarioContext scenarioContext)
	{
		_scenarioContext = scenarioContext;
		_mockNameGenerator = Substitute.For<BaseNameGenerator>();
		_httpClient = FixtureProvider.Factory.CreateClient();
		_httpClient.BaseAddress = new Uri("https://localhost:5001");
	}

	[When("I call the Base endpoint")]
	public void WhenICallTheBaseEndpoint()
	{
		var response = _httpClient.GetAsync("/Name/Base").Result;
		_scenarioContext["Name"] = response.Content.ReadAsStringAsync().Result;
	}

	[When("I call the Implemented endpoint")]
	public void WhenICallTheImplementedEndpoint()
	{
		var response = _httpClient.GetAsync("/Name/Implemented").Result;
		_scenarioContext["Name"] = response.Content.ReadAsStringAsync().Result;
	}

	[Then("I should get the name {string}")]
	public void ThenIShouldGetTheName(string expectedName)
	{
		_scenarioContext["Name"].Should().Be(expectedName);
	}

	[Given("I have mocked the NameGenerator to return the base name {string}")]
	public void GivenIHaveMockedTheNameGeneratorToReturnTheBaseName(string mockedBaseName)
	{
		_mockNameGenerator.GetOverridableName()
			.Returns(mockedBaseName);

		DefaultMockProvider<BaseNameGenerator>.SetMock(_mockNameGenerator, config => config
			.MockMethod(x => x.GetOverridableName()));
	}

	[Given("I have mocked the NameGenerator to return the implemeted name {string}")]
	public void GivenIHaveMockedTheNameGeneratorToReturnTheImplemetedName(string mockedImplementedName)
	{
		_mockNameGenerator.GetImplementedName()
			.Returns(mockedImplementedName);

		DefaultMockProvider<BaseNameGenerator>.SetMock(_mockNameGenerator, config => config
			.MockMethod(x => x.GetImplementedName()));
	}

	[BeforeScenario]
	public void BeforeScenario()
	{
		_mockNameGenerator.ClearSubstitute();
		DefaultMockProvider<BaseNameGenerator>.RemoveMock();
	}
}
