using System;
using Diplomacy.Core.Helpers;
using Diplomacy.Core.Treaty;
using HarmonyLib;
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
        if (!ModState.IsTreatyValid(otherFaction, __instance, ModState.CurrentTreatyType))
            ModState.AddTreaty(new DiplomacyTreaty(__instance, otherFaction, ModState.CurrentTreatyType));

        switch (ModState.CurrentTreatyType)
        {
            case DiplomacyTreatyType.ResetRelation:
                // apply reset 
                __instance.ResetRelations(otherFaction);

                // reset modifier as it is not needed anymore for this treaty type
                tradeHateModifier = 0;
                break;
            case DiplomacyTreatyType.Alliance:
                throw new NotImplementedException();
                break;
            case DiplomacyTreatyType.AllianceBroken:
                throw new NotImplementedException();
                break;
            case DiplomacyTreatyType.Truce:
            case DiplomacyTreatyType.Nap:
            case DiplomacyTreatyType.None:

            default:
                break;
        }

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
}