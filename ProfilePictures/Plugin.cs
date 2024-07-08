using BepInEx;
using BepInEx.Configuration;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProfilePictures
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<String> PFPLink;
        Plugin()
        {
            Harmony harmony = Harmony.CreateAndPatchAll(typeof(Plugin).Assembly, PluginInfo.GUID);
        }

        void Start()
        {
            this.AddComponent<Net>();
            PFPLink = Config.Bind("Settings", "Profile Picture URL", "https://static.wikia.nocookie.net/gorillatag/images/7/77/Gorillapin.png/revision/latest?cb=20220223225937", "Paste the Link here for people to see ur url");
        }
        public class Net : MonoBehaviourPunCallbacks
        {
            public static Dictionary<Player, Sprite> AllreadySaved = new Dictionary<Player, Sprite>();
            public override void OnConnectedToMaster()
            {
                var table = PhotonNetwork.LocalPlayer.CustomProperties;
                table.AddOrUpdate("PFP", PFPLink.Value);
                PhotonNetwork.LocalPlayer.SetCustomProperties(table);
            }
        }
    }
}
