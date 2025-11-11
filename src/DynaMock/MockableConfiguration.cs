using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DynaMock;

public class MockConfiguration<T> where T : class
{
    private readonly HashSet<string> _mockedMethods = [];
    private readonly HashSet<string> _mockedProperties = [];
    private readonly HashSet<string> _mockedEvents = [];

    private readonly Dictionary<string, List<Expression>> _methodCallPatterns = [];
    private readonly Dictionary<string, List<LambdaExpression>> _eventCallPatterns = []; // NEW

    private readonly Dictionary<string, Type> _targetTypeCallMap = [];

    internal IReadOnlySet<string> MockedMethods => _mockedMethods;
    internal IReadOnlySet<string> MockedProperties => _mockedProperties;
    internal IReadOnlySet<string> MockedEvents => _mockedEvents;

    // Optional: expose read-only access if useful elsewhere
    internal IReadOnlyDictionary<string, List<LambdaExpression>> EventCallPatterns => _eventCallPatterns;

    public MockConfiguration<T> MockMethod<TResult>(Expression<Func<T, TResult>> methodExpression)
    {
        var memberName = ExtractMemberName(methodExpression);
        _mockedMethods.Add(memberName);

        if (!_methodCallPatterns.ContainsKey(memberName))
            _methodCallPatterns[memberName] = [];

        _methodCallPatterns[memberName].Add(methodExpression);

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

    public MockConfiguration<T> MockMethod<TResult, TTarget>(Expression<Func<T, TResult>> methodExpression,
        TTarget target) where TTarget : T
    {
        MockMethod(methodExpression);
        _targetTypeCallMap.Add(ExtractMemberName(methodExpression), typeof(TTarget));
        return this;
    }

    public MockConfiguration<T> MockProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
    {
        var memberName = ExtractMemberName(propertyExpression);
        _mockedProperties.Add(memberName);
        return this;
    }

    // Existing simple event registration
    public MockConfiguration<T> MockEvent(string eventName)
    {
        _mockedEvents.Add(eventName);
        return this;
    }

    // NEW: event registration with argument-matching pattern (sender, args)
    public MockConfiguration<T> MockEvent<TSender, TEventArgs>(
        string eventName,
        Expression<Func<TSender, TEventArgs, bool>> argsPattern)
    {
        _mockedEvents.Add(eventName);

        if (!_eventCallPatterns.TryGetValue(eventName, out var list))
        {
            list = [];
            _eventCallPatterns[eventName] = list;
        }

        list.Add(argsPattern);
        return this;
    }

    public MockConfiguration<T> MockEvent<TSender, TEventArgs>(
        Expression<Action<T>> eventExpression,
        Expression<Func<TSender, TEventArgs, bool>> argsPattern)
    {
        var eventName = ExtractEventName(eventExpression);
        _mockedEvents.Add(eventName);

        if (!_eventCallPatterns.TryGetValue(eventName, out var list))
        {
            list = [];
            _eventCallPatterns[eventName] = list;
        }

        list.Add(argsPattern);
        return this;
    }

    // Helper to pull the event name out of an expression like: x => x.MyEvent += null
    private static string ExtractEventName(LambdaExpression eventExpression)
    {
        static string FromMethodName(string methodName)
        {
            if (methodName.StartsWith("add_", StringComparison.Ordinal))
                return methodName.Substring(4);
            if (methodName.StartsWith("remove_", StringComparison.Ordinal))
                return methodName.Substring(7);
            return methodName; // fallback
        }

        Expression body = eventExpression.Body;

        // Unwrap conversions
        if (body is UnaryExpression { NodeType: ExpressionType.Convert } u)
            body = u.Operand;

        // Case 1: add/remove accessor compiled as a special-name method call (add_Event / remove_Event)
        if (body is MethodCallExpression call && call.Method.IsSpecialName)
            return FromMethodName(call.Method.Name);

        // Case 2: event add/remove represented as a binary add-assign/sub-assign on a MemberExpression
        if (body is BinaryExpression bin &&
            (bin.NodeType == ExpressionType.AddAssign || bin.NodeType == ExpressionType.SubtractAssign))
        {
            if (bin.Left is MemberExpression me)
                return me.Member.Name;
        }

        // (Very rare) direct member expression
        if (body is MemberExpression me2)
            return me2.Member.Name;

        throw new ArgumentException($"Unsupported event expression form: {body.NodeType}");
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

        if (!_methodCallPatterns.ContainsKey(methodName) ||
            _methodCallPatterns[methodName].Count == 0)
            return true;

        var matcher = new ArgumentMatcher();
        return _methodCallPatterns[methodName].Any(pattern =>
            matcher.MatchesCall(pattern, args));
    }

    public bool IsPropertyMocked(string propertyName) => _mockedProperties.Contains(propertyName);
    public bool IsEventMocked(string eventName) => _mockedEvents.Contains(eventName);

    public bool IsEventMockedWithArgs(string eventName, object? sender, object? eventArgs)
    {
        if (!_mockedEvents.Contains(eventName))
            return false;

        // If no specific patterns registered, any raise of this event is considered mocked
        if (!_eventCallPatterns.ContainsKey(eventName) || _eventCallPatterns[eventName].Count == 0)
            return true;

        foreach (var lambda in _eventCallPatterns[eventName])
        {
            try
            {
                // Each pattern is Expression<Func<TSender, TEventArgs, bool>>
                var compiled = lambda.Compile();
                var result = (bool)compiled.DynamicInvoke(sender, eventArgs)!;
                if (result) return true;
            }
            catch
            {
                // Ignore incompatible patterns (e.g., wrong arg types); try next
            }
        }

        return false;
    }

    public bool IsEventMockedWithArgs(string eventName, params object?[] args)
    {
        var sender = args.Length > 0 ? args[0] : null;
        var eventArgs = args.Length > 1 ? args[1] : null;
        return IsEventMockedWithArgs(eventName, sender, eventArgs);
    }

    public bool TryGetTypeForMethod(string methodName, out Type? targetType) =>
        _targetTypeCallMap.TryGetValue(methodName, out targetType);
}
