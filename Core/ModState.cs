using System.Collections.Generic;
using System.Linq;
using PavonisInteractive.TerraInvicta;

namespace Diplomacy.Core;

public static class ModState
{
    // TODO: move into settings
    public const int ResetRelationsTreatyValidDays = 360;
    public const int NapTreatyValidDays = 360;
    public const int TruceTreatyValidDays = 360;
    private static readonly List<DiplomacyTreaty> Treaties = new();

    public static DiplomacyTreatyType CurrentTreatyType { get; set; } = DiplomacyTreatyType.None;

    public static void AddTreaty(DiplomacyTreaty treaty)
    {
        Treaties.Add(treaty);
    }

    public static bool IsTreatyValid(TIFactionState faction, TIFactionState other, DiplomacyTreatyType type)
    {
        var treaty = Treaties.FirstOrDefault(t => t.Initiator == faction && t.Other == other && t.TreatyType == type);
        if (treaty == null)
            return false;

        // if found and invalid, remove it
        if (!treaty.IsValid)
            RemoveTreaty(treaty);

        return treaty.IsValid;
    }

    public static void Reset()
    {
        CurrentTreatyType = DiplomacyTreatyType.None;
        Treaties.Clear();
    }

    public static void Load()
    {
        // TODO: load state from some persistant source
    }

    private static void RemoveTreaty(DiplomacyTreaty treaty)
    {
        Treaties.Remove(treaty);
    }
}