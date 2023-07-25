﻿using System;
using System.Linq;
using PavonisInteractive.TerraInvicta;
using UnityModManagerNet;

namespace Diplomacy.Core;

public static class FactionHelper
{
    public static DiplomacyLevel MaxDiplomacyLevelWith(this TIFactionState faction, TIFactionState other)
    {
        return IdeologyDiff(faction, other) switch
        {
            <= 1.5f => DiplomacyLevel.Allied,
            > 1.5f and <= 2.25f => DiplomacyLevel.Friendly,
            > 2.25f and < 3.75f => DiplomacyLevel.Normal,
            >= 3.75f => DiplomacyLevel.Enemy,
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

    public static DiplomacyLevel CurrentDiplomacyLevelWith(this TIFactionState faction, TIFactionState other)
    {
        var currentHate = other.GetFactionHate(faction);

        if (faction.IsAtWar(other, true))
            return DiplomacyLevel.War;

        if (faction.HasNap(other))
            return DiplomacyLevel.Allied;

        var maxDipLevel = faction.MaxDiplomacyLevelWith(other);

        if (currentHate > TemplateManager.global.factionHateWarThreshold)
            return DiplomacyLevel.Enemy.Min(maxDipLevel);

        if (currentHate > TemplateManager.global.factionHateConflictThreshold)
            return DiplomacyLevel.Conflict.Min(maxDipLevel);

        if (currentHate > TemplateManager.global.factionHateConflictThreshold * 0.5f)
            return DiplomacyLevel.Normal.Min(maxDipLevel);

        if (currentHate >= -1)
            return DiplomacyLevel.Friendly.Min(maxDipLevel);

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