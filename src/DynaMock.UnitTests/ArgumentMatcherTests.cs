using System;
using System.Linq.Expressions;
using AwesomeAssertions;

namespace DynaMock.UnitTests;

public class ArgumentMatcherTests
{
	private readonly ArgumentMatcher _matcher;

	public ArgumentMatcherTests()
	{
		_matcher = new ArgumentMatcher();
	}

	#region Literal Value Tests

	[Fact]
	public void MatchesCall_WithStringLiteral_ShouldMatch()
	{
		Expression expression = () => TestMethod("test");

		var matches = _matcher.MatchesCall(expression, new object[] { "test" });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithStringLiteral_ShouldNotMatch()
	{
		Expression expression = () => TestMethod("test");

		var matches = _matcher.MatchesCall(expression, new object[] { "different" });

		matches.Should().BeFalse();
	}

	[Fact]
	public void MatchesCall_WithIntLiteral_ShouldMatch()
	{
		Expression expression = () => TestMethodInt(42);

		var matches = _matcher.MatchesCall(expression, new object[] { 42 });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithIntLiteral_ShouldNotMatch()
	{
		Expression expression = () => TestMethodInt(42);

		var matches = _matcher.MatchesCall(expression, new object[] { 99 });

		matches.Should().BeFalse();
	}

	[Fact]
	public void MatchesCall_WithBoolLiteral_ShouldMatch()
	{
		Expression expression = () => TestMethodBool(true);

		var matches = _matcher.MatchesCall(expression, new object[] { true });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithDoubleLiteral_ShouldMatch()
	{
		Expression expression = () => TestMethodDouble(3.14);

		var matches = _matcher.MatchesCall(expression, new object[] { 3.14 });

		matches.Should().BeTrue();
	}

	#endregion

	#region Simple Predicate Tests

	[Fact]
	public void MatchesCall_WithSimpleStringPredicate_ShouldMatch()
	{
		Expression expression = () => TestMethod(Pred.Is<string>(s => s.Equals("test123")));

		var matches = _matcher.MatchesCall(expression, new object[] { "test123" });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithSimpleStringPredicate_ShouldNotMatch()
	{
		Expression expression = () => TestMethod(Pred.Is<string>(s => s.Equals("test123")));

		var matches = _matcher.MatchesCall(expression, new object[] { "different" });

		matches.Should().BeFalse();
	}

	[Fact]
	public void MatchesCall_WithStringStartsWith_ShouldMatch()
	{
		Expression expression = () => TestMethod(Pred.Is<string>(s => s.StartsWith("test")));

		var matches = _matcher.MatchesCall(expression, new object[] { "test123" });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithStringContains_ShouldMatch()
	{
		Expression expression = () => TestMethod(Pred.Is<string>(s => s.Contains("abc")));

		var matches = _matcher.MatchesCall(expression, new object[] { "xyzabcdef" });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithStringLength_ShouldMatch()
	{
		Expression expression = () => TestMethod(Pred.Is<string>(s => s.Length > 5));

		var matches = _matcher.MatchesCall(expression, new object[] { "testing" });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithIntGreaterThan_ShouldMatch()
	{
		Expression expression = () => TestMethodInt(Pred.Is<int>(i => i > 10));

		var matches = _matcher.MatchesCall(expression, new object[] { 15 });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithIntGreaterThan_ShouldNotMatch()
	{
		Expression expression = () => TestMethodInt(Pred.Is<int>(i => i > 10));

		var matches = _matcher.MatchesCall(expression, new object[] { 5 });

		matches.Should().BeFalse();
	}

	[Fact]
	public void MatchesCall_WithIntRange_ShouldMatch()
	{
		Expression expression = () => TestMethodInt(Pred.Is<int>(i => i >= 10 && i <= 20));

		var matches = _matcher.MatchesCall(expression, new object[] { 15 });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithDoubleComparison_ShouldMatch()
	{
		Expression expression = () => TestMethodDouble(Pred.Is<double>(d => d > 2.5));

		var matches = _matcher.MatchesCall(expression, new object[] { 3.14 });

		matches.Should().BeTrue();
	}

	#endregion

	#region Complex Predicate Tests

	[Fact]
	public void MatchesCall_WithComplexStringPredicate_ShouldMatch()
	{
		Expression expression = () => TestMethod(
			Pred.Is<string>(s => s.StartsWith("test") && s.EndsWith("123") && s.Length > 5)
		);

		var matches = _matcher.MatchesCall(expression, new object[] { "test_foo_123" });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithComplexStringPredicate_ShouldNotMatch()
	{
		Expression expression = () => TestMethod(
			Pred.Is<string>(s => s.StartsWith("test") && s.EndsWith("123"))
		);

		var matches = _matcher.MatchesCall(expression, new object[] { "test_foo_456" });

		matches.Should().BeFalse();
	}

	[Fact]
	public void MatchesCall_WithOrPredicate_ShouldMatch()
	{
		Expression expression = () => TestMethodInt(
			Pred.Is<int>(i => i < 10 || i > 100)
		);

		var matches = _matcher.MatchesCall(expression, new object[] { 5 });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithComplexIntPredicate_ShouldMatch()
	{
		Expression expression = () => TestMethodInt(
			Pred.Is<int>(i => (i % 2 == 0) && i > 0)
		);

		var matches = _matcher.MatchesCall(expression, new object[] { 42 });

		matches.Should().BeTrue();
	}

	#endregion

	#region Reference Type Tests

	[Fact]
	public void MatchesCall_WithCustomClass_LiteralValue_ShouldMatch()
	{
		var person = new Person { Name = "John", Age = 30 };
		Expression expression = () => TestMethodPerson(person);

		var matches = _matcher.MatchesCall(expression, new object[] { person });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithCustomClass_Predicate_ShouldMatch()
	{
		Expression expression = () => TestMethodPerson(
			Pred.Is<Person>(p => p.Age > 25)
		);

		var matches = _matcher.MatchesCall(expression, new object[] { new Person { Name = "John", Age = 30 } });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithCustomClass_ComplexPredicate_ShouldMatch()
	{
		Expression expression = () => TestMethodPerson(
			Pred.Is<Person>(p => p.Name.StartsWith("J") && p.Age >= 18)
		);

		var matches = _matcher.MatchesCall(expression, new object[] { new Person { Name = "Jane", Age = 25 } });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithCustomClass_ComplexPredicate_ShouldNotMatch()
	{
		Expression expression = () => TestMethodPerson(
			Pred.Is<Person>(p => p.Name.StartsWith("J") && p.Age >= 18)
		);

		var matches = _matcher.MatchesCall(expression, new object[] { new Person { Name = "Bob", Age = 25 } });

		matches.Should().BeFalse();
	}

	#endregion

	#region Value Type (Struct) Tests

	[Fact]
	public void MatchesCall_WithStruct_LiteralValue_ShouldMatch()
	{
		var point = new Point { X = 10, Y = 20 };
		Expression expression = () => TestMethodPoint(point);

		var matches = _matcher.MatchesCall(expression, new object[] { new Point { X = 10, Y = 20 } });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithStruct_Predicate_ShouldMatch()
	{
		Expression expression = () => TestMethodPoint(
			Pred.Is<Point>(p => p.X > 5 && p.Y > 5)
		);

		var matches = _matcher.MatchesCall(expression, new object[] { new Point { X = 10, Y = 20 } });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithStruct_Predicate_ShouldNotMatch()
	{
		Expression expression = () => TestMethodPoint(
			Pred.Is<Point>(p => p.X > 100)
		);

		var matches = _matcher.MatchesCall(expression, new object[] { new Point { X = 10, Y = 20 } });

		matches.Should().BeFalse();
	}

	#endregion

	#region Null Tests

	[Fact]
	public void MatchesCall_WithNullLiteral_ShouldMatch()
	{
		Expression expression = () => TestMethod(null);

		var matches = _matcher.MatchesCall(expression, new object[] { null });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithNullLiteral_ShouldNotMatchNonNull()
	{
		Expression expression = () => TestMethod(null);

		var matches = _matcher.MatchesCall(expression, new object[] { "test" });

		matches.Should().BeFalse();
	}

	[Fact]
	public void MatchesCall_WithNullPredicate_ShouldMatch()
	{
		Expression expression = () => TestMethod(Pred.Is<string>(s => s == null));

		var matches = _matcher.MatchesCall(expression, new object[] { null });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithNullCheckPredicate_ShouldMatch()
	{
		Expression expression = () => TestMethodPerson(Pred.Is<Person>(p => p != null && p.Age > 18));

		var matches = _matcher.MatchesCall(expression, new object[] { new Person { Name = "John", Age = 30 } });

		matches.Should().BeTrue();
	}

	#endregion

	#region Wildcard Matcher (Any) Tests

	[Fact]
	public void MatchesCall_WithAnyString_ShouldMatchAnyString()
	{
		Expression expression = () => TestMethod(Pred.Any<string>());

		var matches = _matcher.MatchesCall(expression, new object[] { "anything" });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithAnyString_ShouldMatchEmptyString()
	{
		Expression expression = () => TestMethod(Pred.Any<string>());

		var matches = _matcher.MatchesCall(expression, new object[] { "" });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithAnyString_ShouldMatchNull()
	{
		Expression expression = () => TestMethod(Pred.Any<string>());

		var matches = _matcher.MatchesCall(expression, new object[] { null });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithAnyInt_ShouldMatchAnyInt()
	{
		Expression expression = () => TestMethodInt(Pred.Any<int>());

		var matches = _matcher.MatchesCall(expression, new object[] { 42 });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithAnyInt_ShouldMatchZero()
	{
		Expression expression = () => TestMethodInt(Pred.Any<int>());

		var matches = _matcher.MatchesCall(expression, new object[] { 0 });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithAnyInt_ShouldMatchNegative()
	{
		Expression expression = () => TestMethodInt(Pred.Any<int>());

		var matches = _matcher.MatchesCall(expression, new object[] { -999 });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithAnyBool_ShouldMatchTrue()
	{
		Expression expression = () => TestMethodBool(Pred.Any<bool>());

		var matches = _matcher.MatchesCall(expression, new object[] { true });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithAnyBool_ShouldMatchFalse()
	{
		Expression expression = () => TestMethodBool(Pred.Any<bool>());

		var matches = _matcher.MatchesCall(expression, new object[] { false });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithAnyDouble_ShouldMatchAnyDouble()
	{
		Expression expression = () => TestMethodDouble(Pred.Any<double>());

		var matches = _matcher.MatchesCall(expression, new object[] { 3.14159 });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithAnyPerson_ShouldMatchAnyPerson()
	{
		Expression expression = () => TestMethodPerson(Pred.Any<Person>());

		var matches = _matcher.MatchesCall(expression, new object[] { new Person { Name = "Alice", Age = 25 } });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithAnyPerson_ShouldMatchNullPerson()
	{
		Expression expression = () => TestMethodPerson(Pred.Any<Person>());

		var matches = _matcher.MatchesCall(expression, new object[] { null });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithAnyPoint_ShouldMatchAnyPoint()
	{
		Expression expression = () => TestMethodPoint(Pred.Any<Point>());

		var matches = _matcher.MatchesCall(expression, new object[] { new Point { X = 100, Y = 200 } });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithAnyPoint_ShouldMatchDefaultPoint()
	{
		Expression expression = () => TestMethodPoint(Pred.Any<Point>());

		var matches = _matcher.MatchesCall(expression, new object[] { default(Point) });

		matches.Should().BeTrue();
	}

	#endregion

	#region Multiple Arguments with Any Tests

	[Fact]
	public void MatchesCall_WithLiteralAndAny_ShouldMatch()
	{
		Expression expression = () => TestMethodTwo("test", Pred.Any<int>());

		var matches = _matcher.MatchesCall(expression, new object[] { "test", 42 });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithLiteralAndAny_ShouldNotMatchWrongLiteral()
	{
		Expression expression = () => TestMethodTwo("test", Pred.Any<int>());

		var matches = _matcher.MatchesCall(expression, new object[] { "wrong", 42 });

		matches.Should().BeFalse();
	}

	[Fact]
	public void MatchesCall_WithAnyAndLiteral_ShouldMatch()
	{
		Expression expression = () => TestMethodTwo(Pred.Any<string>(), 42);

		var matches = _matcher.MatchesCall(expression, new object[] { "anything", 42 });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithAnyAndPredicate_ShouldMatch()
	{
		Expression expression = () => TestMethodTwo(
			Pred.Any<string>(),
			Pred.Is<int>(i => i > 40)
		);

		var matches = _matcher.MatchesCall(expression, new object[] { "anything", 42 });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithAnyAndPredicate_ShouldNotMatchFailedPredicate()
	{
		Expression expression = () => TestMethodTwo(
			Pred.Any<string>(),
			Pred.Is<int>(i => i > 40)
		);

		var matches = _matcher.MatchesCall(expression, new object[] { "anything", 30 });

		matches.Should().BeFalse();
	}

	[Fact]
	public void MatchesCall_WithAllAny_ShouldMatch()
	{
		Expression expression = () => TestMethodTwo(Pred.Any<string>(), Pred.Any<int>());

		var matches = _matcher.MatchesCall(expression, new object[] { "anything", 999 });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithThreeAny_ShouldMatch()
	{
		Expression expression = () => TestMethodThree(
			Pred.Any<string>(),
			Pred.Any<int>(),
			Pred.Any<bool>()
		);

		var matches = _matcher.MatchesCall(expression, new object[] { "test", 42, true });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithMixedAnyPredicateLiteral_ShouldMatch()
	{
		Expression expression = () => TestMethodThree(
			Pred.Any<string>(),
			Pred.Is<int>(i => i % 2 == 0),
			true
		);

		var matches = _matcher.MatchesCall(expression, new object[] { "anything", 42, true });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithMixedAnyPredicateLiteral_ShouldNotMatch()
	{
		Expression expression = () => TestMethodThree(
			Pred.Any<string>(),
			Pred.Is<int>(i => i % 2 == 0),
			true
		);

		var matches = _matcher.MatchesCall(expression, new object[] { "anything", 42, false });

		matches.Should().BeFalse();
	}

	#endregion

	#region Any vs Is Comparison Tests

	[Fact]
	public void MatchesCall_AnyIsMorePermissiveThanSpecificPredicate()
	{
		Expression anyExpression = () => TestMethod(Pred.Any<string>());
		Expression predicateExpression = () => TestMethod(Pred.Is<string>(s => s.StartsWith("test")));

		var anyMatches = _matcher.MatchesCall(anyExpression, new object[] { "different" });
		var predicateMatches = _matcher.MatchesCall(predicateExpression, new object[] { "different" });

		anyMatches.Should().BeTrue();
		predicateMatches.Should().BeFalse();
	}

	[Fact]
	public void MatchesCall_AnyMatchesWhatLiteralDoesNot()
	{
		Expression anyExpression = () => TestMethod(Pred.Any<string>());
		Expression literalExpression = () => TestMethod("expected");

		var anyMatches = _matcher.MatchesCall(anyExpression, new object[] { "different" });
		var literalMatches = _matcher.MatchesCall(literalExpression, new object[] { "different" });

		anyMatches.Should().BeTrue();
		literalMatches.Should().BeFalse();
	}

	#endregion

	#region Multiple Arguments Tests

	[Fact]
	public void MatchesCall_WithMultipleLiterals_ShouldMatch()
	{
		Expression expression = () => TestMethodTwo("test", 42);

		var matches = _matcher.MatchesCall(expression, new object[] { "test", 42 });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithMultipleLiterals_ShouldNotMatch_WrongFirst()
	{
		Expression expression = () => TestMethodTwo("test", 42);

		var matches = _matcher.MatchesCall(expression, new object[] { "wrong", 42 });

		matches.Should().BeFalse();
	}

	[Fact]
	public void MatchesCall_WithMultipleLiterals_ShouldNotMatch_WrongSecond()
	{
		Expression expression = () => TestMethodTwo("test", 42);

		var matches = _matcher.MatchesCall(expression, new object[] { "test", 99 });

		matches.Should().BeFalse();
	}

	[Fact]
	public void MatchesCall_WithMixedLiteralAndPredicate_ShouldMatch()
	{
		Expression expression = () => TestMethodTwo("test", Pred.Is<int>(i => i > 40));

		var matches = _matcher.MatchesCall(expression, new object[] { "test", 42 });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithMixedLiteralAndPredicate_ShouldNotMatch()
	{
		Expression expression = () => TestMethodTwo("test", Pred.Is<int>(i => i > 40));

		var matches = _matcher.MatchesCall(expression, new object[] { "test", 30 });

		matches.Should().BeFalse();
	}

	[Fact]
	public void MatchesCall_WithMultiplePredicates_ShouldMatch()
	{
		Expression expression = () => TestMethodTwo(
			Pred.Is<string>(s => s.StartsWith("t")),
			Pred.Is<int>(i => i > 40)
		);

		var matches = _matcher.MatchesCall(expression, new object[] { "test", 42 });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithThreeArguments_AllPredicates_ShouldMatch()
	{
		Expression expression = () => TestMethodThree(
			Pred.Is<string>(s => s.Length > 3),
			Pred.Is<int>(i => i % 2 == 0),
			Pred.Is<bool>(b => b == true)
		);

		var matches = _matcher.MatchesCall(expression, new object[] { "test", 42, true });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithGenericArguments_AllPredicates_ShouldMatch()
	{
		Expression expression = () => TestMethodGenerics(
			Pred.Is<string>(s => s.Length > 3),
			Pred.Is<int>(i => i == 42)
		);

		var matches = _matcher.MatchesCall(expression, new object[] { "test", 42 });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithGenericArguments_MixedLiteralAndPredicate_ShouldMatch()
	{
		Expression expression = () => TestMethodGenerics("test", Pred.Is<int>(i => i > 40));

		var matches = _matcher.MatchesCall(expression, new object[] { "test", 42 });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithGenericArguments_MixedLiteralAndPredicate_ShouldNotMatch()
	{
		Expression expression = () => TestMethodTwo("test", Pred.Is<int>(i => i > 40));

		var matches = _matcher.MatchesCall(expression, new object[] { "test", 30 });

		matches.Should().BeFalse();
	}

	#endregion

	#region Argument Count Mismatch Tests

	[Fact]
	public void MatchesCall_WithTooFewArguments_ShouldNotMatch()
	{
		Expression expression = () => TestMethodTwo("test", 42);

		var matches = _matcher.MatchesCall(expression, new object[] { "test" });

		matches.Should().BeFalse();
	}

	[Fact]
	public void MatchesCall_WithTooManyArguments_ShouldNotMatch()
	{
		Expression expression = () => TestMethodTwo("test", 42);

		var matches = _matcher.MatchesCall(expression, new object[] { "test", 42, "extra" });

		matches.Should().BeFalse();
	}

	[Fact]
	public void MatchesCall_WithNoArguments_ShouldMatch()
	{
		Expression expression = () => TestMethodNoArgs();

		var matches = _matcher.MatchesCall(expression, new object[] { });

		matches.Should().BeTrue();
	}

	#endregion

	#region Edge Cases

	[Fact]
	public void MatchesCall_WithEmptyString_ShouldMatch()
	{
		Expression expression = () => TestMethod("");

		var matches = _matcher.MatchesCall(expression, new object[] { "" });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithEmptyStringPredicate_ShouldMatch()
	{
		Expression expression = () => TestMethod(Pred.Is<string>(s => string.IsNullOrEmpty(s)));

		var matches = _matcher.MatchesCall(expression, new object[] { "" });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithZero_ShouldMatch()
	{
		Expression expression = () => TestMethodInt(0);

		var matches = _matcher.MatchesCall(expression, new object[] { 0 });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithNegativeNumber_ShouldMatch()
	{
		Expression expression = () => TestMethodInt(-42);

		var matches = _matcher.MatchesCall(expression, new object[] { -42 });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithNegativeNumberPredicate_ShouldMatch()
	{
		Expression expression = () => TestMethodInt(Pred.Is<int>(i => i < 0));

		var matches = _matcher.MatchesCall(expression, new object[] { -42 });

		matches.Should().BeTrue();
	}

	[Fact]
	public void MatchesCall_WithVariable_ShouldMatch()
	{
		var testString = "test";
		Expression expression = () => TestMethod(testString);

		var matches = _matcher.MatchesCall(expression, new object[] { "test" });

		matches.Should().BeTrue();
	}

	#endregion

	#region ArgMatcher Tests

	[Fact]
	public void ArgMatcher_Matches_WithMatchingValue_ShouldReturnTrue()
	{
		var matcher = new ArgMatcher<string>(s => s.StartsWith("test"));

		var result = matcher.Matches("test123");

		result.Should().BeTrue();
	}

	[Fact]
	public void ArgMatcher_Matches_WithNonMatchingValue_ShouldReturnFalse()
	{
		var matcher = new ArgMatcher<string>(s => s.StartsWith("test"));

		var result = matcher.Matches("other");

		result.Should().BeFalse();
	}

	[Fact]
	public void ArgMatcher_Matches_WithObjectParameter_MatchingValue_ShouldReturnTrue()
	{
		IArgMatcher matcher = new ArgMatcher<string>(s => s.StartsWith("test"));

		var result = matcher.Matches("test123");

		result.Should().BeTrue();
	}

	[Fact]
	public void ArgMatcher_Matches_WithObjectParameter_WrongType_ShouldReturnFalse()
	{
		IArgMatcher matcher = new ArgMatcher<string>(s => s.StartsWith("test"));

		var result = matcher.Matches(123);

		result.Should().BeFalse();
	}

	[Fact]
	public void ArgMatcher_Matches_WithObjectParameter_Null_ShouldReturnFalse()
	{
		IArgMatcher matcher = new ArgMatcher<string>(s => s != null && s.StartsWith("test"));

		var result = matcher.Matches(null);

		result.Should().BeFalse();
	}

	[Fact]
	public void ArgMatcher_Constructor_WithNullPredicate_ShouldThrow()
	{
		Action act = () => new ArgMatcher<string>(null);

		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void ArgMatcher_ImplicitConversionFromValue_ShouldCreateEqualityMatcher()
	{
		ArgMatcher<int> matcher = 42;

		matcher.Matches(42).Should().BeTrue();
		matcher.Matches(99).Should().BeFalse();
	}
	#endregion

	#region Helper Methods

	private void TestMethod(string arg) { }
	private void TestMethodInt(int arg) { }
	private void TestMethodBool(bool arg) { }
	private void TestMethodDouble(double arg) { }
	private void TestMethodPerson(Person arg) { }
	private void TestMethodPoint(Point arg) { }
	private void TestMethodTwo(string arg1, int arg2) { }
	private void TestMethodThree(string arg1, int arg2, bool arg3) { }
	private void TestMethodGenerics<T, S>(T arg1, S arg2) { }
	private void TestMethodNoArgs() { }
	

	#endregion

	#region Test Helper Classes

	private class Person
	{
		public string Name { get; set; }
		public int Age { get; set; }
	}

	private struct Point : IEquatable<Point>
	{
		public int X { get; set; }
		public int Y { get; set; }

		public bool Equals(Point other)
		{
			return X == other.X && Y == other.Y;
		}

		public override bool Equals(object obj)
		{
			return obj is Point other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(X, Y);
		}
	}

	#endregion
}