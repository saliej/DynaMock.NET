Feature: InterfaceInjection

Demonstrates mocking of injected services

Scenario: Get real service name
	When I call the ServiceName endpoint
	Then the service name should be "DemoService"

Scenario: Get mocked service name
	Given the DemoService is mocked to return the service name "MockedService"
	When I call the ServiceName endpoint
	Then the service name should be "MockedService"

Scenario: Get real current date
	When I call the CurrentDate endpoint
	Then the current date should be today's date

Scenario: Get mocked current date
	Given the DemoService is mocked to return the current date "2023-01-01"
	When I call the CurrentDate endpoint
	Then the current date should be "2023-01-01"

Scenario: Get real random integer between 1 and 100
	When I call the RandomInt endpoint
	Then the random integer should be between 1 and 100 inclusive

Scenario: Get mocked random integer between 1 and 100
	Given the DemoService is mocked to return the random integer 42
	When I call the RandomInt endpoint
	Then the random integer should be 42