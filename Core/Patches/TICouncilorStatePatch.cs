using System.Collections.Generic;
using Diplomacy.Core.Helpers;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;

// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local

namespace Diplomacy.Core.Patches;

[HarmonyPatch(typeof(TICouncilorState))]
public class TICouncilorStatePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TICouncilorState.GetStealableOrgs))]
    private static bool GetStealableOrgsPrefix(
        TICouncilorState targetingCouncilor,
        ref List<TIOrgState> __result,
        TICouncilorState __instance)
    {
        if (!targetingCouncilor.ref_faction.HasAllianceWith(__instance.ref_faction))
            return true;

        __result = new List<TIOrgState>();
        return false;
    }
}