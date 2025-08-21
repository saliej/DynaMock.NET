﻿using System;

namespace DynaMock.SourceGeneration.Models;

internal class EventModel
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsVirtual { get; set; }
    public bool IsAbstract { get; set; }
}