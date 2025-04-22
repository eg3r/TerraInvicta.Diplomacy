using System;
using System.IO;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;

namespace Diplomacy.Core.Patches;

[HarmonyPatch(typeof(GameStateManager))]
public class GameStateManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameStateManager.LoadAllGameStates))]
    private static void LoadAllGameStatesPostfix(string filepath)
    {
        try
        {
            var saveName = Path.GetFileNameWithoutExtension(filepath);
            ModState.Load(saveName);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameStateManager.SaveAllGameStates))]
    private static bool SaveAllGameStatesPostfix(bool bWasSaved, string filepath)
    {
        if (!bWasSaved)
            return false;

        try // make sure not to fail upwards chain if saved main save
        {
            var saveName = Path.GetFileNameWithoutExtension(filepath);

            // Handle autosave rotation
            if (saveName.Equals(Path.GetFileNameWithoutExtension(StartMenuController.autoSaveFilepath),
                    StringComparison.OrdinalIgnoreCase))
                for (var i = ModState.NumberAutosaves; i > 1; i--)
                {
                    var autosavePath = saveName + i;
                    var prevAutosave = saveName + (i == 2 ? "" : i - 1);
                    SaveSystem.MoveSave(prevAutosave, autosavePath);
                }

            _ = ModState.Save(saveName);
        }
        catch (Exception)
        {
            // ignored
        }

        return true;
    }
}