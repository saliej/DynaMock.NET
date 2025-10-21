using System;

namespace DynaMock.SourceGeneration.Models;

internal class PropertyModel
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool HasGetter { get; set; }
    public bool HasSetter { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsAbstract { get; set; }
    public string? GetterBaseImplementation { get; set; }
    public string? SetterBaseImplementation { get; set; }
}