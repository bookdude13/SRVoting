using Synth.SongSelection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace SRVoting.Harmony
{
    [HarmonyPatch(typeof(SongSelectionManager), nameof(SongSelectionManager.OnLeftRoom))]
    public class Patch_SongSelectionManager_OnLeftRoom
    {
        public static void Postfix()
        {
            SRVoting.Instance?.OnMultiplayerRoomLeft();
        }
    }
}
