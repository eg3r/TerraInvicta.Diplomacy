using System.Collections.Generic;
using System.Linq;
using Diplomacy.Core.Treaty;
using PavonisInteractive.TerraInvicta;

namespace Diplomacy.Core;

public static class ModState
{
    public const int ResetRelationsTreatyValidDays = 365 * 2; // TODO: move into settings
    public const int NapTreatyRepeatDays = 365 * 5; // TODO: move into settings
    public const int TruceTreatyRepeatDays = 365 * 2; // TODO: move into settings
    public const int AllianceBrokenValidDays = 365 * 2; // TODO: move into settings

    private static InnerState _innerState = new();

    private static List<DiplomacyTreaty> Treaties => _innerState.Treaties;

    public static DiplomacyTreatyType CurrentTreatyType { get; set; } = DiplomacyTreatyType.None;

    private static string CurrentLoadedSave { get; set; }

    public static void AddTreaty(DiplomacyTreaty treaty)
    {
        Treaties.Add(treaty);
    }

    public static bool IsTreatyValid(TIFactionState faction, TIFactionState other, DiplomacyTreatyType type)
    {
        var treaty = Treaties.FirstOrDefault(t =>
            ((t.GetInitiator() == faction && t.GetOther() == other)
             || (t.GetInitiator() == other && t.GetOther() == faction))
            && t.TreatyType == type);

        if (treaty == null)
            return false;

        // if found and invalid, remove it
        var isValid = treaty.IsValid;
        if (!isValid)
            RemoveTreaty(treaty);

        return isValid;
    }

    public static void Reset()
    {
        CurrentTreatyType = DiplomacyTreatyType.None;
        Treaties.Clear();
        _innerState = new InnerState();
    }

    public static void Load(string saveName = "")
    {
        if (string.IsNullOrEmpty(saveName))
            saveName = CurrentLoadedSave;

        if (string.IsNullOrEmpty(saveName) || !SaveSystem.SaveExists(saveName))
            return;

        _innerState = SaveSystem.Load<InnerState>(saveName);
        CurrentLoadedSave = saveName;
    }

    public static void Save(string saveName)
    {
        SaveSystem.Save(_innerState, saveName);
    }

    private static void RemoveTreaty(DiplomacyTreaty treaty)
    {
        Treaties.Remove(treaty);
    }
}