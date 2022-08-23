using Synth.SongSelection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace SRVoting.Harmony
{
    [HarmonyPatch(typeof(SongSelectionManager), nameof(SongSelectionManager.OpenMultiplayerRoomMenu))]
    public class Patch_SongSelectionManager_OpenMultiplayerRoomMenu
    {
        public static void Postfix(Synth.Versus.Room room)
        {
            SRVoting.Instance?.OnOpenMultiplayerRoomMenu(room);
        }
    }
}
