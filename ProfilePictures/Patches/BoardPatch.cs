using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProfilePictures.Patches
{
    [HarmonyPatch(typeof(GorillaPlayerScoreboardLine))]
    [HarmonyPatch("Start", MethodType.Normal)]
    internal class BoardPatch
    {
        private static void Postfix(GorillaPlayerScoreboardLine __instance)
        {
            if (__instance.linePlayer.CustomProperties.ContainsKey("PFP"))
            {
                PFPAdder a = __instance.gameObject.AddComponent<PFPAdder>();
                a.scoreboardLine = __instance;
            }
        }
    }
}