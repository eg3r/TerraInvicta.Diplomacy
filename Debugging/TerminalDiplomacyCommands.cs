using System.Linq;
using Diplomacy.Core.Abstractions;
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

    private void RegisterCommands()
    {
        _terminalController.RegisterCommand("startTrade", StartTrade,
            "params: ResistCouncil, DestroyCouncil, ExploitCouncil, SubmitCouncil, AppeaseCouncil, CooperateCouncil, EscapeCouncil, AlienCouncil");
    }

    private void StartTrade(string[] args)
    {
        if (!_isActive)
            return;

        if (args.Length is 1)
        {
            var faction = GameStateManager.FindByTemplate<TIFactionState>(args[0] != "" ? args[0] : "EscapeCouncil");

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

            // TIPromptQueueState.AddPromptStatic(faction, councilor, mission, "PromptFactionContactMakeOffer", 0);
            return;
        }

        Debug.Log("Start trade debug command called with wrong parameters ");
        UnityModManager.Logger.Log("Start trade debug command called with wrong parameters");
    }

    public void SetActive(bool isActive)
    {
        _isActive = isActive;
    }
}