using HarmonyLib;

namespace ProfilePictures;

public static class Patches
{
    [HarmonyPatch(typeof(GorillaPlayerScoreboardLine))]
    private class GorillaPlayerScoreboardLinePatches
    {
        [HarmonyPatch(nameof(GorillaPlayerScoreboardLine.UpdateLine))]
        [HarmonyPrefix]
        // ReSharper disable once InconsistentNaming
        private static void UpdateLinePatch(GorillaPlayerScoreboardLine __instance)
        {
            __instance.TryGetComponent<ProfilePictureHandler>(out var profilePicture);
            if (!profilePicture)
                __instance.gameObject.AddComponent<ProfilePictureHandler>().Refresh();
            else
                profilePicture.Refresh();
        }
    }
}