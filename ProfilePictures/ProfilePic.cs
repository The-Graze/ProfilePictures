using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace ProfilePictures
{
    public class ProfilePic : MonoBehaviour
    {
        private GorillaPlayerScoreboardLine scoreboardLine;
        private string currentUrl;
        private Sprite playerSprite;

        void Start()
        {
            scoreboardLine = GetComponent<GorillaPlayerScoreboardLine>();
            if (scoreboardLine == null)
            {
                Debug.LogError("GorillaPlayerScoreboardLine component not found.");
                Destroy(this);
                return;
            }
        }

        void Update()
        {
            if (scoreboardLine == null) return;

            if (scoreboardLine.linePlayer.IsLocal)
            {
                if (playerSprite == null)
                {
                    currentUrl = Plugin.PFPLink.Value;
                    playerSprite = Plugin.LocalSprite;
                    UpdateScoreboardLine();
                }
            }
            else
            {
                if (scoreboardLine.playerVRRig.creator.CustomProperties.TryGetValue("PFP", out object pfpUrl))
                {
                    string url = (string)pfpUrl;
                    if (currentUrl != url)
                    {
                        currentUrl = url;
                        playerSprite = null;
                        StartCoroutine(GetTexture(url));
                    }
                }
                else
                {
                    Reset();
                }

                if (playerSprite != null)
                {
                    UpdateScoreboardLine();
                }
            }
        }

        private void UpdateScoreboardLine()
        {
            scoreboardLine.playerSwatch.material = null;
            scoreboardLine.playerSwatch.sprite = playerSprite;
            scoreboardLine.playerSwatch.overrideSprite = playerSprite;
            scoreboardLine.playerSwatch.color = Color.white;
        }

        private void Reset()
        {
            scoreboardLine.playerSwatch.sprite = null;
            scoreboardLine.playerSwatch.overrideSprite = null;
            scoreboardLine.playerSwatch.material = scoreboardLine.playerVRRig.scoreboardMaterial;
            scoreboardLine.playerSwatch.color = scoreboardLine.playerVRRig.playerColor;
            playerSprite = null;
            currentUrl = "";
            StartCoroutine(WaitAndDestroy());
        }

        private IEnumerator WaitAndDestroy()
        {
            yield return new WaitForSeconds(2);
            Destroy(this);
        }

        private IEnumerator GetTexture(string url)
        {
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
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

                playerSprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                playerSprite.name = scoreboardLine.playerVRRig.creator.NickName;
            }
        }
    }
}
