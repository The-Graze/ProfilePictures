using HarmonyLib;

namespace ProfilePictures.Patches
{
    [HarmonyPatch(typeof(GorillaPlayerScoreboardLine))]
    [HarmonyPatch("UpdateLine", MethodType.Normal)]
    internal class BoardPatch
    {
        private static void Postfix(GorillaPlayerScoreboardLine __instance)
        {
            __instance.GetOrAddComponent<BoardLineHandler>(out BoardLineHandler a );
            a.GPBLine = __instance;
        }
    }
}