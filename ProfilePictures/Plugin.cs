using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using GorillaNetworking;
using HarmonyLib;

namespace ProfilePictures;

[BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
public class Plugin : BaseUnityPlugin
{
    private readonly ConfigEntry<string>? _imageURL;
    private Plugin()
    {
        new Harmony(PluginInfo.Guid).PatchAll(Assembly.GetExecutingAssembly());
        _imageURL = Config.Bind("Settings", "ImageURL",
            "https://raw.githubusercontent.com/The-Graze/the-graze.github.io/refs/heads/main/images/Screenshot_20251130_024931.png",
            "The URL for the image, Needs to be the file on a website \n ('https://*****.png' as an example)");
        _imageURL.SettingChanged += (_, _) => { SetProps(); };
    }

    private void Start()
    {
        CosmeticsV2Spawner_Dirty.OnPostInstantiateAllPrefabs2 += SetProps;
    }

    private void SetProps()
    {
        var tbl = NetworkSystem.Instance.LocalPlayer.GetPlayerRef().CustomProperties;
        tbl.AddOrUpdate(Constants.PropName, _imageURL?.Value);

        NetworkSystem.Instance.LocalPlayer.GetPlayerRef().SetCustomProperties(tbl);
    }
}

public static class Constants
{
    public const string PropName = "ProfilePictures";
}