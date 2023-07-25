using System.IO;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local

namespace Diplomacy.Core.Patches;

[HarmonyPatch(typeof(LoadMenuController))]
public class LoadMenuControllerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(LoadMenuController.DeleteSaveFile))]
    private static void DeleteSaveFilePostfix(LoadMenuController __instance)
    {
        var saveName = Path.GetFileNameWithoutExtension(__instance.saveList.selectedButton.saveInfo.path);
        SaveSystem.DeleteSave(saveName);
    }
}