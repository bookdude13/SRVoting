using SRVoting.Models;
using SRVoting.Services;
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
        private GameObject upArrow;
        private GameObject downArrow;
        private TMPro.TMP_Text upVoteCountText;
        private TMPro.TMP_Text downVoteCountText;

        private string currentSongHash = "";
        private VoteState currentSongVote = VoteState.NO_VOTE;

        public void Init(Util.ILogger logger, SynthriderzService synthriderzService)
        {
            this.logger = logger;
            this.synthriderzService = synthriderzService;
        }

        public void OnSongChanged()
        {
            logger.Msg("Updating UI");
            StartCoroutine(UpdateVoteUI());
        }

        private void Vote(string senderName)
        {
            // Translate sender into up/down
            VoteState newVote = senderName == arrowUpName ? VoteState.VOTED_UP : VoteState.VOTED_DOWN;

            // If same as existing, "unvote", otherwise cast desired vote
            VoteState voteToSend = newVote == currentSongVote ? VoteState.NO_VOTE : newVote;

            logger.Msg($"Voting for song {currentSongHash}. Old: {currentSongVote}, New: {voteToSend}");
            StartCoroutine(synthriderzService.Vote(
                currentSongHash,
                voteToSend,
                response => UpdateUI(response),
                errorMsg => HandleApiError(errorMsg))
            );
        }

        private IEnumerator UpdateVoteUI()
        {
            if (synthriderzService == null)
            {
                logger.Msg("No synthriderz service; skipping");
                yield return null;
            }

            logger.Msg("Song clicked; waiting for update");
            yield return new WaitForSeconds(0.01f);

            logger.Msg("Make sure UI exists...");
            yield return EnsureUIExists();

            // Disable UI until we get response, to avoid voting on other songs
            UpdateUIUp(false, false, "");
            UpdateUIDown(false, false, "");

            logger.Msg("Getting selected track...");
            var selectedTrack = Synth.SongSelection.SongSelectionManager.GetInstance?.SelectedGameTrack;
            string songName = selectedTrack?.TrackName ?? "";
            currentSongHash = selectedTrack?.LeaderboardHash ?? "";
            bool isCustom = selectedTrack?.IsCustomSong ?? false;
            bool canLeaderboard = selectedTrack?.CanUpdateLeaderboard ?? false;
            logger.Msg($"{songName} selected. IsCustom? {isCustom}. CanPostLeaderboards? {canLeaderboard}. Hash: {currentSongHash}");

            if (!isCustom)
            {
                logger.Msg("Not showing votes for OST");
                UpdateUIUp(false, false, "");
                UpdateUIDown(false, false, "");
            }
            else
            {
                yield return synthriderzService.GetVotes(
                    currentSongHash,
                    response => UpdateUI(response),
                    errorMsg => HandleApiError(errorMsg)
                );

                logger.Msg("Done updating UI");
            }
        }

        private void HandleApiError(string errorMessage)
        {
            logger.Msg(errorMessage);

            // Just hide the UI elements on error for now.
            // Eventually this could be error text or an error prompt
            UpdateUI(null);
        }

        private void UpdateUI(VotesResponseModel getVotesResponse)
        {
            if (getVotesResponse == null)
            {
                logger.Msg("No vote data, disabling arrows...");
                currentSongVote = VoteState.NO_VOTE;
                UpdateUIUp(false, false, "");
                UpdateUIDown(false, false, "");
            }
            else
            {
                logger.Msg("Updating UI with returned vote info...");
                currentSongVote = getVotesResponse.MyVote();
                UpdateUIUp(true, getVotesResponse.MyVote() == VoteState.VOTED_UP, string.Format("{0}", getVotesResponse.UpVoteCount));
                UpdateUIDown(true, getVotesResponse.MyVote() == VoteState.VOTED_DOWN, string.Format("{0}", getVotesResponse.DownVoteCount));
            }
        }

        private void UpdateUIUp(bool isActive, bool isMyVote, string text)
        {
            upVoteCountText.SetText(text);
            upVoteCountText.fontStyle = isMyVote ? TMPro.FontStyles.Underline : TMPro.FontStyles.Normal;
            upArrow.SetActive(isActive);
        }

        private void UpdateUIDown(bool isActive, bool isMyVote, string text)
        {
            downVoteCountText.SetText(text);
            downVoteCountText.fontStyle = isMyVote ? TMPro.FontStyles.Underline : TMPro.FontStyles.Normal;
            downArrow.SetActive(isActive);
        }

        private IEnumerator EnsureUIExists()
        {
            if (upArrow == null || downArrow == null || upVoteCountText == null || downVoteCountText == null)
            {
                logger.Msg("Initializing UI...");
                var rootGO = GameObject.Find("CentralPanel/Song Selection/VisibleWrap");

                var controlsGO = rootGO.transform.Find("Controls");
                var volumeText = controlsGO.Find("MuteWrap/VALUE");
                var volumeLeft = controlsGO.Find("MuteWrap/Arrow UP");
                var volumeRight = controlsGO.Find("MuteWrap/Arrow Down");

                var selectedTrackGO = rootGO.transform.Find("Selected Track");
                var difficultiesGO = selectedTrackGO.transform.Find("Difficulties");
                Transform hardButton = difficultiesGO.Find("StandardButton - Hard");
                Transform customButton = difficultiesGO.Find("StandardButton - Custom");

                // Create arrows
                var upVoteContainer = CreateVoteDirectionContainer(selectedTrackGO, difficultiesGO, hardButton);
                upArrow = CreateVoteArrow(upVoteContainer.transform, volumeLeft, arrowUpName);
                upVoteCountText = CreateVoteCountText(upVoteContainer.transform, upArrow, volumeText.gameObject);

                var downVoteContainer = CreateVoteDirectionContainer(selectedTrackGO, difficultiesGO, customButton);
                downArrow = CreateVoteArrow(downVoteContainer.transform, volumeRight, arrowDownName);
                downVoteCountText = CreateVoteCountText(downVoteContainer.transform, downArrow, volumeText.gameObject);

                logger.Msg("Done creating UI");
            }

            yield return null;
        }

        private GameObject CreateVoteDirectionContainer(
            Transform parent,
            Transform leftSideReference,
            Transform rightOffsetReference
        ) {
            var voteContainer = new GameObject("srvoting_container");
            voteContainer.transform.SetParent(parent, false);
            voteContainer.transform.localPosition = leftSideReference.localPosition + rightOffsetReference.localPosition + new Vector3(2.0f, 0.0f, 0.0f);
            voteContainer.transform.localRotation = leftSideReference.localRotation;

            return voteContainer;
        }

        private GameObject CreateVoteArrow(
            Transform voteContainer,
            Transform arrowToClone,
            string arrowName
        ) {
            // Clone arrow used to actually vote
            var voteArrow = GameObject.Instantiate(arrowToClone, voteContainer.transform);
            voteArrow.name = arrowName;
            voteArrow.localPosition = Vector3.zero;
            voteArrow.localEulerAngles = arrowToClone.localEulerAngles + new Vector3(0f, 0f, 90f);

            // Replace button event
            var buttonEvents = voteArrow.GetComponent<VRTK_InteractableObject_UnityEvents>();
            buttonEvents.OnUse.RemoveAllListeners();

            // 2 persistent listeners not removed by RemoveAllListeners() exist
            // See https://forum.unity.com/threads/documentation-unityevent-removealllisteners-only-removes-non-persistent-listeners.341796/
            // After trial and error, the one at index 0 controls volume still and needs to be disabled.
            // The one at index 1 still needs to stick around to handle new events
            buttonEvents.OnUse.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.Off);

            voteArrow.gameObject.SetActive(true);
            buttonEvents.OnUse.AddListener((sender, e) => Vote( ((VRTK.VRTK_InteractableObject)sender).name));

            logger.Msg($"Arrow {arrowName} added");
            return voteArrow.gameObject;
        }

        private TMPro.TMP_Text CreateVoteCountText(Transform voteContainer, GameObject voteArrow, GameObject textReference)
        {
            var voteCountText = GameObject.Instantiate(textReference, voteContainer);
            voteCountText.name = voteArrow.name + "_text";
            voteCountText.transform.localPosition = voteArrow.transform.localPosition + new Vector3(1.2f, 0.0f, 0.0f);
            voteCountText.transform.eulerAngles = textReference.transform.eulerAngles;

            var text = voteCountText.GetComponent<TMPro.TMP_Text>();
            text.SetText("#####");
            text.alignment = TMPro.TextAlignmentOptions.Left;

            logger.Msg("Text added");
            return text;
        }
    }
}
