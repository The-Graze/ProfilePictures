using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using BepInEx;
using BepInEx.Configuration;
using GorillaNetworking;
using HarmonyLib;
using ProfilePictures.DiscordSDK;
using UnityEngine.Networking;
using UnityEngine;

namespace ProfilePictures;
[BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
public class Plugin : BaseUnityPlugin
{
    private readonly ConfigEntry<string>? _imageURL;
    private static ConfigEntry<string>? _imageSource;
    private float _timer;
    private Discord? _cord;
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private int? _cordAttempts = 0;
    private bool _cordGiveUp, _firstRun;
    private enum Source
    {
        Discord,
        Steam,
        URL
    }
    private Plugin()
    {
        try
        {
            new Harmony(PluginInfo.Guid).PatchAll(Assembly.GetExecutingAssembly());
            var description = new ConfigDescription(
                "Where should your PFP be sourced from",
                new AcceptableValueList<string>(nameof(Source.Discord), nameof(Source.Steam), nameof(Source.URL))
            );
            _imageSource = Config.Bind("Settings", "PFP source", nameof(Source.Discord) , description);
        
            _imageURL = Config.Bind("Settings", "ImageURL",
                "https://raw.githubusercontent.com/The-Graze/the-graze.github.io/refs/heads/main/images/Screenshot_20251130_024931.png",
                "The URL for the image if that setting is enabled \n, Needs to be the file on a website \n ('https://*****.png' as an example)\n");
            _imageSource.SettingChanged += (_,_) => SetProps();
            _imageURL.SettingChanged += (_, _) => SetProps();
            Config.SaveOnConfigSet = true;
        
            CosmeticsV2Spawner_Dirty.OnPostInstantiateAllPrefabs += FirstTime;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        
    }

    private void Update()
    {
        if(_cordGiveUp || !_firstRun)
            return;
        
        _timer += Time.deltaTime;
        if (!(_timer >= 0.1f)) return; //100 ms
        _timer = 0f;
        
        if (_cord == null) InitCord();
        
        else _cord?.RunCallbacks();
    }

    private void FirstTime()
    {
        _firstRun = true;
        SetProps();
        CosmeticsV2Spawner_Dirty.OnPostInstantiateAllPrefabs2 -= FirstTime;
    }

    private async void SetProps()
    {
        try
        {
            var tbl = NetworkSystem.Instance.LocalPlayer.GetPlayerRef().CustomProperties;

            switch (_imageSource?.Value)
            {
                case nameof(Source.Discord):
                    tbl.AddOrUpdate(Constants.PropName, DiscordPfp());
                    break;
                case nameof(Source.Steam):
                    var avatarUrl = await GetAvatarUrl(Steamworks.SteamUser.GetSteamID().m_SteamID);
                    tbl.AddOrUpdate(Constants.PropName, avatarUrl);
                    break;
                default:
                    tbl.AddOrUpdate(Constants.PropName, _imageURL?.Value);
                    break;
            }
            Logger.LogInfo($"Setting PFP Prop to: {_imageSource?.Value}");
            NetworkSystem.Instance.LocalPlayer.GetPlayerRef().SetCustomProperties(tbl);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    
    private void InitCord()
    {
        try
        {
            _cord = new Discord(1133006310640734300, (ulong)CreateFlags.NoRequireDiscord);
        }
        catch (Exception e)
        {
            if (_cordAttempts >= 5)
            {
                _cordGiveUp = true;
                File.WriteAllText("error.txt",$"{e.Message + e.StackTrace}");
                Logger.LogError($"Discord SDK Failed, Setting Source to Steam If discord is selected");
                // ReSharper disable once InvertIf
                if (_imageSource?.Value == nameof(Source.Discord))
                {
                    _imageSource.SetSerializedValue(nameof(Source.Steam));
                    SetProps();
                }

                return;
            }

            _cordAttempts++;
            Logger.LogError($"Discord RPC/SDK Failed to Init for:\n'{e.Message}'\n trying again [{_cordAttempts}/{5}]");
        }
    }
    
    public string DiscordPfp()
    {
        var user = _cord!.GetUserManager().GetCurrentUser();
        return string.IsNullOrEmpty(user.Avatar) ? $"https://cdn.discordapp.com/embed/avatars/{user.Id % 5}.png" : $"https://cdn.discordapp.com/avatars/{user.Id}/{user.Avatar}.png?size=1024";
    }
    
    public static async Task<string?> GetAvatarUrl(ulong steamId)
    {
        var url = $"https://steamcommunity.com/profiles/{steamId}/?xml=1";

        using var web = UnityWebRequest.Get(url);
        var operation = web.SendWebRequest();
        while (!operation.isDone)
            await Task.Yield();

        if (web.result != UnityWebRequest.Result.Success)
            return null;
        
        var xml = new XmlDocument();
        xml.LoadXml(web.downloadHandler.text);

        var node = xml.SelectSingleNode("//avatarFull");
        return node?.InnerText;
    }
}

public static class Constants
{
    public const string PropName = "ProfilePictures";
}