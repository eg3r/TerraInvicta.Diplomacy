using Diplomacy.Core.Helpers;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;

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
        TIFactionState judgingFaction,
        TIFactionState judgedFaction,
        IntelFactionRelationsGridItemController __instance)
    {
        __instance.factionIcon.sprite = judgedFaction.factionIcon64UI;

        string strAttitude;
        var currentDiplomacyLevel = judgingFaction.CurrentDiplomacyLevelWith(judgedFaction);
        switch (currentDiplomacyLevel)
        {
            case DiplomacyLevel.War:
                strAttitude = Loc.T("UI.Intel.FactionWar");
                break;
            case DiplomacyLevel.Conflict:
            case DiplomacyLevel.Enemy:
                strAttitude = Loc.T("UI.Intel.FactionHate10");
                break;
            case DiplomacyLevel.Allied:
                strAttitude = judgedFaction.IsAlienFaction
                    ? Loc.T("UI.Intel.FactionLove")
                    : Loc.T("TIDiplomacy.UI.Notifications.Allied");
                break;
            case DiplomacyLevel.Normal:
            case DiplomacyLevel.Friendly:
            default:
                strAttitude = Loc.T("UI.Intel.FactionHate0");
                break;
        }

        if (currentDiplomacyLevel is not DiplomacyLevel.Allied and not DiplomacyLevel.War)
        {
            if (judgingFaction.HasNap(judgedFaction))
                strAttitude += ", " + Loc.T("UI.Notifications.Diplomacy.NAP");
            else if (judgingFaction.HasTruce(judgedFaction))
                strAttitude += ", " + Loc.T("UI.Notifications.Diplomacy.Truce");
        }

        __instance.attitudeDescription.SetText(strAttitude);
        return false;
    }
}