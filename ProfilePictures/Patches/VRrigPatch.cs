using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProfilePictures.Patches
{
    /// <summary>
    /// This is an example patch, made to demonstrate how to use Harmony. You should remove it if it is not used.
    /// </summary>
    [HarmonyPatch(typeof(VRRig))]
    [HarmonyPatch("Start", MethodType.Normal)]
    internal class VRRigPatch
    {
        private static void Postfix(VRRig __instance)
        {
            if (__instance.photonView.Owner.CustomProperties.ContainsKey("PFP"))
            {
                Plugin.Instance.StartCoroutine(Plugin.Instance.GetTexture(__instance.photonView.Owner,__instance.photonView.Owner.CustomProperties["PFP"].ToString(), !__instance.isOfflineVRRig));
            }
        }
    }
}
