using System;
using System.Linq;
using Diplomacy.Core.Abstractions;
using Diplomacy.Core.Helpers;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Actions;
using PavonisInteractive.TerraInvicta.Debugging;
using UnityEngine;
using UnityModManagerNet;

namespace Diplomacy.Debugging;

public class TerminalDiplomacyCommands : IModClass
{
    private readonly TerminalController _terminalController;
    private bool _isActive = true;

    public TerminalDiplomacyCommands(TerminalController terminalController)
    {
        _terminalController = terminalController;
        RegisterCommands();
    }

    public void SetActive(bool isActive)
    {
        _isActive = isActive;
    }

    private void RegisterCommands()
    {
        _terminalController.RegisterCommand("startTrade", StartTrade,
            "params: ResistCouncil, DestroyCouncil, ExploitCouncil, SubmitCouncil, AppeaseCouncil, CooperateCouncil, EscapeCouncil, AlienCouncil");

        _terminalController.RegisterCommand("resetRelations", ResetRelations,
            "params: ResistCouncil, DestroyCouncil, ExploitCouncil, SubmitCouncil, AppeaseCouncil, CooperateCouncil, EscapeCouncil, AlienCouncil");
    }

    private void ResetRelations(string[] args)
    {
        if (!_isActive)
            return;

        if (args.Length is 1)
        {
            var faction = GameStateManager.FindByTemplate<TIFactionState>(args[0] != "" ? args[0] : "EscapeCouncil");

            if (faction == null)
            {
                Debug.Log($"Faction {args[0]} not found!");
                UnityModManager.Logger.Log($"Faction {args[0]} not found!");
                return;
            }

            // get player faction
            var playerFaction = GameControl.control.activePlayer;

            if (playerFaction == null)
            {
                Debug.Log("No player faction found!");
                UnityModManager.Logger.Log("No player faction found!");
                return;
            }

            faction.ResetRelations(playerFaction, true);

            Debug.Log($"Relations with {faction.displayName} reset for player faction {playerFaction.displayName}.");
            UnityModManager.Logger.Log($"Relations with {faction.displayName} reset for player faction {playerFaction.displayName}.");

            return;
        }

    }

    private void StartTrade(string[] args)
    {
        if (!_isActive)
            return;

        if (args.Length is 1)
        {
            var faction = GameStateManager.FindByTemplate<TIFactionState>(args[0] != "" ? args[0] : "EscapeCouncil");

            if (faction == null)
            {
                Debug.Log($"Faction {args[0]} not found!");
                UnityModManager.Logger.Log($"Faction {args[0]} not found!");
                return;
            }

            var targetCouncilor = faction.councilors.First();
            var councilor = GeneralControlsController.UISelectedAssetState as TICouncilorState;

            if (councilor == null)
            {
                Debug.Log("No councilor selected!");
                return;
            }

            faction.playerControl.StartAction(new AssignCouncilorToMission(
                councilor,
                TIFactionState.contactMission,
                targetCouncilor,
                0,
                true
            ));

            councilor.activeMission.ResolveMission();
            return;
        }

        Debug.Log("Start trade debug command called with wrong parameters ");
        UnityModManager.Logger.Log("Start trade debug command called with wrong parameters");
    }
}