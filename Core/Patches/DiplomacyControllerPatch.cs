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
                    UnlockTreatyOption(DiplomacyTreatyType.Truce, __instance);

                break;
            case DiplomacyLevel.Enemy when maxDiplomacyLevel > DiplomacyLevel.Conflict:
            case DiplomacyLevel.Conflict when maxDiplomacyLevel > DiplomacyLevel.Conflict:
                if (!hasBrokenAlliance)
                {
                    var hasReset = ModState.IsTreatyValid(playerFaction, otherFaction, DiplomacyTreatyType.ResetRelation);
                    if (!hasReset)
                        UnlockTreatyOption(DiplomacyTreatyType.ResetRelation, __instance);
                }

                break;
            case DiplomacyLevel.Normal when maxDiplomacyLevel > DiplomacyLevel.Normal && !hasBrokenAlliance:

                if (!ModState.IsTreatyValid(playerFaction, otherFaction, DiplomacyTreatyType.Nap))
                    UnlockTreatyOption(DiplomacyTreatyType.Nap, __instance);

                break;
            // firendly will only be set if there is nap already or intel is shared
            // so just check if already intel is shared if not, offer it
            case DiplomacyLevel.Friendly when maxDiplomacyLevel >= DiplomacyLevel.Friendly
                 && !hasBrokenAlliance
                 && !ModState.IsTreatyValid(playerFaction, otherFaction, DiplomacyTreatyType.Intel):


                UnlockTreatyOption(DiplomacyTreatyType.Intel, __instance);

                break;
            case DiplomacyLevel.Friendly when maxDiplomacyLevel == DiplomacyLevel.Allied && !hasBrokenAlliance:

                // for now offer alliance only if there is already intel sharing 
                if (playerFaction.IsIntelSharedBy(otherFaction) || otherFaction.IsIntelSharedBy(playerFaction))
                    UnlockTreatyOption(DiplomacyTreatyType.Alliance, __instance);

                break;
            case DiplomacyLevel.Allied:
                UnlockTreatyOption(DiplomacyTreatyType.AllianceBroken, __instance);
                break;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DiplomacyController.EvaluateTrade))]
    private static void EvaluateTradePrefix(DiplomacyController __instance)
    {
        // here we know a treaty was selected, set it so it can be provessed in TIFactionState.ProcessTrade (also patched)
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

    private static void UnlockTreatyOption(DiplomacyTreatyType treatyType, DiplomacyController diplomacyController)
    {
        var text = "";
        var description = "";
        var setTreatyType = TradeOffer.TreatyType.None;
        _possibleTreaty = treatyType;

        switch (treatyType)
        {
            case DiplomacyTreatyType.Truce:
                text = Loc.T("UI.Notifications.Diplomacy.Truce");
                description = Loc.T("UI.Notifications.Diplomacy.TruceDesc", 12);
                setTreatyType = TradeOffer.TreatyType.Truce;
                break;
            case DiplomacyTreatyType.Nap:
                text = Loc.T("UI.Notifications.Diplomacy.NAP");
                description = Loc.T("UI.Notifications.Diplomacy.NAPDesc");
                setTreatyType = TradeOffer.TreatyType.NAP;
                break;
            case DiplomacyTreatyType.ResetRelation:
                text = Loc.T("TIDiplomacy.UI.Notifications.ResetRelations");
                description = Loc.T("TIDiplomacy.UI.Notifications.ResetRelationsDescription");
                break;
            case DiplomacyTreatyType.Intel:
                text = Loc.T("UI.Notifications.Diplomacy.IntelSharing");
                description = Loc.T("UI.Notifications.Diplomacy.IntelSharingDesc");
                setTreatyType = TradeOffer.TreatyType.Intel;
                break;
            case DiplomacyTreatyType.Alliance:
                text = Loc.T("TIDiplomacy.UI.Notifications.Alliance");
                description = Loc.T("TIDiplomacy.UI.Notifications.AllianceDescription");
                break;
            case DiplomacyTreatyType.AllianceBroken:
                text = Loc.T("TIDiplomacy.UI.Notifications.BreakAlliance");
                description = Loc.T("TIDiplomacy.UI.Notifications.BreakAllianceDescription");
                break;
        }

        diplomacyController.aiBankTreatyItem.gameObject.SetActive(true);
        diplomacyController.aiBankTreatyItem.quantityText.text = text;
        diplomacyController.aiBankTreatyItem.tooltipTrigger.SetDelegate("BodyText", () => description);

        diplomacyController.aiTableTreatyItem.itemDescription.text = text;
        diplomacyController.aiTableTreatyItem.tooltipTrigger.SetDelegate("BodyText", () => description);
        diplomacyController.aiTableTreatyItem.treaty = setTreatyType;

        diplomacyController.playerBankTreatyItem.gameObject.SetActive(true);
        diplomacyController.playerBankTreatyItem.quantityText.text = text;
        diplomacyController.playerBankTreatyItem.tooltipTrigger.SetDelegate("BodyText", () => description);

        diplomacyController.playerTableTreatyItem.itemDescription.text = text;
        diplomacyController.playerTableTreatyItem.tooltipTrigger.SetDelegate("BodyText", () => description);
        diplomacyController.playerTableTreatyItem.treaty = setTreatyType;
    }
}