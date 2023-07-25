using PavonisInteractive.TerraInvicta;

namespace Diplomacy.Core;

public class DiplomacyTreaty
{
    private readonly int _treatyGameDay;

    public DiplomacyTreaty(TIFactionState initiator, TIFactionState other, DiplomacyTreatyType treatyType)
    {
        TreatyType = treatyType;
        Initiator = initiator;
        Other = other;
        _treatyGameDay = TITimeState.CampaignDuration_days();
    }

    public DiplomacyTreatyType TreatyType { get; }
    public TIFactionState Initiator { get; }
    public TIFactionState Other { get; }
    public bool IsValid => IsValidTreatyTime();

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