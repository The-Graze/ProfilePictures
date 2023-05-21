using BepInEx;
using BepInEx.Configuration;
using ExitGames.Client.Photon.StructWrapping;
using Oculus.Platform;
using OVR.OpenVR;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Utilla;
using static Mono.Security.X509.X520;
using static System.Net.Mime.MediaTypeNames;

namespace ProfilePictures
{
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static volatile Plugin Instance;
        public ConfigEntry<string> PFPLink;
        void Awake()
        {
            Instance = this;
            PFPLink = Config.Bind("Settings","Profile Picture URL", "https://static.wikia.nocookie.net/gorillatag/images/7/77/Gorillapin.png/revision/latest?cb=20220223225937", "Paste the Link here for people to see ur url");
        }
        void Start()
        { Utilla.Events.GameInitialized += OnGameInitialized; }
        void OnGameInitialized(object sender, EventArgs e)
        {
            HarmonyPatches.ApplyHarmonyPatches();
            var table = PhotonNetwork.LocalPlayer.CustomProperties;
            table.Add("PFP", PFPLink.Value);
            PhotonNetwork.LocalPlayer.SetCustomProperties(table);
        }
    }
    public class PFPAdder : MonoBehaviour
    {
        GorillaPlayerScoreboardLine s;
        string URL;
        Sprite pfp;
        bool hasimage;
        void Awake()
        {
            hasimage = false;
            s = GetComponent<GorillaPlayerScoreboardLine>();
            if (s.playerVRRig.photonView.Owner.CustomProperties.ContainsKey("PFP"))
            {
                URL = s.playerVRRig.photonView.Owner.CustomProperties["PFP"].ToString();
                if(URL != null) StartCoroutine(GetTexture(URL));
            }
        }
        void Update()
        {
            if (hasimage == true)
            {
                s.playerSwatch.color = Color.white;
                s.playerSwatch.overrideSprite = pfp;
            }
        }
        IEnumerator GetTexture(string URL)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(URL);
            yield return www.SendWebRequest();
            if (www.error != null)
            {
                Debug.Log(www.error);
            }
            else
            {
                Texture image = ((DownloadHandlerTexture)www.downloadHandler).texture;
                pfp = Sprite.Create((Texture2D)image, new Rect(0.0f, 0.0f, image.width, image.height), new Vector2(0.5f, 0.5f), 100.0f);
                s.infectedTexture = (Texture2D)image;
                hasimage = true;
                www.Dispose();
            }
        }
    }
}
