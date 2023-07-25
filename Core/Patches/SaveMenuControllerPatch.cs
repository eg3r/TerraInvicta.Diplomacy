using System.IO;
using HarmonyLib;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local

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