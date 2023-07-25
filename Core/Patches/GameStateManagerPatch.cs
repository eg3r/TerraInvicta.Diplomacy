using System;
using System.IO;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local

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
            ModState.Save(saveName);
        }
        catch (Exception)
        {
            // ignored
        }

        return true;
    }
}