using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProfilePictures.Patches
{
    /// <summary>
    /// This is an example patch, made to demonstrate how to use Harmony. You should remove it if it is not used.
    /// </summary>
    [HarmonyPatch(typeof(GorillaPlayerScoreboardLine))]
    [HarmonyPatch("Start", MethodType.Normal)]
    internal class BoardPatch
    {
        private static void Postfix(GorillaPlayerScoreboardLine __instance)
        {
            if(__instance.linePlayer.CustomProperties.ContainsKey("PFP")) 
            {
                Plugin.Instance.PlayerLines.Add(__instance);
                __instance.gameObject.AddComponent<PFPAdder>().scoreboardLine = __instance;
            }
        }
    }
}