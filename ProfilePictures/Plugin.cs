using BepInEx;
using BepInEx.Configuration;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using ProfilePictures.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace ProfilePictures
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<String> PFPLink;
        public static Dictionary<Player, Sprite> AllreadySaved = new Dictionary<Player, Sprite>();
        public static List<Player> SavedPlayerList;
        public static Sprite LocalSprite;
        Plugin()
        {
            Harmony harmony = Harmony.CreateAndPatchAll(typeof(Plugin).Assembly, PluginInfo.GUID);
        }

        void Update()
        {
            SavedPlayerList = AllreadySaved.Keys.ToList();
        }

        void Start()
        {
            this.AddComponent<Net>();
            PFPLink = Config.Bind("Settings", "Profile Picture URL", "https://static.wikia.nocookie.net/gorillatag/images/7/77/Gorillapin.png/revision/latest?cb=20220223225937", "Paste the Link here for people to see ur url");
            StartCoroutine(GetTexture(PFPLink.Value));
        }
        public class Net : MonoBehaviourPunCallbacks
        {
            public override void OnLeftRoom()
            {
                AllreadySaved.Clear();
                AllreadySaved.Add(PhotonNetwork.LocalPlayer, LocalSprite);
            }
            public override void OnPreLeavingRoom()
            {
                AllreadySaved.Clear();
                AllreadySaved.Add(PhotonNetwork.LocalPlayer, LocalSprite);
            }
            public override void OnPlayerLeftRoom(Player otherPlayer)
            {
                AllreadySaved.Remove(otherPlayer);
                AllreadySaved.Add(PhotonNetwork.LocalPlayer, LocalSprite);
            }
        }
        public static IEnumerator GetTexture(string URL)
        {
            var table = PhotonNetwork.LocalPlayer.CustomProperties;
            table.AddOrUpdate("PFP", PFPLink.Value);
            PhotonNetwork.LocalPlayer.SetCustomProperties(table);
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(URL);
            yield return www.SendWebRequest();
            Texture image = ((DownloadHandlerTexture)www.downloadHandler).texture;
            Sprite PFP = Sprite.Create((Texture2D)image, new Rect(0.0f, 0.0f, image.width, image.height), new Vector2(0.5f, 0.5f), 100.0f);
            PFP.name = "_Local_" + PhotonNetwork.LocalPlayer.NickName;
            LocalSprite = PFP;

        }
    }
}
