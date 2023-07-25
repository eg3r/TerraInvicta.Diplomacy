using System.Collections.Generic;
using System.Linq;
using PavonisInteractive.TerraInvicta;

namespace Diplomacy.Core;

public static class ModState
{
    public const int ResetRelationsTreatyValidDays = 360; // TODO: move into settings
    public const int NapTreatyValidDays = 360; // TODO: move into settings
    public const int TruceTreatyValidDays = 360; // TODO: move into settings

    private static InnerState _innerState = new();

    private static List<DiplomacyTreaty> Treaties => _innerState.Treaties;

    public static DiplomacyTreatyType CurrentTreatyType { get; set; } = DiplomacyTreatyType.None;

    public static string CurrentLoadedSave { get; private set; }

    public static void AddTreaty(DiplomacyTreaty treaty)
    {
        Treaties.Add(treaty);
    }

    public static bool IsTreatyValid(TIFactionState faction, TIFactionState other, DiplomacyTreatyType type)
    {
        var treaty = Treaties.FirstOrDefault(t =>
            t.GetInitiator() == faction && t.GetOther() == other && t.TreatyType == type);
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
    }

    public static void Load(string saveName)
    {
        CurrentLoadedSave = saveName;
        if (SaveSystem.SaveExists(saveName))
            _innerState = SaveSystem.Load<InnerState>(saveName);
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