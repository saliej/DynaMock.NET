using System;
using System.Collections.Generic;

namespace DynaMock.SourceGeneration.Models;

public class MethodModel
{
    public string Name { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public List<(string Type, string Name)> Parameters { get; set; } = [];
    public bool IsAsync { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsAbstract { get; set; }
    public string? BaseImplementation { get; set; }
    public List<string> TypeParameters { get; set; } = [];
    public List<string> Constraints { get; set; } = [];
    public bool HasImplementation { get; set; }
}
