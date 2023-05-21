using HarmonyLib;
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
            __instance.gameObject.AddComponent<PFPAdder>();
        }
    }
}
