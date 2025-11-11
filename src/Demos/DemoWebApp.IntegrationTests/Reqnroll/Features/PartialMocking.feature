Feature: PartialMocking

Demonstrates mocking of injected classes used by a service

Scenario: Get all the items from the real service
	When I call the AllItems endpoint
	Then the items should be
	| Item |
	| Foo1 |
	| Foo2 |
	| Bar1 |
	| Bar2 |
	| Baz1 |
	| Baz2 |

Scenario: Get all the items from a partially mocked service
	Given the Repository is mocked to return the Bar Items
	| Item     |
	| MockBar1 |
	| MockBar2 |
	When I call the AllItems endpoint
	Then the items should be
	| Item     |
	| Foo1     |
	| Foo2     |
	| MockBar1 |
	| MockBar2 |
	| Baz1     |
	| Baz2     |

