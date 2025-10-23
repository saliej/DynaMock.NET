using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DynaMock;

/// <summary>
/// Matches method call arguments
/// </summary>
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

        // Handle Arg.Any<T>() pattern - would need custom implementation
        if (IsArgAnyPattern(expectedExpr))
            return true;

        // Handle constant values
        if (expectedExpr is ConstantExpression constant)
        {
            return Equals(constant.Value, actualValue);
        }

        // Handle member access (e.g., variables)
        if (expectedExpr is MemberExpression member)
        {
            var value = GetMemberValue(member);
            return Equals(value, actualValue);
        }

        // Handle method calls that return a value
        if (expectedExpr is MethodCallExpression methodCall)
        {
            var value = EvaluateExpression(methodCall);
            return Equals(value, actualValue);
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
            return Equals(value, actualValue);
        }
        catch
        {
            return false;
        }
    }

    private bool IsDefaultExpression(Expression expr)
    {
        return expr is DefaultExpression ||
               (expr is ConstantExpression { Value: null });
    }

    private bool IsArgAnyPattern(Expression expr)
    {
        // Check for Arg.Any<T>() or similar patterns
        if (expr is MethodCallExpression methodCall)
        {
            if (methodCall.Method.DeclaringType?.Name == "Arg" && 
                methodCall.Method.Name == "Any")
            {
                return true;
            }
        }
        return false;
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