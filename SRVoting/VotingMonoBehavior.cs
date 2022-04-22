using SRVoting.Models;
using SRVoting.Services;
using SRVoting.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

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

            yield return synthriderzService.GetVotes(songHash, (getVotesResponse) =>
            {
                logger.Msg("Successfully retrieved votes. Updating UI...");

                try
                {
                    var rootGO = GameObject.Find("CentralPanel/Song Selection/VisibleWrap");
                    var selectedTrackGO = rootGO.transform.Find("Selected Track");
                    var controlsGO = rootGO.transform.Find("Controls");

                    var arrowUp = controlsGO.Find("MuteWrap/Arrow UP");
                    logger.Msg("up arw? " + arrowUp?.name);

                    var difficultiesGO = selectedTrackGO.transform.Find("Difficulties");
                    logger.Msg("Diff? " + difficultiesGO?.name);
                    Transform hardButton = difficultiesGO.Find("StandardButton - Hard");
                    logger.Msg("hard? " + hardButton?.name);
                    hardButton.transform.localPosition += new Vector3(0.0f, -0.5f, 0.0f);

                    /*var testGO = new GameObject("srvoting_upvote_test", typeof(Image));
                    testGO.transform.SetParent(difficultiesGO);
                    testGO.transform.localPosition = Vector3.zero;
                    testGO.transform.position = hardButton.position + new Vector3(4.0f, 0.0f, 0.0f);
                    testGO.transform.localRotation = hardButton.localRotation;
                    var testImg = testGO.GetComponent<Image>();
                    testImg.color = Color.white;

                    var upArrowGO = GameObject.Instantiate(hardButton, difficultiesGO.transform);
                    upArrowGO.name = "srvoting_upvote";
                    upArrowGO.transform.localRotation = Quaternion.identity;
                    upArrowGO.transform.position = hardButton.position + new Vector3(3.0f, 0.0f, 0.0f);
                    upArrowGO.transform.localScale = new Vector3(0.5f, 1.0f, 1.0f);*/
                } catch (Exception e)
                {
                    logger.Msg("Failed to setup UI: " + e.Message + "\n" + e.StackTrace);
                }

                logger.Msg("Done updating UI");
            });
        }
    }
}
