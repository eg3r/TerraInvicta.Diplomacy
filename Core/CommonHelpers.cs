namespace Diplomacy.Core;

public static class CommonHelpers
{
    public static DiplomacyLevel Min(this DiplomacyLevel level, DiplomacyLevel other)
    {
        return level > other ? other : level;
    }
}