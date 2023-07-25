using System;
using System.IO;
using Diplomacy.Core.Helpers;
using Newtonsoft.Json;
using PavonisInteractive.TerraInvicta;

namespace Diplomacy.Core;

public static class SaveSystem
{
    private const string SaveFileEnding = ".modsave";
    private static string _saveFolder;

    private static string SaveFolderPath
    {
        get
        {
            if (!string.IsNullOrEmpty(_saveFolder))
                return _saveFolder;

            _saveFolder = CommonHelpers.GetModSaveFolderPath() + Path.DirectorySeparatorChar;
            Directory.CreateDirectory(CommonHelpers.GetModSaveFolderPath());

            return _saveFolder;
        }
    }

    public static bool SaveExists(string saveName, bool useDefaultFileEnding = true)
    {
        return File.Exists(GetSavePath(saveName, useDefaultFileEnding));
    }

    private static string GetSavePath(string name, bool useDefaultFileEnding = true)
    {
        return Path.Combine(SaveFolderPath, name + (useDefaultFileEnding ? SaveFileEnding : ""));
    }

    public static bool Save<T>(T data, string saveName, bool useDefaultFileEnding = true)
    {
        try
        {
            var path = GetSavePath(saveName, useDefaultFileEnding);
            File.WriteAllText(path, JsonConvert.SerializeObject(data));
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            return false;
        }
    }

    public static T Load<T>(string saveName, bool useDefaultFileEnding = true)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(GetSavePath(saveName, useDefaultFileEnding)));
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            return default;
        }
    }

    public static void DeleteSave(string saveName, bool useDefaultFileEnding = true)
    {
        File.Delete(GetSavePath(saveName, useDefaultFileEnding));
    }
}