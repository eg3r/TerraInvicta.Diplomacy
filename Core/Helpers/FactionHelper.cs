using System;
using System.Linq;
using Diplomacy.Core.Treaty;
using PavonisInteractive.TerraInvicta;
using UnityModManagerNet;

namespace Diplomacy.Core.Helpers;

public static class FactionHelper
{
    public static DiplomacyLevel MaxDiplomacyLevelWith(this TIFactionState faction, TIFactionState other)
    {
        if (faction.HasBrokenAlliance(other))
            return DiplomacyLevel.Conflict;

        return IdeologyDiff(faction, other) switch
        {
            < 1.42f => DiplomacyLevel.Allied,
            >= 1.42f and < 2.2f => DiplomacyLevel.Friendly,
            >= 2.2f and < 2.9f => DiplomacyLevel.Normal,
            >= 2.9f and < 3.1f => DiplomacyLevel.Conflict,
            >= 3.1f => DiplomacyLevel.Enemy,
            _ => DiplomacyLevel.Normal
        };
    }

    public static float IdeologyDiffX(this TIFactionState faction, TIFactionState other)
    {
        return Math.Abs(faction.ideologyCoordinates.x - other.ideologyCoordinates.x);
    }

    public static float IdeologyDiffY(this TIFactionState faction, TIFactionState other)
    {
        return Math.Abs(faction.ideologyCoordinates.y - other.ideologyCoordinates.y);
    }

    public static float IdeologyDiff(this TIFactionState faction, TIFactionState other)
    {
        return TINationState.GetIdeologicalDistance(faction.ideologyCoordinates, other.ideologyCoordinates);
    }

    public static bool IsAtWar(this TIFactionState faction, TIFactionState other, bool bothSides = false)
    {
        return faction.FindGoals(GoalType.WarOnFaction, faction, other).Any()
               || (bothSides && other.FindGoals(GoalType.WarOnFaction, other, faction).Any());
    }

    public static bool HasNap(this TIFactionState faction, TIFactionState other)
    {
        return faction.FindGoals(GoalType.NonAggressionPact, faction, other).Any() ||
               other.FindGoals(GoalType.NonAggressionPact, other, faction).Any();
    }

    public static bool HasTruce(this TIFactionState faction, TIFactionState other)
    {
        return faction.FindGoals(GoalType.TruceWithFaction, faction, other).Any() ||
               other.FindGoals(GoalType.TruceWithFaction, other, faction).Any();
    }

    public static bool HasAlliance(this TIFactionState faction, TIFactionState other)
    {
        return ModState.IsTreatyValid(faction, other, DiplomacyTreatyType.Alliance);
    }

    public static bool HasBrokenAlliance(this TIFactionState faction, TIFactionState other)
    {
        return ModState.IsTreatyValid(faction, other, DiplomacyTreatyType.AllianceBroken);
    }

    public static DiplomacyLevel CurrentDiplomacyLevelWith(this TIFactionState faction, TIFactionState other)
    {
        var currentHate = other.GetFactionHate(faction);

        if (faction.IsAtWar(other, true))
            return DiplomacyLevel.War;

        if (faction.HasNap(other))
            return DiplomacyLevel.Friendly;

        if (faction.HasAlliance(other))
            return DiplomacyLevel.Allied;

        var maxDipLevel = faction.MaxDiplomacyLevelWith(other);

        if (currentHate > TemplateManager.global.factionHateWarThreshold)
            return DiplomacyLevel.Enemy.Min(maxDipLevel);

        if (currentHate > TemplateManager.global.factionHateConflictThreshold)
            return DiplomacyLevel.Conflict.Min(maxDipLevel);

        if (currentHate >= -1)
            return DiplomacyLevel.Normal.Min(maxDipLevel);

        UnityModManager.Logger.Log("CurrentDiplomacyLevelWith error");
        return DiplomacyLevel.Normal; // fallback
    }

    public static void ResetRelations(this TIFactionState faction, TIFactionState other, bool bothWays = false)
    {
        faction.SetFactionHate(other, 0);
        if (bothWays)
            other.SetFactionHate(faction, 0);
    }
}