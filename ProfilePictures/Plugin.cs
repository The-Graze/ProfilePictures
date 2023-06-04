using BepInEx;
using BepInEx.Configuration;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ProfilePictures
{
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static volatile Plugin Instance;
        public Dictionary<Player, Sprite> PlayerSprites = new Dictionary<Player, Sprite>();
        public ConfigEntry<String> PFPLink;
        public List<GorillaPlayerScoreboardLine> PlayerLines = new List<GorillaPlayerScoreboardLine>();
        public Sprite lPFP;
        void Awake()
        {
            Instance = this;
            PFPLink = Config.Bind("Settings","Profile Picture URL", "https://static.wikia.nocookie.net/gorillatag/images/7/77/Gorillapin.png/revision/latest?cb=20220223225937", "Paste the Link here for people to see ur url");
        }
        void Start()
        {
            Utilla.Events.GameInitialized += OnGameInitialized;
            HarmonyPatches.ApplyHarmonyPatches();
        }
        void OnGameInitialized(object sender, EventArgs e)
        {
            HarmonyPatches.ApplyHarmonyPatches();
            var table = PhotonNetwork.LocalPlayer.CustomProperties;
            table.Add("PFP", PFPLink.Value);
            PhotonNetwork.LocalPlayer.SetCustomProperties(table);
        }
        public IEnumerator GetTexture(Player player, string URL, bool islocal)
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
                Sprite pfp = Sprite.Create((Texture2D)image, new Rect(0.0f, 0.0f, image.width, image.height), new Vector2(0.5f, 0.5f), 100.0f);

                if (!PlayerSprites.ContainsKey(player) || !PlayerSprites.ContainsValue(pfp) && islocal == false)
                {
                    PlayerSprites.Add(player, pfp);
                }
                if (player == PhotonNetwork.LocalPlayer && lPFP == null)
                {
                    lPFP = pfp;
                }
                foreach (GorillaPlayerScoreboardLine sb in PlayerLines)
                {
                    if (sb.linePlayer == player)
                    {
                        sb.GetComponent<PFPAdder>().pfp = pfp;
                        sb.GetComponent<PFPAdder>().hasPFP = true;
                    }
                }
                www.Dispose();
            }
        }

        void Update()
        {
           foreach (Player p in PlayerSprites.Keys)
           {
                if(p == null) PlayerSprites.Remove(p);
           }
           if(!PhotonNetwork.InRoom) PlayerSprites.Clear(); PlayerLines.Clear();
        }
    }
    public class PFPAdder : MonoBehaviour
    {
        Plugin p = Plugin.Instance;
        GorillaPlayerScoreboardLine scoreboardLine;
        public Sprite pfp;
        public bool hasPFP;
        void Start()
        {
            scoreboardLine = this.GetComponent<GorillaPlayerScoreboardLine>();
            p.PlayerLines.Add(scoreboardLine);
        }

        void Update()
        {
            if (scoreboardLine.linePlayer.IsLocal)
            {
                pfp = p.lPFP;
                hasPFP = true;
            }
            if (hasPFP && pfp != null)
            {
                scoreboardLine.playerSwatch.overrideSprite = pfp;
                scoreboardLine.playerSwatch.color = Color.white;
            }
        }

        void OnDisable()
        {
            p.PlayerLines.Remove(scoreboardLine);
        }
        void OnDestroy()
        {
            p.PlayerLines.Remove(scoreboardLine);
        }
    }


}
