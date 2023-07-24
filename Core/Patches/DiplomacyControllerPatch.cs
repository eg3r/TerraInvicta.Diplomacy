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
        var maxDipLvl = playerFaction.MaxDiplomacyLevelWith(otherFaction);
        var currentDipLevel = playerFaction.CurrentDiplomacyLevelWith(otherFaction);

        switch (currentDipLevel)
        {
            case DiplomacyLevel.War when maxDipLvl > DiplomacyLevel.War:
                if (!ModState.IsTreatyValid(playerFaction, otherFaction, DiplomacyTreatyType.Truce))
                    OfferTreatyOption(DiplomacyTreatyType.Truce, __instance);
                break;
            case DiplomacyLevel.Enemy when maxDipLvl > DiplomacyLevel.Enemy:
            case DiplomacyLevel.Conflict when maxDipLvl > DiplomacyLevel.Conflict:
            case DiplomacyLevel.Normal when maxDipLvl > DiplomacyLevel.Normal:
                if (!ModState.IsTreatyValid(playerFaction, otherFaction, DiplomacyTreatyType.ResetRelation))
                    OfferTreatyOption(DiplomacyTreatyType.ResetRelation, __instance);
                break;
            case DiplomacyLevel.Friendly when maxDipLvl > DiplomacyLevel.Conflict:
                if (!ModState.IsTreatyValid(playerFaction, otherFaction, DiplomacyTreatyType.Nap))
                    OfferTreatyOption(DiplomacyTreatyType.Nap, __instance);
                break;
            case DiplomacyLevel.Allied:
            default:
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
        if (ModState.CurrentTreatyType == DiplomacyTreatyType.ResetRelation)
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
                diplomacyController.aiTableTreatyItem.TreatyTruce = true;
                diplomacyController.aiTableTreatyItem.TreatyNAP = false;
                break;
            case DiplomacyTreatyType.Nap:
                text = Loc.T("UI.Notifications.Diplomacy.NAP");
                description = Loc.T("UI.Notifications.Diplomacy.NAPDesc");
                diplomacyController.aiTableTreatyItem.TreatyTruce = false;
                diplomacyController.aiTableTreatyItem.TreatyNAP = true;
                break;
            case DiplomacyTreatyType.ResetRelation:
                text = Loc.T("TIDiplomacy.UI.Notifications.ResetRelations");
                description = Loc.T("TIDiplomacy.UI.Notifications.ResetRelationsDescription");
                diplomacyController.aiTableTreatyItem.TreatyTruce = false;
                diplomacyController.aiTableTreatyItem.TreatyNAP = false;
                break;
        }

        diplomacyController.aiBankTreatyItem.gameObject.SetActive(true);
        diplomacyController.aiBankTreatyItem.quantityText.text = text;
        diplomacyController.aiTableTreatyItem.itemDescription.text = text;
        diplomacyController.aiBankTreatyItem.tooltipTrigger.SetDelegate("BodyText", () => description);
        diplomacyController.aiTableTreatyItem.tooltipTrigger.SetDelegate("BodyText", () => description);
    }
}