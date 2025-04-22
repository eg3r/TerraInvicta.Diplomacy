using System.Collections.Generic;

using Diplomacy.Core.Treaty;

/** 
  * This is fixing save files from version 1.0.0 to 1.0.1.
  * It updates the diplomacy treaties to ensure that allied factions have correct intel on each other.
  * Was broken in 1.0.0 
  */
public class SaveFix_1_0_0_to_1_0_1 : ISaveFix
{
    public string FromVersion => "1.0.0";
    public string ToVersion => "1.0.1";

    public void Fix(GameControl gameControl, List<DiplomacyTreaty> treaties)
    {
        // get all allied factions from ModState and give them correct intel on each other
        foreach (var treaty in treaties)
        {
            if (treaty.IsValid && treaty.TreatyType == DiplomacyTreatyType.Alliance)
            {
                var initiator = treaty.GetInitiator();
                var other = treaty.GetOther();

                if (initiator != null && other != null)
                {
                    initiator.GiveIntelToFaction(other, false);
                    other.GiveIntelToFaction(initiator, false);
                }
            }
        }
    }
}
