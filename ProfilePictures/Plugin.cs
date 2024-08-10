using BepInEx;
using BepInEx.Configuration;
using GorillaNetworking;
using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace ProfilePictures
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<string> PFPLink;
        public static Sprite LocalSprite;

        private void Awake()
        {
            HarmonyPatches.ApplyHarmonyPatches();
        }

        private void Start()
        {
            // Bind the configuration entry
            PFPLink = Config.Bind("Settings", "Profile Picture URL",
                "https://static.wikia.nocookie.net/gorillatag/images/7/77/Gorillapin.png/revision/latest?cb=20220223225937",
                "Paste the Link here for people to see your URL");

            GorillaTagger.OnPlayerSpawned(() => StartCoroutine(DownloadAndSetTexture()));
        }

        private IEnumerator DownloadAndSetTexture()
        {
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(PFPLink.Value))
            {
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success)
                {
                    yield break;
                }

                Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                if (texture == null)
                {
                    yield break;
                }

                LocalSprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                LocalSprite.name = "Local_" + PhotonNetwork.LocalPlayer.NickName;

                var properties = PhotonNetwork.LocalPlayer.CustomProperties;
                properties.AddOrUpdate("PFP",PFPLink.Value);
                PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
            }
        }
    }
}
