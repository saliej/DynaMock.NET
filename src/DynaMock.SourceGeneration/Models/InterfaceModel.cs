using System;
using System.Collections.Generic;

namespace DynaMock.SourceGeneration.Models;

public class InterfaceModel
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public List<MethodModel> Methods { get; set; } = [];
    public List<PropertyModel> Properties { get; set; } = [];
}
