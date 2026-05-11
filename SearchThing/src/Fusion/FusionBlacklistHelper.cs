using HarmonyLib;
using LabFusion.Data;
using MelonLoader;

namespace SearchThing.Fusion;

public static class FusionBlacklistHelper
{
    private static List<string>? GetBlacklist()
    {
        var traverse = new Traverse(typeof(ModBlacklist)).Field("_blacklistedModIds");
        if (traverse.FieldExists())
            return traverse.GetValue<List<string>>();

        MelonLogger.Error("Failed to get blacklist: _blacklistedModIds field not found");
        return new List<string>();
    }

    private static void SaveBlacklist(List<string> blacklist)
    {
        var path = PersistentData.GetPath("mod_blacklist.txt");

        using var sw = File.CreateText(path);

        sw.WriteLine("# This file is for preventing the use of specific mods while in a server.");
        sw.WriteLine("# To blacklist a mod, add a mod identifier on its own line.");
        sw.WriteLine("# A mod identifier can either be its mod.io number ID, mod.io URL ID, or AssetWarehouse barcode.");
        sw.WriteLine("# When the identifier is a number ID or URL ID, it will block the mod from being downloaded.");
        sw.WriteLine("# On the other hand, if the identifier is a AssetWarehouse barcode, it will prevent the mod from being used.");
        sw.WriteLine("# For server hosts, blacklisting a barcode prevents all clients from using that mod.");
        sw.WriteLine("#");
        sw.WriteLine("# Examples");
        sw.WriteLine("# -------------------------------------");
        sw.WriteLine("# Number ID");
        sw.WriteLine("# 4057308");
        sw.WriteLine("#");
        sw.WriteLine("# URL ID");
        sw.WriteLine("# test-chambers");
        sw.WriteLine("#");
        sw.WriteLine("# Barcode");
        sw.WriteLine("# SLZ.TestChambers.Level.TestChamber02");
        sw.WriteLine("# SLZ.TestChambers.Level.TestChamber07");
        sw.WriteLine("# -------------------------------------");

        foreach (var id in blacklist)
        {
            sw.WriteLine(id);
        }
    }

    public static void ToggleBlacklist(string barcode)
    {
        var blacklist = GetBlacklist();
        if (blacklist == null)
        {
            MelonLogger.Error("Cannot toggle blacklist: failed to retrieve blacklist");
            return;
        }

        if (blacklist.Contains(barcode))
        {
            blacklist.Remove(barcode);
        }
        else
        {
            blacklist.Add(barcode);
        }

        SaveBlacklist(blacklist);
    }

    public static bool IsBlacklisted(string barcodeID)
    {
        return ModBlacklist.IsBlacklisted(barcodeID);
    }
}