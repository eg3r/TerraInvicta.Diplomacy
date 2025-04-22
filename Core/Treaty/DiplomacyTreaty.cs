using Diplomacy.Core.Helpers;
using Newtonsoft.Json;
using PavonisInteractive.TerraInvicta;

// ReSharper disable SuggestBaseTypeForParameterInConstructor

namespace Diplomacy.Core.Treaty;

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
    [JsonIgnore] public bool IsValid => CheckIfValid();
    [JsonIgnore] public int TreatyGameDay => _treatyGameDay;

    public TIFactionState GetInitiator()
    {
        return GameStateManager.FindGameState<TIFactionState>(InitiatorID);
    }

    public TIFactionState GetOther()
    {
        return GameStateManager.FindGameState<TIFactionState>(OtherID);
    }

    private bool CheckIfValid()
    {
        // Check used for treaty creation, if the days are small (same day) treaty is still in creation
        if (TITimeState.CampaignDuration_days() - _treatyGameDay < 3)
            return true;

        switch (TreatyType)
        {
            case DiplomacyTreatyType.ResetRelation:
                return TITimeState.CampaignDuration_days() - _treatyGameDay < ModState.ResetRelationsTreatyValidDays;
                
            case DiplomacyTreatyType.Nap:
                return GetInitiator().HasNap(GetOther())
                    // Prevents repeat before max duration
                    || TITimeState.CampaignDuration_days() - _treatyGameDay < ModState.NapTreatyRepeatDays;
                    
            case DiplomacyTreatyType.Truce:
                return GetInitiator().HasTruce(GetOther())
                    // Prevents repeat before max duration
                    || TITimeState.CampaignDuration_days() - _treatyGameDay < ModState.TruceTreatyRepeatDays;
                    
            case DiplomacyTreatyType.Alliance:
                return true; // Alliance does not run out, can only be broken
                
            case DiplomacyTreatyType.AllianceBroken:
                return TITimeState.CampaignDuration_days() - _treatyGameDay < ModState.AllianceBrokenValidDays;
                
            case DiplomacyTreatyType.Intel:
                // Intel like alliance does not run out, can only be broken
                var initiator = GetInitiator();
                var other = GetOther();
                return initiator.IsIntelSharedBy(other) || other.IsIntelSharedBy(initiator);
                
            case DiplomacyTreatyType.None:
            default:
                return false;
        }
    }
}