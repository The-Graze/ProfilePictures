using GorillaNetworking;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace ProfilePictures
{
    public class BoardLineHandler : MonoBehaviour
    {
        public GorillaPlayerScoreboardLine GPBLine;

        Player player;
        Sprite PFP;
        Sprite DeafultSprite;
        bool GrabbedPfP;

        void Start()
        {
            DeafultSprite = GPBLine.playerSwatch.sprite;
        }

        void Update()
        {
            player = GPBLine.rigContainer.photonView.Owner;
            if (player.CustomProperties.ContainsKey("PFP"))
            {
                if (Plugin.Net.AllreadySaved.ContainsKey(player))
                {
                    PFP = Plugin.Net.AllreadySaved[player];
                    GrabbedPfP = true;
                }
                else
                {
                    StartCoroutine(GetTexture(player));
                }
                if (PFP != null && GrabbedPfP)
                {
                    GPBLine.playerSwatch.sprite = PFP;
                    GPBLine.playerSwatch.color = Color.white;
                }
            }
            else
            {
                PFP = null;
                GPBLine.playerSwatch.sprite = DeafultSprite;
                GPBLine.playerSwatch.color = GPBLine.playerVRRig.playerColor;
                GrabbedPfP = false;
            }
        }
        public IEnumerator GetTexture(Player player)
        {
            string URL = player.CustomProperties["PFP"].ToString();
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(URL);
            yield return www.SendWebRequest();
            Texture image = ((DownloadHandlerTexture)www.downloadHandler).texture;
            PFP = Sprite.Create((Texture2D)image, new Rect(0.0f, 0.0f, image.width, image.height), new Vector2(0.5f, 0.5f), 100.0f);
            Plugin.Net.AllreadySaved.AddOrUpdate(player, PFP);
            GrabbedPfP = true;
        }

    }
}
