using Diplomacy.Core.Helpers;
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
    [HarmonyPrefix]
    [HarmonyPatch(nameof(IntelFactionRelationsGridItemController.SetListItem))]
    private static bool SetListItemPrefix(
        TIFactionState primaryFactioninUI,
        TIFactionState judgedFaction,
        IntelFactionRelationsGridItemController __instance)
    {
        // Set instance variables like original method
        Traverse.Create(__instance).Field("primaryFactioninUI").SetValue(primaryFactioninUI);
        Traverse.Create(__instance).Field("judgedFaction").SetValue(judgedFaction);

        __instance.factionIcon.sprite = judgedFaction.factionIcon64UI;
        var currentDiplomacyLevel = primaryFactioninUI.CurrentDiplomacyLevelWith(judgedFaction);

        // Determine base attitude using patch logic
        string baseAttitude = currentDiplomacyLevel switch
        {
            DiplomacyLevel.War => Loc.T("UI.Intel.FactionWar"),
            DiplomacyLevel.Conflict or DiplomacyLevel.Enemy => Loc.T("UI.Intel.FactionHate10"),
            DiplomacyLevel.Allied => judgedFaction.IsAlienFaction
                                ? Loc.T("UI.Intel.FactionLove")
                                : Loc.T("TIDiplomacy.UI.Notifications.Allied"),
            _ => Loc.T("UI.Intel.FactionHate0"),
        };

        // Determine treaty status string and manage cancel button (incorporating original logic)
        StringBuilder treatyStatusBuilder = new StringBuilder();
        string treatyStatus = "";
        bool showCancelButton = false;
        string cancelTooltipKey = "";

        // Check Truce first (original logic order)
        if (primaryFactioninUI.HasTruce(judgedFaction, includeToBeDiscarded: false))
        {
            treatyStatus = treatyStatusBuilder.Append(", ").Append(Loc.T("UI.Notifications.Diplomacy.Truce")).ToString();
            showCancelButton = false; // Cannot cancel Truce from here
        }
        // Check NAP / Intel Sharing
        else if (primaryFactioninUI.HasNAP(judgedFaction, includeToBeDiscarded: false))
        {
            treatyStatusBuilder.Append(", ").Append(Loc.T("UI.Notifications.Diplomacy.NAP"));
            // Check Intel Sharing within NAP (original logic)
            if (primaryFactioninUI.intelSharingFactions.Contains(judgedFaction))
            {
                treatyStatusBuilder.Append(", ").Append(Loc.T("UI.Notifications.Diplomacy.IntelSharing"));
                cancelTooltipKey = "UI.Intel.Faction.Relations.CancelTreaty_Intel";
                // Show cancel button only if the player is the one whose relations are being judged (original logic)
                showCancelButton = GameControl.control.activePlayer == judgedFaction;
            }
            else // Just NAP
            {
                cancelTooltipKey = "UI.Intel.Faction.Relations.CancelTreaty_NAP";
                // Show cancel button only if player is judged faction AND not permanent ally (original logic)
                showCancelButton = GameControl.control.activePlayer == judgedFaction && currentDiplomacyLevel != DiplomacyLevel.Allied;
            }
            treatyStatus = treatyStatusBuilder.ToString();
        }
        else // No Truce or NAP
        {
            showCancelButton = false;
        }

        // Set Cancel Button state
        __instance.cancelTreatyButtonObject.SetActive(showCancelButton);
        if (showCancelButton && !string.IsNullOrEmpty(cancelTooltipKey))
        {
            // Use a local variable capture for the delegate
            string tooltipText = Loc.T(cancelTooltipKey);
            __instance.cancelTreatyButtonTip.SetDelegate("BodyText", () => tooltipText);
        }

        // Combine base attitude and treaty status
        string finalAttitudeText = baseAttitude + treatyStatus;

        // Add debug hate value (original logic)
        if (TIGlobalConfig.globalConfig.debug_showHateValues)
        {
            finalAttitudeText += $" ({primaryFactioninUI.GetFactionHate(judgedFaction):F0} hate)";
        }

        __instance.attitudeDescription.SetText(finalAttitudeText);

        return false; // Prevent original method execution
    }
}