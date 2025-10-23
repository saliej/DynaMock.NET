using System;

namespace DynaMock.UnitTests.TestServices;

public class ConcreteService
{
	public string GetValue()
    {
        return "ConcreteValue";
    }

    public int Add(int first, int second)
    {
        return first + second;
    }

    public int Count { get; set; }
}