using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynaMock.SourceGeneration.Models;

public struct TypeToMock
{
	public TypeToMock(INamedTypeSymbol typeSymbol, Location location, bool virtualMembers)
	{
		TypeSymbol = typeSymbol ?? throw new ArgumentNullException(nameof(typeSymbol));
		Location = location ?? throw new ArgumentNullException(nameof(location));
		VirtualMembers = virtualMembers;
	}

	public INamedTypeSymbol TypeSymbol { get; set; }
	public Location Location { get; set; }
	public bool VirtualMembers { get; set; }
}
