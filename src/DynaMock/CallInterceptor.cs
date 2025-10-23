using System;
using System.Linq.Expressions;

namespace DynaMock;

/// <summary>
/// Intercepts and routes calls to mock or real implementation
/// </summary>
public class CallInterceptor<T> where T : class
{
    private readonly IMockProvider<T> _mockProvider;
    private readonly T _realImplementation;

    public CallInterceptor(IMockProvider<T> mockProvider, T realImplementation)
    {
        _mockProvider = mockProvider ?? throw new ArgumentNullException(nameof(mockProvider));
        _realImplementation = realImplementation ?? throw new ArgumentNullException(nameof(realImplementation));
    }

    //public TResult InterceptMethod<TResult>(
    //Expression<Func<T, TResult>> methodExpression,
    //Func<T, TResult> realCall,
    //params object?[] args)
    //{
    //    if (_mockProvider.Current == null)
    //        return realCall(_realImplementation);

    //    var memberName = ExtractMemberName(methodExpression);

    //    if (_mockProvider.MockConfig != null)
    //    {
    //        // Check if method is mocked with these specific arguments
    //        if (!_mockProvider.MockConfig.IsMethodMockedWithArgs(memberName, args))
    //        {
    //            return realCall(_realImplementation);
    //        }
    //    }

    //    var mockFunc = methodExpression.Compile();
    //    return mockFunc(_mockProvider.Current);
    //}

    public TResult InterceptMethod<TResult>(
        Expression<Func<T, TResult>> methodExpression,
        Func<T, TResult> realCall,
        params object?[] args)
    {
        // If no mock is set, call the real implementation
        if (_mockProvider.Current == null)
            return realCall(_realImplementation);

        var memberName = ExtractMemberName(methodExpression);
    
        if (_mockProvider.MockConfig?.IsMethodMockedWithArgs(memberName, args) == true)
        {
            if (_mockProvider.MockConfig.TryGetTypeForMethod(memberName, out var targetType))
            {
                return InvokeOnTargetType(methodExpression, 
                    _mockProvider.Current, 
                    targetType!, args);
            }

            var mockFunc = methodExpression.Compile();
            return mockFunc(_mockProvider.Current);
        }

        // No mocking configured for this method call, call the real implementation
        return realCall(_realImplementation);
    }
    private TResult InvokeOnTargetType<TResult>(
        Expression<Func<T, TResult>> methodExpression,
        T target,
        Type targetType,
        object?[] args)
    {
        if (methodExpression.Body is not MethodCallExpression methodCall)
            throw new InvalidOperationException("Expected method call expression");

        var methodInfo = methodCall.Method;
    
        // Find the method on the actual runtime type (handles 'new' methods)
        var actualMethod = targetType.GetMethod(
            methodInfo.Name,
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.Instance,
            null,
            methodInfo.GetParameters().Select(p => p.ParameterType).ToArray(),
            null) ?? methodInfo;

        var result = actualMethod.Invoke(target, args);
        return (TResult)result!;
    }

    public void InterceptVoidMethod(
        Expression<Action<T>> methodExpression,
        Action<T> realCall,
        params object?[] args)
    {
        if (_mockProvider.Current == null)
        {
            realCall(_realImplementation);
            return;
        }

        var memberName = ExtractMemberName(methodExpression);

        if (_mockProvider.MockConfig != null)
        {
            if (!_mockProvider.MockConfig.IsMethodMockedWithArgs(memberName, args))
            {
                realCall(_realImplementation);
                return;
            }
        }

        var mockAction = methodExpression.Compile();
        mockAction(_mockProvider.Current);
    }

    public TProperty InterceptPropertyGet<TProperty>(
        Expression<Func<T, TProperty>> propertyExpression,
        Func<T, TProperty> realCall)
    {
        if (_mockProvider.Current == null)
            return realCall(_realImplementation);

        var memberName = ExtractMemberName(propertyExpression);

        if (_mockProvider.MockConfig != null &&
            !_mockProvider.MockConfig.IsPropertyMocked(memberName))
        {
            return realCall(_realImplementation);
        }

        var mockFunc = propertyExpression.Compile();
        return mockFunc(_mockProvider.Current);
    }

    public void InterceptPropertySet<TProperty>(
        Expression<Func<T, TProperty>> propertyExpression,
        Action<T> realCall,
        TProperty value)
    {
        if (_mockProvider.Current == null)
        {
            realCall(_realImplementation);
            return;
        }

        var memberName = ExtractMemberName(propertyExpression);

        if (_mockProvider.MockConfig != null &&
            !_mockProvider.MockConfig.IsPropertyMocked(memberName))
        {
            realCall(_realImplementation);
            return;
        }

        realCall(_mockProvider.Current);
    }

    public void InterceptEventAdd(
        string eventName,
        Action<T> realCall)
    {
        if (_mockProvider.Current == null)
        {
            realCall(_realImplementation);
            return;
        }

        if (_mockProvider.MockConfig != null &&
            !_mockProvider.MockConfig.IsEventMocked(eventName))
        {
            realCall(_realImplementation);
            return;
        }

        realCall(_mockProvider.Current);
    }

    public void InterceptEventRemove(
        string eventName,
        Action<T> realCall)
    {
        if (_mockProvider.Current == null)
        {
            realCall(_realImplementation);
            return;
        }

        if (_mockProvider.MockConfig != null &&
            !_mockProvider.MockConfig.IsEventMocked(eventName))
        {
            realCall(_realImplementation);
            return;
        }

        realCall(_mockProvider.Current);
    }
    private string ExtractMemberName(Expression expression)
    {
        switch (expression)
        {
            case LambdaExpression lambda:
                return ExtractMemberName(lambda.Body);

            case MethodCallExpression methodCall:
                return methodCall.Method.Name;

            case MemberExpression member:
                return member.Member.Name;

            case UnaryExpression { NodeType: ExpressionType.Convert } unary:
                return ExtractMemberName(unary.Operand);

            default:
                throw new ArgumentException($"Unsupported expression type: {expression.GetType()}");
        }
    }
}