using Diplomacy.Core.Helpers;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local

namespace Diplomacy.Core.Patches;

[HarmonyPatch(typeof(TIMissionCondition_DetainTarget))]
public class TIMissionCondition_DetainTargetPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TIMissionCondition_DetainTarget.CanTarget))]
    private static bool CanTargetPrefix(ref string __result, TICouncilorState councilor, TIGameState possibleTarget)
    {
        var faction = councilor.faction;
        var other = possibleTarget.ref_faction;
        if (faction == null || other == null || !faction.HasAllianceWith(other))
            return true;

        __result = nameof(TIMissionCondition_DetainTarget);
        return false;
    }
}