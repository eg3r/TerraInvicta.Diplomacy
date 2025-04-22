using System;
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
    public const int MinDaysBetweenTreaties = 90; // TODO: move into settings

    // TODO: move to settings, ATTENTION NEEDS PATCHING TO SUPPORT OFFICIAL: TIMissionPhaseState.StartNewMissionPhase
    public const int NumberAutosaves = 3;

    private static InnerState _innerState = new();

    private static List<DiplomacyTreaty> Treaties => _innerState.Treaties;

    public static DiplomacyTreatyType CurrentTreatyType { get; set; } = DiplomacyTreatyType.None;

    private static string CurrentLoadedSave { get; set; }


    private static bool IsTreatyValid(DiplomacyTreaty treaty)
    {
        if (treaty == null)
            return false;

        var isValid = treaty.IsValid;
        if (!isValid)
            RemoveTreaty(treaty);

        return isValid;
    }

    private static DiplomacyTreaty FindTreaty(TIFactionState faction, TIFactionState other, DiplomacyTreatyType type)
    {
        return Treaties.FirstOrDefault(t => ((t.GetInitiator() == faction && t.GetOther() == other)
                                             || (t.GetInitiator() == other && t.GetOther() == faction))
                                            && t.TreatyType == type);
    }

    private static List<DiplomacyTreaty> FindTreaties(
        TIFactionState faction,
        DiplomacyTreatyType type,
        bool onlyValid = false)
    {
        return Treaties.FindAll(t =>
            (t.GetInitiator() == faction || t.GetOther() == faction)
            && t.TreatyType == type
            && (!onlyValid || t.IsValid)
        );
    }

    public static void AddTreaty(DiplomacyTreaty treaty)
    {
        Treaties.Add(treaty);
    }

    public static bool IsTreatyValid(TIFactionState faction, TIFactionState other, DiplomacyTreatyType type)
    {
        return IsTreatyValid(FindTreaty(faction, other, type));
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

    public static bool Save(string saveName)
    {
        return SaveSystem.Save(_innerState, saveName);
    }

    private static void RemoveTreaty(DiplomacyTreaty treaty)
    {
        Treaties.Remove(treaty);
    }

    public static void RemoveAllianceTreaty(TIFactionState faction, TIFactionState other)
    {
        var treaty = FindTreaty(faction, other, DiplomacyTreatyType.Alliance);
        if (treaty != null)
            RemoveTreaty(treaty);
    }

    public static void RemoveNapTreaty(TIFactionState faction, TIFactionState other)
    {
        var treaty = FindTreaty(faction, other, DiplomacyTreatyType.Nap);
        if (treaty != null)
            RemoveTreaty(treaty);
    }

    public static void RemoveTruce(TIFactionState faction, TIFactionState other)
    {
        var treaty = FindTreaty(faction, other, DiplomacyTreatyType.Truce);
        if (treaty != null)
            RemoveTreaty(treaty);
    }

    public static DiplomacyTreaty GetLatestTreatyBetween(TIFactionState faction, TIFactionState other)
    {
        return (from t in Treaties
                where ((t.GetInitiator() == faction && t.GetOther() == other) ||
                       (t.GetInitiator() == other && t.GetOther() == faction))
                      && t.IsValid
                orderby t.TreatyGameDay descending
                select t).FirstOrDefault();
    }

    public static List<TIFactionState> GetAlliancesFor(TIFactionState faction)
    {
        return FindTreaties(faction, DiplomacyTreatyType.Alliance).Select(
            treaty => treaty.GetOther() != faction
                ? treaty.GetOther()
                : treaty.GetInitiator()).ToList();
    }

    private static List<ISaveFix> GetSaveFixes(string fromVersion, string toVersion) => [.. AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(ISaveFix).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<ISaveFix>()
            .Where(s => s.FromVersion == fromVersion && s.ToVersion == toVersion)];

    public static void FixSave(GameControl gameControl)
    {
        var currentSaveVersion = new InnerState().SaveVersion;
        if (currentSaveVersion == _innerState.SaveVersion)
            return;

        GetSaveFixes(_innerState.SaveVersion, currentSaveVersion).ForEach(fix => fix.Fix(gameControl, Treaties));
        _innerState.SaveVersion = currentSaveVersion;
    }
}