namespace Diplomacy.Core.Treaty;

public enum DiplomacyTreatyType
{
    None = TradeOffer.TreatyType.None,
    Truce = TradeOffer.TreatyType.Truce,
    Nap = TradeOffer.TreatyType.NAP,
    Intel = TradeOffer.TreatyType.Intel,
    ResetRelation,
    Alliance,
    AllianceBroken
}