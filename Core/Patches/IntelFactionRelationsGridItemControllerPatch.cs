﻿using Diplomacy.Core.Helpers;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using UnityModManagerNet;
using System.Text; // Added for StringBuilder

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Global
// ReSharper disable InconsistentNaming

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

    // This is not needed for now, as a ref for future development:
    // [HarmonyPrefix]
    // [HarmonyPatch(nameof(IntelFactionRelationsGridItemController.OnClickCancelTreaty))]
    // private static bool OnClickCancelTreaty(IntelFactionRelationsGridItemController __instance)
    // {
    //     var accesorTraverse = Traverse.Create(__instance);
    //     var primaryFactioninUI = accesorTraverse.Field("primaryFactioninUI").GetValue<TIFactionState>();
    //     var judgedFaction = accesorTraverse.Field("judgedFaction").GetValue<TIFactionState>();
    //     var currentDiplomacyLevel = primaryFactioninUI.CurrentDiplomacyLevelWith(judgedFaction);
    //     return currentDiplomacyLevel != DiplomacyLevel.Allied;
    // }

    // private static string GetAttitudeText(DiplomacyLevel diplomacyLevel, bool isAlienFaction)
    // {
    //     return diplomacyLevel switch
    //     {
    //         DiplomacyLevel.War => Loc.T("UI.Intel.FactionWar"),                                 // War
    //         DiplomacyLevel.Conflict or DiplomacyLevel.Enemy => Loc.T("UI.Intel.FactionHate10"), // In Conflict
    //         DiplomacyLevel.Allied => isAlienFaction ? Loc.T("UI.Intel.FactionLove")             // Support
    //                             : Loc.T("TIDiplomacy.UI.Notifications.Allied"),                 // Allied
    //         _ => Loc.T("UI.Intel.FactionHate0"),                                                // Tolerance
    //     };
    // }
}