using System.Collections.Generic;
using System.Linq;
using Diplomacy.Core.Helpers;
using Diplomacy.Core.Treaty;
using HarmonyLib;
using ModestTree;
using PavonisInteractive.TerraInvicta;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local

namespace Diplomacy.Core.Patches;

[HarmonyPatch(typeof(TIFactionState))]
public class TiFactionStatePatch
{
    // NOTE: ProcessTrade is called twice from DiplomacyTradeAction.Execute (once for each trade side)
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TIFactionState.ProcessTrade))]
    private static bool ProcessTradePrefix(
        TradeOffer acceptedOffer,
        ref float tradeHateModifier,
        TIFactionState otherFaction,
        TIFactionState __instance)
    {
        if (ModState.CurrentTreatyType == DiplomacyTreatyType.None)
            return true;

        // ProcessTrade called once for every trade party, add it only once 
        var firstPass = !ModState.IsTreatyValid(otherFaction, __instance, ModState.CurrentTreatyType);
        if (firstPass)
            ModState.AddTreaty(new DiplomacyTreaty(__instance, otherFaction, ModState.CurrentTreatyType));

        switch (ModState.CurrentTreatyType)
        {
            case DiplomacyTreatyType.ResetRelation:
                if (firstPass)
                    ModState.RemoveTruce(__instance, otherFaction);

                // apply reset 
                __instance.ResetRelations(otherFaction);

                // reset modifier as it is not needed anymore for this treaty type
                tradeHateModifier = 0;
                break;
            case DiplomacyTreatyType.Alliance:
                // remove possible NAP
                if (firstPass)
                    ModState.RemoveNapTreaty(__instance, otherFaction);

                // !NEW NOW: BeginIntelSharingWith call takes care of intel sharing
                if (!__instance.IsIntelSharedBy(otherFaction))
                    otherFaction.BeginIntelSharingWith(__instance);

                // reset modifier as it is not needed anymore for this treaty type
                tradeHateModifier = 0;
                break;
            case DiplomacyTreatyType.AllianceBroken:
                // remove alliance treaty
                if (firstPass)
                    ModState.RemoveAllianceTreaty(__instance, otherFaction);

                // remove intel sharing
                __instance.EndIntelSharingWith(otherFaction);

                // after alliance was broken add hate
                tradeHateModifier = -TemplateManager.global.factionHateConflictThreshold;
                break;
            // the base game treaties are still done by the game
            case DiplomacyTreatyType.Intel:
            case DiplomacyTreatyType.Truce:
            case DiplomacyTreatyType.Nap:
            case DiplomacyTreatyType.None:
            default:
                break;
        }

        // done both passes, reset process of current treaty
        if (!firstPass)
            ModState.CurrentTreatyType = DiplomacyTreatyType.None;

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(TIFactionState.NetTradeScore))]
    private static float NetTradeScorePostfix(
        float originalScore,
        TradeOffer actingPlayerOffer,
        TradeOffer otherFactionOffer,
        ref float myScore,
        ref float theirScore)
    {
        if (ModState.CurrentTreatyType != DiplomacyTreatyType.ResetRelation)
            return originalScore;

        var offerFaction = actingPlayerOffer.offeringFaction;
        var receivingFaction = otherFactionOffer.offeringFaction;
        var hateTowardsThem = offerFaction.GetFactionHate(receivingFaction);
        var hateTowardsMe = receivingFaction.GetFactionHate(offerFaction);
        var hateDelta = hateTowardsThem - hateTowardsMe;

        // check who hates whom more and give a bonus
        theirScore *= hateDelta > 0 ? 1.25f : 0.75f;

        // take risk aversion also into consideration
        if (receivingFaction.currentRiskAversion > offerFaction.currentRiskAversion)
            theirScore *= 1.25f;

        // small bonus on malleable factions
        if (receivingFaction.malleable)
            theirScore *= 1.1f;

        return theirScore - myScore;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(TIFactionState.permanentAlly))]
    private static bool PermanentAllyPostfix(bool bPermanentAlly, TIFactionState faction, TIFactionState __instance)
    {
        if (bPermanentAlly)
            return true;

        if (faction == null || __instance == null)
            return false;

        if (!__instance.isActivePlayer && !faction.isActivePlayer)
            return false;

        return __instance.HasAllianceWith(faction);
    }


    [HarmonyPrefix]
    [HarmonyPatch(nameof(TIFactionState.StealableProjects))]
    private static bool StealableProjectsPrefix(
        TIFactionState stealingFaction,
        ref List<TIProjectTemplate> __result,
        TIFactionState __instance)
    {
        if (!__instance.HasAllianceWith(stealingFaction))
            return true;

        __result = new List<TIProjectTemplate>();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TIFactionState.ProjectsVulnerableToSabotage))]
    private static bool ProjectsVulnerableToSabotagePrefix(
        TIFactionState sabotagingFaction,
        ref List<TIProjectTemplate> __result,
        TIFactionState __instance)
    {
        if (!__instance.HasAllianceWith(sabotagingFaction))
            return true;

        __result = new List<TIProjectTemplate>();
        return false;
    }
}