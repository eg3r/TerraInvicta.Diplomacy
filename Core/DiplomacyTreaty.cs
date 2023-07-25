using Newtonsoft.Json;
using PavonisInteractive.TerraInvicta;

// ReSharper disable SuggestBaseTypeForParameterInConstructor

namespace Diplomacy.Core;

public class DiplomacyTreaty
{
    [JsonProperty] private readonly int _treatyGameDay;

    [JsonConstructor]
    public DiplomacyTreaty(int treatyGameDay, int initiatorID, int otherID, DiplomacyTreatyType treatyType)
    {
        InitiatorID = initiatorID;
        OtherID = otherID;
        TreatyType = treatyType;
        _treatyGameDay = treatyGameDay;
    }

    public DiplomacyTreaty(TIFactionState initiator, TIFactionState other, DiplomacyTreatyType treatyType)
    {
        TreatyType = treatyType;
        InitiatorID = (int)initiator.ID;
        OtherID = (int)other.ID;
        _treatyGameDay = TITimeState.CampaignDuration_days();
    }

    [JsonProperty] public int InitiatorID { get; }
    [JsonProperty] public int OtherID { get; }

    [JsonProperty] public DiplomacyTreatyType TreatyType { get; }

    [JsonIgnore] public bool IsValid => IsValidTreatyTime();

    public TIFactionState GetInitiator()
    {
        return GameStateManager.FindGameState<TIFactionState>(InitiatorID);
    }

    public TIFactionState GetOther()
    {
        return GameStateManager.FindGameState<TIFactionState>(OtherID);
    }

    private bool IsValidTreatyTime()
    {
        switch (TreatyType)
        {
            case DiplomacyTreatyType.ResetRelation:
                return TITimeState.CampaignDuration_days() - _treatyGameDay < ModState.ResetRelationsTreatyValidDays;
            case DiplomacyTreatyType.Nap:
                return TITimeState.CampaignDuration_days() - _treatyGameDay < ModState.NapTreatyValidDays;
            case DiplomacyTreatyType.Truce:
                return TITimeState.CampaignDuration_days() - _treatyGameDay < ModState.TruceTreatyValidDays;
            case DiplomacyTreatyType.None:
            default:
                return false;
        }
    }
}