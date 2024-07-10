using GorillaNetworking;
using HarmonyLib;
using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Networking;

namespace ProfilePictures.Patches
{
    [HarmonyPatch(typeof(GorillaPlayerScoreboardLine))]
    [HarmonyPatch("UpdateLine", MethodType.Normal)]
    public static class BoardPatch
    {
        public static Sprite PFP;
        private static bool Prefix(GorillaPlayerScoreboardLine __instance)
        {
            try
            {
                if (__instance.playerNameVisible != __instance.playerVRRig.playerNameVisible)
                {
                    __instance.UpdatePlayerText();
                }

                if (Time.time > __instance.initTime + __instance.emptyRigCooldown)
                {
                    if (__instance.playerVRRig.photonView != null)
                    {
                        __instance.emptyRigCount = 0;
                    }
                    else
                    {
                        __instance.emptyRigCount++;
                        if (__instance.emptyRigCount > 30)
                        {
                            GorillaNot.instance.SendReport("empty rig", __instance.linePlayer.UserId, __instance.linePlayer.NickName);
                        }
                    }
                }
                if (__instance.playerVRRig.creator.CustomProperties.ContainsKey("PFP"))
                {
                    if (!Plugin.AllreadySaved.ContainsKey(__instance.playerVRRig.creator))
                    {
                        GorillaComputer.instance.StartCoroutine(GetTexture(__instance));
                    }
                    else
                    {
                        PFP = Plugin.AllreadySaved[__instance.playerVRRig.creator];
                    }
                    __instance.playerSwatch.color = Color.white;
                    __instance.playerSwatch.sprite = PFP;
                }
                else
                {
                    Material material = ((__instance.playerVRRig.setMatIndex != 0) ? __instance.playerVRRig.materialsToChangeTo[__instance.playerVRRig.setMatIndex] : __instance.playerVRRig.scoreboardMaterial);
                    if (__instance.playerSwatch.material != material)
                    {
                        __instance.playerSwatch.material = material;
                        __instance.playerSwatch.sprite = null;
                    }
                    if (__instance.playerSwatch.color != __instance.playerVRRig.materialsToChangeTo[0].color)
                    {
                        __instance.playerSwatch.color = __instance.playerVRRig.materialsToChangeTo[0].color;
                    }
                }

                if (__instance.myRecorder == null)
                {
                    __instance.myRecorder = NetworkSystem.Instance.LocalRecorder;
                }

                if (__instance.playerVRRig != null)
                {
                    if (__instance.playerVRRig.remoteUseReplacementVoice || __instance.playerVRRig.localUseReplacementVoice || GorillaComputer.instance.voiceChatOn == "FALSE")
                    {
                        if (__instance.playerVRRig.SpeakingLoudness > __instance.playerVRRig.replacementVoiceLoudnessThreshold && !__instance.rigContainer.ForceMute && !__instance.rigContainer.Muted)
                        {
                            __instance.speakerIcon.enabled = true;
                        }
                        else
                        {
                            __instance.speakerIcon.enabled = false;
                        }
                    }
                    else if ((__instance.rigContainer.Voice != null && __instance.rigContainer.Voice.IsSpeaking) || (__instance.playerVRRig.rigSerializer.IsLocallyOwned && __instance.myRecorder != null && __instance.myRecorder.IsCurrentlyTransmitting))
                    {
                        __instance.speakerIcon.enabled = true;
                    }
                    else
                    {
                        __instance.speakerIcon.enabled = false;
                    }
                }
                else
                {
                    __instance.speakerIcon.enabled = false;
                }

                if (!__instance.isMuteManual)
                {
                    bool isPlayerAutoMuted = __instance.rigContainer.GetIsPlayerAutoMuted();
                    if (__instance.muteButton.isAutoOn != isPlayerAutoMuted)
                    {
                        __instance.muteButton.isAutoOn = isPlayerAutoMuted;
                        __instance.muteButton.UpdateColor();
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        public static IEnumerator GetTexture(GorillaPlayerScoreboardLine GPBLine)
        {
            if (Plugin.AllreadySaved.ContainsKey(GPBLine.playerVRRig.creator))
            {
                GPBLine.playerSwatch.sprite = Plugin.AllreadySaved[GPBLine.playerVRRig.creator];
            }
            else
            {
                string URL = GPBLine.playerVRRig.creator.CustomProperties["PFP"].ToString();
                UnityWebRequest www = UnityWebRequestTexture.GetTexture(URL);
                yield return www.SendWebRequest();
                Texture image = ((DownloadHandlerTexture)www.downloadHandler).texture;
                PFP = Sprite.Create((Texture2D)image, new Rect(0.0f, 0.0f, image.width, image.height), new Vector2(0.5f, 0.5f), 100.0f);
                Plugin.AllreadySaved.AddOrUpdate(GPBLine.playerVRRig.creator, PFP);
                GPBLine.playerSwatch.sprite = PFP;
            }
        }
    }
}