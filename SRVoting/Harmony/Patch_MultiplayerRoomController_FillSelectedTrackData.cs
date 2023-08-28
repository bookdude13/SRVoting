using HarmonyLib;
using Il2CppUtil.Controller;

namespace SRVoting.Harmony
{
    // This is called during initialization (for the host),
    // and from Game_InfoProvider.OnEvent() with event code SetSelectedVersusTrack (for clients)
    [HarmonyPatch(typeof(MultiplayerRoomController), nameof(MultiplayerRoomController.FillSelectedTrackData))]
    public class Patch_MultiplayerRoomController_FillSelectedTrackData
    {
        public static void Postfix(object[] data)
        {
            SRVoting.Instance?.OnMultiplayerRoomUpdateTrackData();
        }
    }
}
