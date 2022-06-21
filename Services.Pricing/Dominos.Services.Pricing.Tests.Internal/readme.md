Test Time Fun: Perpetrators caught violating these commandments your PR will be destroyed shortly.

Internal Test:

Tests within this project should not make external calls. Specifically we have divided this project into
two sections:

	- Unit: testing of singular units.
	- Component: testing of interactions between internal system components via mocks and stubs. 

Naming Conventions:

	- Should<Expression> (e.g. ShouldReturnOutOfStockProductIfSizeIsOutOfStock)
	- Classes with multiple methods, <Method Name>_Should<Expression> (e.g. GetLanguage_ShouldBeLowerCaseForAllCountries)

Test Conventions:

	- Every public method should have tests.
	- Every function should have dedicated tests.
	- However if a function's only purpose is to call a downstream method without holding any form of business logic, then it should be tested as part of an end-to-end test

Testing Resources:
https://github.com/xunit/samples.xunit

Trait Tests:
http://www.brendanconnolly.net/organizing-tests-with-xunit-traits/
https://github.com/brendanconnolly/Xunit.Categories - Might be worth including or writing our own

Theory Tests:
https://andrewlock.net/creating-parameterised-tests-in-xunit-with-inlinedata-classdata-and-memberdata/
http://hamidmosalla.com/2017/02/25/xunit-theory-working-with-inlinedata-memberdata-classdata/
https://andrewlock.net/creating-strongly-typed-xunit-theory-test-data-with-theorydata/

Shared Data and Context:
https://xunit.github.io/docs/shared-context