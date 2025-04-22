using Diplomacy.Core.Helpers;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;

namespace Diplomacy.Core.Patches;

[HarmonyPatch(typeof(IntelFactionRelationsGridItemController))]
public class IntelFactionRelationsGridItemControllerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(IntelFactionRelationsGridItemController.SetListItem))]
    private static void SetListItemPrefix(
        TIFactionState primaryFactioninUI,
        TIFactionState judgedFaction,
        IntelFactionRelationsGridItemController __instance)
    {
        // for now only difference is when we are allied, aliens can't be allied as per this mod mechanics for now
        var currentDiplomacyLevel = primaryFactioninUI.CurrentDiplomacyLevelWith(judgedFaction);
        if (currentDiplomacyLevel == DiplomacyLevel.Allied && !judgedFaction.IsAlienFaction)
        {
            var treatyText = Loc.T("TIDiplomacy.UI.Notifications.Alliance"); // Alliance
            __instance.cancelTreatyButtonTip.SetDelegate("BodyText", () => Loc.T("TIDiplomacy.UI.Notifications.BreakAllianceNotPossible"));
            __instance.cancelTreatyButtonObject.SetActive(false);

            if (TIGlobalConfig.globalConfig.debug_showHateValues)
                treatyText += " (" + primaryFactioninUI.GetFactionHate(judgedFaction) + " hate)";

            // Combine base attitude and treaty status
            __instance.attitudeDescription.SetText(treatyText);
        }
    }
}