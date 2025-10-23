using System;
using System.Linq.Expressions;

namespace DynaMock;

public class MockConfiguration<T> where T : class
{
    private readonly HashSet<string> _mockedMethods = [];
    private readonly HashSet<string> _mockedProperties = [];
    private readonly HashSet<string> _mockedEvents = [];
    private readonly Dictionary<string, List<Expression>> _methodCallPatterns = [];

    private readonly Dictionary<string, Type> _targetTypeCallMap = [];

    public IReadOnlySet<string> MockedMethods => _mockedMethods;
    public IReadOnlySet<string> MockedProperties => _mockedProperties;
    public IReadOnlySet<string> MockedEvents => _mockedEvents;

    public MockConfiguration<T> MockMethod<TResult>(Expression<Func<T, TResult>> methodExpression)
    {
        var memberName = ExtractMemberName(methodExpression);
        _mockedMethods.Add(memberName);
        
        // Store the full expression for argument matching
        if (!_methodCallPatterns.ContainsKey(memberName))
            _methodCallPatterns[memberName] = [];
        
        _methodCallPatterns[memberName].Add(methodExpression);
        
        return this;
    }

    public MockConfiguration<T> MockMethod<TResult, TTarget>(Expression<Func<T, TResult>> methodExpression, 
        TTarget target) where TTarget : T
    {
        MockMethod(methodExpression);

        _targetTypeCallMap.Add(ExtractMemberName(methodExpression), typeof(TTarget));

        return this;
    }

    public MockConfiguration<T> MockMethod(Expression<Action<T>> methodExpression)
    {
        var memberName = ExtractMemberName(methodExpression);
        _mockedMethods.Add(memberName);
        
        if (!_methodCallPatterns.ContainsKey(memberName))
            _methodCallPatterns[memberName] = [];
        
        _methodCallPatterns[memberName].Add(methodExpression);
        
        return this;
    }

    public MockConfiguration<T> MockProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
    {
        var memberName = ExtractMemberName(propertyExpression);
        _mockedProperties.Add(memberName);
        return this;
    }

    public MockConfiguration<T> MockEvent(string eventName)
    {
        _mockedEvents.Add(eventName);
        return this;
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

    public bool IsMethodMocked(string methodName) => _mockedMethods.Contains(methodName);
    
    public bool IsMethodMockedWithArgs(string methodName, object?[] args)
    {
        if (!_mockedMethods.Contains(methodName))
            return false;

        // If no specific patterns registered, any call to this method is mocked
        if (!_methodCallPatterns.ContainsKey(methodName) || 
            _methodCallPatterns[methodName].Count == 0)
            return true;

        // Check if any pattern matches the arguments
        var matcher = new ArgumentMatcher();
        return _methodCallPatterns[methodName].Any(pattern => 
            matcher.MatchesCall(pattern, args));
    }
    
    public bool IsPropertyMocked(string propertyName) => _mockedProperties.Contains(propertyName);
    public bool IsEventMocked(string eventName) => _mockedEvents.Contains(eventName);

    public bool TryGetTypeForCall(string methodName, out Type? targetType) =>
        _targetTypeCallMap.TryGetValue(methodName, out targetType);
}