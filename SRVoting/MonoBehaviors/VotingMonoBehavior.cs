using Il2CppInterop.Runtime.Attributes;
using Il2CppMiKu.NET.Charting;
using MelonLoader;
using SRModCore;
using SRVoting.Models;
using SRVoting.Services;
using SRVoting.UI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace SRVoting.MonoBehaviors
{
    [RegisterTypeInIl2Cpp]
    public abstract class VotingMonoBehavior : MonoBehaviour
    {
        protected readonly string arrowUpName = "srvoting_voteArrowUp";
        protected readonly string arrowDownName = "srvoting_voteArrowDown";

        protected SRLogger logger;
        protected SynthriderzService synthriderzService;

        protected VoteDirectionComponent upVoteComponent;
        protected VoteDirectionComponent downVoteComponent;

        private string currentSongHash = "";
        private VoteState currentSongVote = VoteState.NO_VOTE;

        public VotingMonoBehavior(IntPtr ptr) : base(ptr) { }

        public void Init(SRLogger logger, SynthriderzService synthriderzService)
        {
            this.logger = logger;
            this.synthriderzService = synthriderzService;

            upVoteComponent = new VoteDirectionComponent(
                logger,
                arrowUpName,
                new Action(() => Vote(arrowUpName))
            );
            downVoteComponent = new VoteDirectionComponent(
                logger,
                arrowDownName,
                new Action(() => Vote(arrowDownName))
            );

            MelonCoroutines.Start(EnsureUIExists());
        }

        protected abstract System.Collections.IEnumerator EnsureUIExists();

        public void Refresh()
        {
            logger.Msg("Refreshing UI");
            MelonCoroutines.Start(UpdateVoteUI());
        }

        protected IEnumerator WaitForSongSelectionOpen()
        {
            var ssmInstance = Il2CppSynth.SongSelection.SongSelectionManager.GetInstance;
            while (ssmInstance == null || !ssmInstance.SongSelectionOpenComplete)
            {
                yield return null;
            }
        }

        protected IEnumerator WaitForSelectedTrack()
        {
            var ssmInstance = Il2CppSynth.SongSelection.SongSelectionManager.GetInstance;
            while (ssmInstance == null || ssmInstance.SelectedGameTrack == null)
            {
                yield return null;
            }
        }

        Il2CppSynth.Retro.Game_Track_Retro GetSelectedTrack()
        {
            var ssmInstance = Il2CppSynth.SongSelection.SongSelectionManager.GetInstance;
            if (ssmInstance == null)
            {
                logger.Msg("Null ssm instance!");
                return null;
            }

            if (!ssmInstance.songSelectionOpenComplete)
            {
                logger.Msg("Not done opening song selection!");
                return null;
            }

            var selectedTrack = ssmInstance?.SelectedGameTrack;
            return selectedTrack;
        }

        protected virtual System.Collections.IEnumerator UpdateVoteUI()
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
            yield return WaitForSongSelectionOpen();
            logger.Debug("Waiting for selected track");
            yield return WaitForSelectedTrack();

            logger.Debug("Getting selected track...");
            var selectedTrack = GetSelectedTrack();
            string songName = selectedTrack?.TrackName ?? "";
            currentSongHash = selectedTrack?.LeaderboardHash ?? "";
            bool isCustom = selectedTrack?.IsCustomSong ?? false;

            if (songName == "" || currentSongHash == "")
            {
                logger.Msg($"No song selected. Name {songName}, hash {currentSongHash}");
                upVoteComponent.UpdateUI(false, false, "");
                downVoteComponent.UpdateUI(false, false, "");
            }
            else if (!isCustom)
            {
                logger.Msg($"OST track '{songName}' selected. Hash: {currentSongHash}");
                upVoteComponent.UpdateUI(false, false, "N/A");
                downVoteComponent.UpdateUI(false, false, "N/A");
            }
            else
            {
                logger.Msg($"Custom song '{songName}' selected. Hash: {currentSongHash}");
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

        private void Vote(string senderName)
        {
            // Translate sender into up/down
            VoteState newVote = senderName == arrowUpName ? VoteState.VOTED_UP : VoteState.VOTED_DOWN;

            // If same as existing, "unvote", otherwise cast desired vote
            VoteState voteToSend = newVote == currentSongVote ? VoteState.NO_VOTE : newVote;

            // Disable events until done voting
            upVoteComponent.DisableEvents();
            downVoteComponent.DisableEvents();
            
            logger.Debug($"Voting for song {currentSongHash}. Old: {currentSongVote}, New: {voteToSend}");
            MelonCoroutines.Start(VoteAndUpdateUI(voteToSend));
        }

        private System.Collections.IEnumerator VoteAndUpdateUI(VoteState vote)
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
                logger.Msg("No vote data, disabling arrows...");
                currentSongVote = VoteState.NO_VOTE;
                upVoteComponent.UpdateUI(false, false, "");
                downVoteComponent.UpdateUI(false, false, "");
            }
            else
            {
                logger.Msg("Updating UI with returned vote info...");
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
    }
}
