using System.IO;
using HarmonyLib;

namespace Diplomacy.Core.Patches;

[HarmonyPatch(typeof(SaveMenuController))]
public class SaveMenuControllerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(SaveMenuController.DeleteSaveFile))]
    private static void DeleteSaveFilePostfix(SaveMenuController __instance)
    {
        var saveName = Path.GetFileNameWithoutExtension(__instance.saveList.selectedButton.saveInfo.path);
        SaveSystem.DeleteSave(saveName);
    }
}