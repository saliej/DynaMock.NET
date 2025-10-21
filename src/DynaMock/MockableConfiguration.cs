using System;
using System.Linq.Expressions;

namespace DynaMock;

public class MockConfiguration<T> where T : class
{
	private readonly HashSet<string> _mockedMethods = [];
	private readonly HashSet<string> _mockedProperties = [];
	private readonly HashSet<string> _mockedEvents = [];

	public IReadOnlySet<string> MockedMethods => _mockedMethods;
	public IReadOnlySet<string> MockedProperties => _mockedProperties;
	public IReadOnlySet<string> MockedEvents => _mockedEvents;

	public MockConfiguration<T> MockMethod<TResult>(Expression<Func<T, TResult>> methodExpression)
	{
		if (methodExpression == null)
		{
			throw new ArgumentNullException(nameof(methodExpression), "Method expression cannot be null.");
		}

		var memberName = ExtractMemberName(methodExpression);
		_mockedMethods.Add(memberName);
		return this;
	}

	public MockConfiguration<T> MockMethod(Expression<Action<T>> methodExpression)
	{
		if (methodExpression == null)
		{
			throw new ArgumentNullException(nameof(methodExpression), "Method expression cannot be null.");
		}

		var memberName = ExtractMemberName(methodExpression);
		_mockedMethods.Add(memberName);
		return this;
	}

	public MockConfiguration<T> MockProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
	{
		if (propertyExpression == null)
		{
			throw new ArgumentNullException(nameof(propertyExpression), "Property expression cannot be null.");
		}

		var memberName = ExtractMemberName(propertyExpression);
		_mockedProperties.Add(memberName);
		return this;
	}

	public MockConfiguration<T> MockEvent(string eventName)
	{
		if (string.IsNullOrWhiteSpace(eventName))
		{
			throw new ArgumentException("Event name cannot be null or empty.", nameof(eventName));
		}

		_mockedEvents.Add(eventName);
		return this;
	}

	private string ExtractMemberName(Expression expression)
	{
		switch (expression)
		{
			case LambdaExpression lambda:
				if (lambda.Body == null)
				{
					throw new ArgumentException("Lambda expression body cannot be null.", nameof(expression));
				}
				return ExtractMemberName(lambda.Body);

			case MethodCallExpression methodCall:
				if (methodCall.Method == null)
				{
					throw new ArgumentException("Method call expression must reference a valid method.", nameof(expression));
				}

				// For method calls, the target should be the parameter of type T (the class being mocked)
				if (methodCall.Object is not ParameterExpression)
				{
					throw new ArgumentException(
						$"Invalid method call expression. Method calls must be called on the class instance being mocked. " +
						$"Use 'x => x.MethodName()' instead of 'x => someOtherObject.MethodName()'.",
						nameof(expression));
				}

				return methodCall.Method.Name;

			case MemberExpression member:
				if (member.Member == null)
				{
					throw new ArgumentException("Member expression must reference a valid member.", nameof(expression));
				}

				// For properties/methods, the expression should be a parameter of type T (the class being mocked)
				if (member.Expression is not ParameterExpression parameter || parameter.Type != typeof(T))
				{
					throw new ArgumentException(
						$"Invalid member expression: '{member.Member.Name}'. " +
						$"Members must be accessed directly on the class instance being mocked. " +
						$"Use 'x => x.MemberName' instead of 'x => someOtherObject.MemberName'.",
						nameof(expression));
				}

				return member.Member.Name;

			case UnaryExpression { NodeType: ExpressionType.Convert } unary:
				if (unary.Operand == null)
				{
					throw new ArgumentException("Convert expression must have a valid operand.", nameof(expression));
				}
				return ExtractMemberName(unary.Operand);

			default:
				throw new ArgumentException(
					$"Unsupported expression type: {expression.GetType().Name}. " +
					"Expected a lambda expression accessing a method or property on the mocked class instance. " +
					"Example: 'x => x.MethodName()' for methods or 'x => x.PropertyName' for properties.",
					nameof(expression));
		}
	}

	public bool IsMethodMocked(string methodName) => _mockedMethods.Contains(methodName);
	public bool IsPropertyMocked(string propertyName) => _mockedProperties.Contains(propertyName);
	public bool IsEventMocked(string eventName) => _mockedEvents.Contains(eventName);
}