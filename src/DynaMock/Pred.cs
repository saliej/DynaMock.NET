using System;

namespace DynaMock;

public class Pred
{
	public static ArgMatcher<T> Is<T>(Func<T, bool> predicate)
	{
		return new ArgMatcher<T>(predicate);
	}

	public static ArgMatcher<T> Any<T>()
	{
		return new ArgMatcher<T>(_ => true);
	}
}

public interface IArgMatcher
{
	bool Matches(object? arg);
}

public class ArgMatcher<T> : IArgMatcher
{
	private readonly Func<T, bool> _predicate;

	public ArgMatcher(Func<T, bool> predicate)
	{
		_predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
	}

	public bool Matches(T arg)
	{
		return _predicate(arg);
	}

	// Implement non-generic interface
	public bool Matches(object? arg)
	{
		// Handle null explicitly - let the predicate decide
		if (arg == null)
		{
			return _predicate((T)(object)null);
		}

		if (arg is T typedArg)
		{
			return _predicate(typedArg);
		}

		return false;
	}

	public static implicit operator T(ArgMatcher<T> matcher)
	{
		return default;
	}

	public static implicit operator ArgMatcher<T>(T value)
	{
		return new ArgMatcher<T>(arg => EqualityComparer<T>.Default.Equals(arg, value));
	}
}
