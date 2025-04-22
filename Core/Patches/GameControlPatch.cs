using HarmonyLib;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Global

namespace Diplomacy.Core.Patches;

/**
  * Used to patch game/gamestate to fix possible save loading compatibility issues.
  **/
[HarmonyPatch(typeof(GameControl))]
public class GameControlPatch
{
  [HarmonyPostfix]
  [HarmonyPatch(nameof(GameControl.CompleteInit))]
  private static void CompleteInitPostfix(bool loadingSave, GameControl __instance)
  {
    if (loadingSave)
      ModState.FixSave(__instance);
  }
}
