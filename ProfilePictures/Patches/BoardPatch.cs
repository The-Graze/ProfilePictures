using GorillaNetworking;
using HarmonyLib;
using System.ComponentModel;
using UnityEngine;

namespace ProfilePictures.Patches
{
    [HarmonyPatch(typeof(GorillaPlayerScoreboardLine))]
    [HarmonyPatch("UpdateLine")]
    [HarmonyWrapSafe]
    public class BoardPatch
    {
        private static bool Prefix(GorillaPlayerScoreboardLine __instance)
        {
            if (__instance.playerVRRig.creator.CustomProperties.ContainsKey("PFP"))
            {
                if (__instance.GetComponent<ProfilePic>() == null)
                {
                    __instance.gameObject.AddComponent<ProfilePic>();
                }
                if (__instance.playerNameVisible != __instance.playerVRRig.playerNameVisible)
                {
                    __instance.UpdatePlayerText();
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

                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
