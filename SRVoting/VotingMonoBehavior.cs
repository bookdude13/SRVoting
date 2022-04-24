using SRVoting.Models;
using SRVoting.Services;
using SRVoting.UI;
using System.Collections;
using UnityEngine;
using VRTK.UnityEventHelper;

namespace SRVoting
{
    class VotingMonoBehavior : MonoBehaviour
    {
        private readonly string arrowUpName = "srvoting_voteArrowUp";
        private readonly string arrowDownName = "srvoting_voteArrowDown";

        private Util.ILogger logger;
        private SynthriderzService synthriderzService;

        private VoteDirectionComponent upVoteComponent;
        private VoteDirectionComponent downVoteComponent;

        private string currentSongHash = "";
        private VoteState currentSongVote = VoteState.NO_VOTE;

        public void Init(Util.ILogger logger, SynthriderzService synthriderzService)
        {
            this.logger = logger;
            this.synthriderzService = synthriderzService;

            upVoteComponent = new VoteDirectionComponent(logger, arrowUpName, Vote);
            downVoteComponent = new VoteDirectionComponent(logger, arrowDownName, Vote);
        }

        public void OnSongChanged()
        {
            logger.Debug("Song changed. Updating UI");
            StartCoroutine(UpdateVoteUI());
        }

        private void Vote(object sender, VRTK.InteractableObjectEventArgs e)
        {
            string senderName = ((VRTK.VRTK_InteractableObject)sender).name;

            // Translate sender into up/down
            VoteState newVote = senderName == arrowUpName ? VoteState.VOTED_UP : VoteState.VOTED_DOWN;

            // If same as existing, "unvote", otherwise cast desired vote
            VoteState voteToSend = newVote == currentSongVote ? VoteState.NO_VOTE : newVote;

            // Disable events until done voting
            upVoteComponent.DisableEvents();
            downVoteComponent.DisableEvents();
            
            logger.Debug($"Voting for song {currentSongHash}. Old: {currentSongVote}, New: {voteToSend}");
            StartCoroutine(VoteAndUpdateUI(voteToSend));
        }

        private IEnumerator VoteAndUpdateUI(VoteState vote)
        {
            VotesResponseModel getVotesResponse = null;
            string errorMessage = null;

            yield return synthriderzService.Vote(
                currentSongHash,
                vote,
                response => getVotesResponse = response,
                errorMsg => errorMessage = errorMsg
            );

            if (errorMessage != null)
            {
                HandleApiError(errorMessage);
            }
            else
            {
                HandleApiResponse(getVotesResponse);
            }
        }

        private IEnumerator UpdateVoteUI()
        {
            if (synthriderzService == null)
            {
                logger.Msg("No synthriderz service; skipping");
                yield return null;
            }

            logger.Debug("Make sure UI exists...");
            yield return EnsureUIExists();

            // Start disabled while we wait for everything
            upVoteComponent.UpdateUI(false, false, "");
            downVoteComponent.UpdateUI(false, false, "");

            logger.Debug("Song clicked; waiting for update");
            yield return new WaitForSeconds(0.01f);

            logger.Debug("Getting selected track...");
            var selectedTrack = Synth.SongSelection.SongSelectionManager.GetInstance?.SelectedGameTrack;
            string songName = selectedTrack?.TrackName ?? "";
            currentSongHash = selectedTrack?.LeaderboardHash ?? "";
            bool isCustom = selectedTrack?.IsCustomSong ?? false;
            logger.Msg($"{songName} selected. IsCustom? {isCustom}. Hash: {currentSongHash}");

            if (!isCustom)
            {
                upVoteComponent.UpdateUI(false, false, "N/A");
                downVoteComponent.UpdateUI(false, false, "N/A");
            }
            else {
                VotesResponseModel getVotesResponse = null;
                string errorMessage = null;
                yield return synthriderzService.GetVotes(
                    currentSongHash,
                    response => getVotesResponse = response,
                    errorMsg => errorMessage = errorMsg
                );

                if (errorMessage != null)
                {
                    HandleApiError(errorMessage);
                }
                else
                {
                    HandleApiResponse(getVotesResponse);
                }

                logger.Debug("Done updating UI");
            }
        }

        private void HandleApiError(string errorMessage)
        {
            logger.Msg(errorMessage);

            // Just hide the UI elements on error for now.
            // Eventually this could be error text or an error prompt
            HandleApiResponse(null);
        }

        private void HandleApiResponse(VotesResponseModel getVotesResponse)
        {
            if (getVotesResponse == null)
            {
                logger.Debug("No vote data, disabling arrows...");
                currentSongVote = VoteState.NO_VOTE;
                upVoteComponent.UpdateUI(false, false, "");
                downVoteComponent.UpdateUI(false, false, "");
            }
            else
            {
                logger.Debug("Updating UI with returned vote info...");
                currentSongVote = getVotesResponse.MyVote();
                upVoteComponent.UpdateUI(
                    true,
                    getVotesResponse.MyVote() == VoteState.VOTED_UP,
                    string.Format("{0}", getVotesResponse.UpVoteCount)
                );
                downVoteComponent.UpdateUI(
                    true,
                    getVotesResponse.MyVote() == VoteState.VOTED_DOWN,
                    string.Format("{0}", getVotesResponse.DownVoteCount)
                );
            }
        }

        private IEnumerator EnsureUIExists()
        {
            if (!upVoteComponent.IsUiCreated || !downVoteComponent.IsUiCreated)
            {
                logger.Msg("Initializing UI...");

                // Find existing pieces
                var rootGO = GameObject.Find("CentralPanel/Song Selection/VisibleWrap");

                var controlsGO = rootGO.transform.Find("Controls");
                var volumeText = controlsGO.Find("MuteWrap/VALUE");
                var volumeLeft = controlsGO.Find("MuteWrap/Arrow UP");
                var volumeRight = controlsGO.Find("MuteWrap/Arrow Down");

                var selectedTrackGO = rootGO.transform.Find("Selected Track");
                var difficultiesGO = selectedTrackGO.transform.Find("Difficulties");
                Transform hardButton = difficultiesGO.Find("StandardButton - Hard");
                Transform customButton = difficultiesGO.Find("StandardButton - Custom");

                // Create new pieces
                upVoteComponent.CreateUI(selectedTrackGO, difficultiesGO, hardButton, volumeLeft, volumeText.gameObject);
                downVoteComponent.CreateUI(selectedTrackGO, difficultiesGO, customButton, volumeRight, volumeText.gameObject);

                logger.Msg("Done creating UI");
            }

            yield return null;
        }
    }
}
