Feature: AbstractClass

Demonstrates mocking of abstract classes used by a service

Scenario: Get the base name from the name service
	When I call the Base endpoint
	Then I should get the name "OverridedName"

Scenario: Get the implemented name from the name service
	When I call the Implemented endpoint
	Then I should get the name "ImplementedName"

Scenario: Get the mocked base name from the name service
	Given I have mocked the NameGenerator to return the base name "MockedBaseName"
	When I call the Base endpoint
	Then I should get the name "MockedBaseName"
	When I call the Implemented endpoint
	Then I should get the name "ImplementedName"

Scenario: Get the mocked implemented name from the name service
	Given I have mocked the NameGenerator to return the implemeted name "MockedImplementedName"
	When I call the Implemented endpoint
	Then I should get the name "MockedImplementedName"
	When I call the Base endpoint
	Then I should get the name "OverridedName"