using System;

namespace Diplomacy.Core;

[Flags]
public enum DiplomacyLevel
{
    War = -4,
    Enemy = -2,
    Conflict = -1,
    Normal = 0,
    Friendly = 1,
    Allied = 2
}