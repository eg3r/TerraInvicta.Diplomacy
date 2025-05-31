using System.IO;
using HarmonyLib;

using PavonisInteractive.TerraInvicta;

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