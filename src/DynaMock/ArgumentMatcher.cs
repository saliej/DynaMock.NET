using System;
using System.Linq.Expressions;

namespace DynaMock;

// Refactored ArgumentMatcher using Predicate matcher
public class ArgumentMatcher
{
	public bool MatchesCall(Expression callExpression, object?[] actualArgs)
	{
		if (callExpression is not LambdaExpression lambda)
			return false;

		if (lambda.Body is not MethodCallExpression methodCall)
			return false;

		var expectedArgs = methodCall.Arguments;

		if (expectedArgs.Count != actualArgs.Length)
			return false;

		for (int i = 0; i < expectedArgs.Count; i++)
		{
			if (!MatchesArgument(expectedArgs[i], actualArgs[i]))
				return false;
		}

		return true;
	}

	private bool MatchesArgument(Expression expectedExpr, object? actualValue)
	{
		// Handle default(T) - matches any value
		if (IsDefaultExpression(expectedExpr))
			return true;

		// Handle Pred.Is<T>() pattern
		if (IsPredIsPattern(expectedExpr, out var predMatcher))
		{
			return predMatcher.Matches(actualValue);
		}

		// Handle constant values
		if (expectedExpr is ConstantExpression constant)
		{
			return MatchesConstant(constant, actualValue);
		}

		// Handle member access (e.g., variables)
		if (expectedExpr is MemberExpression member)
		{
			var value = GetMemberValue(member);
			return MatchesValue(value, actualValue);
		}

		// Handle method calls that return a value
		if (expectedExpr is MethodCallExpression methodCall)
		{
			var value = EvaluateExpression(methodCall);
			return MatchesValue(value, actualValue);
		}

		// Handle conversions
		if (expectedExpr is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
		{
			return MatchesArgument(unary.Operand, actualValue);
		}

		// Default: try to evaluate and compare
		try
		{
			var value = EvaluateExpression(expectedExpr);
			return MatchesValue(value, actualValue);
		}
		catch
		{
			return false;
		}
	}

	private bool IsDefaultExpression(Expression expr)
	{
		// Only treat explicit default(T) as "match any"
		// Don't treat null constants as default - they should match null specifically
		return expr is DefaultExpression;
	}

	private bool IsPredIsPattern(Expression expr, out IArgMatcher? matcher)
	{
		matcher = null;
		if (expr is MethodCallExpression methodCall)
		{
			if (methodCall.Method.DeclaringType?.Name == "Pred" && methodCall.Method.Name == "Is")
			{
				try
				{
					var value = EvaluateExpression(methodCall);
					if (value is IArgMatcher argMatcher)
					{
						matcher = argMatcher;
						return true;
					}
				}
				catch { }
			}
		}
		return false;
	}

	private bool MatchesConstant(ConstantExpression constant, object? actualValue)
	{
		if (constant.Value is IArgMatcher matcher)
		{
			return matcher.Matches(actualValue);
		}
		return Equals(constant.Value, actualValue);
	}

	private bool MatchesValue(object? expectedValue, object? actualValue)
	{
		if (expectedValue is IArgMatcher matcher)
		{
			return matcher.Matches(actualValue);
		}
		return Equals(expectedValue, actualValue);
	}

	private object? GetMemberValue(MemberExpression member)
	{
		var objectMember = Expression.Convert(member, typeof(object));
		var getterLambda = Expression.Lambda<Func<object>>(objectMember);
		var getter = getterLambda.Compile();
		return getter();
	}

	private object? EvaluateExpression(Expression expr)
	{
		var objectMember = Expression.Convert(expr, typeof(object));
		var lambda = Expression.Lambda<Func<object>>(objectMember);
		var func = lambda.Compile();
		return func();
	}
}