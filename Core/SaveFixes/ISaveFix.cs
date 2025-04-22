using System.Collections.Generic;
using Diplomacy.Core.Treaty;

internal interface ISaveFix
{
    string FromVersion { get; } // OLD MOD SAVE VERSION (FOUND IN INNERSTATE)
    string ToVersion { get; }   // NEW MOD SAVE VERSION (FOUND IN INNERSTATE)
    void Fix(GameControl gameControl, List<DiplomacyTreaty> treaties);
}
