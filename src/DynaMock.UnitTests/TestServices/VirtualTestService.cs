using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynaMock.UnitTests.TestServices;

/// <summary>
/// Test service for VirtualMembers = true tests
/// </summary>
public class VirtualTestService
{
	public virtual string Name { get; set; } = "Virtual Service";
	public virtual int Count { get; set; }

	public virtual string GetValue()
	{
		return "Virtual Value";
	}

	public virtual string GetDefaultValue()
	{
		return "Default Virtual Value";
	}

	public virtual int Calculate(int a, int b)
	{
		return a + b;
	}

	public virtual void DoSomething()
	{
		// Implementation
	}
}

