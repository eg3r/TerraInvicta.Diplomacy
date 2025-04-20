using System;
using System.Collections.Generic;
using System.Linq;
using Diplomacy.Core.Treaty;
using JetBrains.Annotations;
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

    public static bool IsAtWarWith(this TIFactionState faction, TIFactionState other, bool bothSides = false)
    {
        return faction.FindGoals(GoalType.WarOnFaction, faction, other).Any()
               || (bothSides && other.FindGoals(GoalType.WarOnFaction, other, faction).Any());
    }

    public static bool HasNap(this TIFactionState faction, TIFactionState other, bool bothSides = true)
    {
        return faction.FindGoals(GoalType.NonAggressionPact, faction, other).Any() ||
               (bothSides && other.FindGoals(GoalType.NonAggressionPact, other, faction).Any());
    }

    public static bool HasTruce(this TIFactionState faction, TIFactionState other, bool bothSides = true)
    {
        return faction.FindGoals(GoalType.TruceWithFaction, faction, other).Any() ||
               (bothSides && other.FindGoals(GoalType.TruceWithFaction, other, faction).Any());
    }

    public static bool HasAllianceWith(this TIFactionState faction, TIFactionState other)
    {
        return ModState.IsTreatyValid(faction, other, DiplomacyTreatyType.Alliance);
    }

    public static bool IsIntelSharedBy(this TIFactionState faction, TIFactionState other)
    {
        // if sharing intel, (this) is added to the (other) intelSharingFactions list
        // meaning we need to check their intelSharingFactions list if we share itel with them
        // and our intelSharingFactions list if they share intel with us
        return faction.intelSharingFactions.Contains(other);
    }

    public static List<TIFactionState> GetAlliances(this TIFactionState faction)
    {
        return ModState.GetAlliancesFor(faction);
    }

    public static bool HasBrokenAlliance(this TIFactionState faction, TIFactionState other)
    {
        return ModState.IsTreatyValid(faction, other, DiplomacyTreatyType.AllianceBroken);
    }

    [CanBeNull]
    public static DiplomacyTreaty GetLatestActiveTreaty(this TIFactionState faction, TIFactionState other)
    {
        return ModState.GetLatestTreatyBetween(faction, other);
    }

    public static DiplomacyLevel CurrentDiplomacyLevelWith(this TIFactionState faction, TIFactionState other)
    {
        if (faction.IsAtWarWith(other, true))
            return DiplomacyLevel.War;

        if (faction.permanentAlly(other))
            return DiplomacyLevel.Allied;

        // TODO: investigate intel sharing system and adjust if needed, for not intelsharing = friendly
        if (faction.HasNap(other) || faction.intelSharingFactions.Contains(other))
            return DiplomacyLevel.Friendly;

        var maxDipLevel = faction.MaxDiplomacyLevelWith(other);
        var currentHate = other.GetFactionHate(faction);

        if (currentHate >= TemplateManager.global.factionHateWarThreshold)
            return DiplomacyLevel.Enemy.Min(maxDipLevel);

        if (currentHate >= TemplateManager.global.factionHateConflictThreshold)
            return DiplomacyLevel.Conflict.Min(maxDipLevel);

        if (currentHate >= -100 && currentHate < TemplateManager.global.factionHateConflictThreshold)
            return DiplomacyLevel.Normal.Min(maxDipLevel);

        UnityModManager.Logger.Log("CurrentDiplomacyLevelWith error");
        return DiplomacyLevel.Normal.Min(maxDipLevel); // fallback
    }

    public static void ResetRelations(this TIFactionState faction, TIFactionState other, bool bothWays = false)
    {
        faction.SetFactionHate(other, 0);
        if (bothWays)
            other.SetFactionHate(faction, 0);
    }
}