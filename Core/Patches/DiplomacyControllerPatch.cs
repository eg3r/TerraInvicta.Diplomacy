using Diplomacy.Core.Helpers;
using Diplomacy.Core.Treaty;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Global

namespace Diplomacy.Core.Patches;

[HarmonyPatch(typeof(DiplomacyController))]
public class DiplomacyControllerPatch
{
    private static DiplomacyTreatyType _possibleTreaty;

    [HarmonyPostfix]
    [HarmonyPatch("LoadBankValues")]
    private static void Postfix(DiplomacyController __instance, bool ___isThisAnAIOffer)
    {
        // TODO: Ai is not handled right now
        if (___isThisAnAIOffer)
            return;

        if (__instance.aiTableTreatyItem.gameObject.activeSelf)
            return;

        var playerFaction = GameControl.control.activePlayer;
        var otherFaction = __instance.tradingFaction;

        // apply cooldown of treaties 
        var latestTreaty = playerFaction.GetLatestActiveTreaty(otherFaction);
        if (latestTreaty != null &&
            TITimeState.CampaignDuration_days() - latestTreaty.TreatyGameDay < ModState.MinDaysBetweenTreaties)
            return;

        var hasBrokenAlliance = playerFaction.HasBrokenAlliance(otherFaction);
        var maxDiplomacyLevel = playerFaction.MaxDiplomacyLevelWith(otherFaction);
        var currentDiplomacyLevel = playerFaction.CurrentDiplomacyLevelWith(otherFaction);

        switch (currentDiplomacyLevel)
        {
            case DiplomacyLevel.War when maxDiplomacyLevel > DiplomacyLevel.War:
                if (!ModState.IsTreatyValid(playerFaction, otherFaction, DiplomacyTreatyType.Truce))
                    OfferTreatyOption(DiplomacyTreatyType.Truce, __instance);
                break;
            case DiplomacyLevel.Enemy when maxDiplomacyLevel > DiplomacyLevel.Conflict:
            case DiplomacyLevel.Conflict when maxDiplomacyLevel > DiplomacyLevel.Conflict:
                var hasReset = ModState.IsTreatyValid(playerFaction, otherFaction, DiplomacyTreatyType.ResetRelation);
                if (!hasReset && !hasBrokenAlliance)
                    OfferTreatyOption(DiplomacyTreatyType.ResetRelation, __instance);
                break;
            case DiplomacyLevel.Normal when maxDiplomacyLevel > DiplomacyLevel.Normal:
                if (!ModState.IsTreatyValid(playerFaction, otherFaction, DiplomacyTreatyType.Nap) && !hasBrokenAlliance)
                    OfferTreatyOption(DiplomacyTreatyType.Nap, __instance);
                // TODO: investigate if offering IntelSharing is correct here
                else if (!ModState.IsTreatyValid(playerFaction, otherFaction, DiplomacyTreatyType.Intel) && !hasBrokenAlliance)
                    OfferTreatyOption(DiplomacyTreatyType.Intel, __instance);
                break;
            case DiplomacyLevel.Friendly when maxDiplomacyLevel > DiplomacyLevel.Friendly:
                if (!hasBrokenAlliance)
                    OfferTreatyOption(DiplomacyTreatyType.Alliance, __instance);
                break;
            case DiplomacyLevel.Allied:
                OfferTreatyOption(DiplomacyTreatyType.AllianceBroken, __instance);
                break;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DiplomacyController.EvaluateTrade))]
    private static void EvaluateTradePrefix(DiplomacyController __instance)
    {
        // here we know a treaty was selected
        ModState.CurrentTreatyType = __instance.aiTableTreatyItem.gameObject.activeSelf
            ? _possibleTreaty
            : DiplomacyTreatyType.None;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(DiplomacyController.EvaluateTrade))]
    private static void EvaluateTradePostfix(DiplomacyController __instance)
    {
        // get rid of the normal "improve relations" item when using ResetRelation
        if (ModState.CurrentTreatyType
            is DiplomacyTreatyType.ResetRelation
            or DiplomacyTreatyType.AllianceBroken
            or DiplomacyTreatyType.Alliance
            or DiplomacyTreatyType.Intel)
            __instance.aiTableHateReductionItem.gameObject.SetActive(false);
    }

    private static void OfferTreatyOption(DiplomacyTreatyType treatyType, DiplomacyController diplomacyController)
    {
        var text = "";
        var description = "";
        _possibleTreaty = treatyType;

        switch (treatyType)
        {
            case DiplomacyTreatyType.Truce:
                text = Loc.T("UI.Notifications.Diplomacy.Truce");
                description = Loc.T("UI.Notifications.Diplomacy.TruceDesc", 12);
                diplomacyController.aiTableTreatyItem.treaty = TradeOffer.TreatyType.Truce;
                break;
            case DiplomacyTreatyType.Nap:
                text = Loc.T("UI.Notifications.Diplomacy.NAP");
                description = Loc.T("UI.Notifications.Diplomacy.NAPDesc");
                diplomacyController.aiTableTreatyItem.treaty = TradeOffer.TreatyType.NAP;
                break;
            case DiplomacyTreatyType.ResetRelation:
                text = Loc.T("TIDiplomacy.UI.Notifications.ResetRelations");
                description = Loc.T("TIDiplomacy.UI.Notifications.ResetRelationsDescription");
                diplomacyController.aiTableTreatyItem.treaty = TradeOffer.TreatyType.None;
                break;
            case DiplomacyTreatyType.Intel:
                text = Loc.T("TIDiplomacy.UI.Notifications.Intel");
                description = Loc.T("TIDiplomacy.UI.Notifications.Intel");
                diplomacyController.aiTableTreatyItem.treaty = TradeOffer.TreatyType.Intel;
                break;
            case DiplomacyTreatyType.Alliance:
                text = Loc.T("TIDiplomacy.UI.Notifications.Alliance");
                description = Loc.T("TIDiplomacy.UI.Notifications.AllianceDescription");
                diplomacyController.aiTableTreatyItem.treaty = TradeOffer.TreatyType.None;
                break;
            case DiplomacyTreatyType.AllianceBroken:
                text = Loc.T("TIDiplomacy.UI.Notifications.BreakAlliance");
                description = Loc.T("TIDiplomacy.UI.Notifications.BreakAllianceDescription");
                diplomacyController.aiTableTreatyItem.treaty = TradeOffer.TreatyType.None;
                break;
        }

        diplomacyController.aiBankTreatyItem.gameObject.SetActive(true);
        diplomacyController.aiBankTreatyItem.quantityText.text = text;
        diplomacyController.aiTableTreatyItem.itemDescription.text = text;
        diplomacyController.aiBankTreatyItem.tooltipTrigger.SetDelegate("BodyText", () => description);
        diplomacyController.aiTableTreatyItem.tooltipTrigger.SetDelegate("BodyText", () => description);
    }
}