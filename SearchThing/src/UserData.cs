using MelonLoader.Utils;

namespace SearchThing;

public static class UserData
{
    public static readonly string DirectoryPath = MelonEnvironment.UserDataDirectory + "/searchthing";
    public static readonly string DatabasePath = DirectoryPath + "/database.db";
    public static readonly string PresetsPath = DirectoryPath + "/presets";
    
    static UserData()
    {
        if (!Directory.Exists(DirectoryPath))
        {
            Directory.CreateDirectory(DirectoryPath);
        }
        
        if (!Directory.Exists(PresetsPath))
        {
            Directory.CreateDirectory(PresetsPath);
        }
    }
}