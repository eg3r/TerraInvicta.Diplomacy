using System.IO;
using System.Reflection;

namespace Diplomacy.Core.Helpers;

public static class CommonHelpers
{
    public static DiplomacyLevel Min(this DiplomacyLevel level, DiplomacyLevel other)
    {
        return level > other ? other : level;
    }

    public static string GetModFolderPath()
    {
        return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }

    public static string GetModSaveFolderPath()
    {
        // for now use the games save folder
        return Path.Combine(CreateSaveFileScrollList.GetSaveFolderPath(), "DiplomacyModSaves");
    }
}