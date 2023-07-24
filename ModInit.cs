using System.Collections.Generic;
using System.Reflection;
using Diplomacy.Core;
using Diplomacy.Core.Abstractions;
using Diplomacy.Debugging;
using HarmonyLib;
using PavonisInteractive.TerraInvicta.Debugging;
using PavonisInteractive.TerraInvicta.Systems.Bootstrap;
using UnityModManagerNet;

namespace Diplomacy
{
    // ReSharper disable once UnusedType.Global
    public class ModInit
    {
        private static bool _isActive;
        private static Harmony _harmonyRef;
        private static readonly List<IModClass> ModClasses = new();

        // ReSharper disable once UnusedMember.Local
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnToggle = OnToggle;
            _harmonyRef = new Harmony(modEntry.Info.Id);
            SetupMod();
            return true;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            _isActive = value;

            if (_isActive)
                ActivateMod();
            else
                DeactivateMod();

            return true;
        }

        private static void ActivateMod()
        {
            ModClasses.ForEach(m => m.SetActive());
            _harmonyRef.PatchAll(Assembly.GetExecutingAssembly());
            ModState.Load();
            UnityModManager.Logger.Log("Diplomacy Mod Activated.");
        }

        private static void DeactivateMod()
        {
            ModClasses.ForEach(m => m.SetActive(false));
            _harmonyRef.UnpatchAll(_harmonyRef.Id);
            ModState.Reset();
            UnityModManager.Logger.Log("Diplomacy Mod Deactivated.");
        }

        private static void SetupMod()
        {
            ModClasses.Add(new TerminalDiplomacyCommands(GlobalInstaller.container.Resolve<Terminal>().controller));
        }
    }
}