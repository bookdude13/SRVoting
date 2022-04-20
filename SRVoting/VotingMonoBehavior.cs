using SRVoting.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SRVoting
{
    class VotingMonoBehavior : MonoBehaviour
    {
        private Util.ILogger logger;
        private SynthriderzService synthriderzService;

        public void Init(Util.ILogger logger, SynthriderzService synthriderzService)
        {
            this.logger = logger;
            this.synthriderzService = synthriderzService;
        }

        public void RefreshVotes()
        {
            logger.Msg("Refreshing votes");
            StartCoroutine(UpdateVoteUI());
        }

        private IEnumerator UpdateVoteUI()
        {
            if (synthriderzService == null)
            {
                logger.Msg("No synthriderz service; skipping");
                yield return null;
            }

            logger.Msg("Song clicked; waiting for update");
            yield return new WaitForSeconds(0.1f);

            var selectedTrack = Synth.SongSelection.SongSelectionManager.GetInstance?.SelectedGameTrack;
            string songName = selectedTrack?.TrackName ?? "";
            string songHash = selectedTrack?.LeaderboardHash ?? "";
            bool isCustom = selectedTrack?.IsCustomSong ?? false;
            bool canLeaderboard = selectedTrack?.CanUpdateLeaderboard ?? false;
            logger.Msg($"{songName} selected. IsCustom? {isCustom}. CanPostLeaderboards? {canLeaderboard}. Hash: {songHash}");

            yield return synthriderzService.GetVotes(songHash);
        }
    }
}
