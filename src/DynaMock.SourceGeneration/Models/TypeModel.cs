using System;
using System.Collections.Generic;

namespace DynaMock.SourceGeneration.Models;

internal class TypeModel
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public bool IsInterface { get; set; }
    public List<MethodModel> Methods { get; set; } = [];
    public List<PropertyModel> Properties { get; set; } = [];
    public List<EventModel> Events { get; set; } = []; // Add this
    public List<string> GenericTypeParameters { get; set; } = [];
    public List<string> GenericTypeConstraints { get; set; } = [];
}