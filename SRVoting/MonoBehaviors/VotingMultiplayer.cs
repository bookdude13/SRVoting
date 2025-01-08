using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using SRModCore;
using System;
using System.Collections;
using UnityEngine;

namespace SRVoting.MonoBehaviors
{
    [RegisterTypeInIl2Cpp]
    public class VotingMultiplayer: VotingMonoBehavior
    {
        public VotingMultiplayer(IntPtr ptr) : base(ptr) { }

        protected override System.Collections.IEnumerator EnsureUIExists()
        {
            var elementsCreated = upVoteComponent.IsUiCreated && downVoteComponent.IsUiCreated;
            if (!elementsCreated)
            {
                logger.Msg("Initializing Multiplayer UI...");

                try
                {
                    // Find existing pieces
                    var rootGO = GameObject.Find("Main Stage Prefab/Z-Wrap/Multiplayer/RoomPanel/Scale Wrap/MultiplayerRoomPanel/MainPanel/BottomPanel");
                    logger.Msg("Root " + rootGO);
                    
                    var arrowsContainer = rootGO.transform.Find("VolumeControl");
                    var volumeText = arrowsContainer.Find("VALUE");
                    var volumeLeft = arrowsContainer.Find("Arrow Up");
                    var volumeRight = arrowsContainer.Find("Arrow Down");
                    var infoWrapGO = rootGO.transform.Find("Song Info Wrap");
                    
                    // Horizontal layout is centered within the bottom panel, left to right as follows:
                    // - button, down votes, up votes, + button
                    // Make sure to leave enough room for large numbers of votes to not have the text overlap

                    var centerOffsetFromReference = new Vector3(0.0f, 1.6f, 0f);

                    var downButtonOffset = new Vector3(-2.4f, 0.0f, 0.0f);
                    var downTextOffset = new Vector3(-0.4f, 0.0f, 0.0f);

                    var upTextOffset = new Vector3(0.1f, 0.0f, 0.0f);
                    var upButtonOffset = new Vector3(2.1f, 0.0f, 0.0f);

                    // Create new pieces
                    downVoteComponent.CreateUIForHorizontal(
                        infoWrapGO, centerOffsetFromReference, downButtonOffset, downTextOffset, Il2CppTMPro.TextAlignmentOptions.Left, volumeRight, volumeText.gameObject
                    );
                    upVoteComponent.CreateUIForHorizontal(
                        infoWrapGO, centerOffsetFromReference, upButtonOffset, upTextOffset, Il2CppTMPro.TextAlignmentOptions.Right, volumeLeft, volumeText.gameObject
                    );

                    logger.Msg("Done creating UI");
                }
                catch (Exception ex)
                {
                    logger.Error("Failed to initialize UI", ex);
                }
            }

            yield return null;
        }

        protected override bool WaitForSongSelection => false;

        protected override IEnumerator UpdateVoteUI()
        {
            var countdownTimerWrap = GameObject.Find("Multiplayer/RoomPanel/Rooms/BottomPanel/SongInfo/TimeWrap");
            if (countdownTimerWrap != null && countdownTimerWrap.activeInHierarchy)
            {
                logger.Msg("Moving countdown timer wrap");
                // Originally at <2.4, 0.4, 0.0>
                countdownTimerWrap.transform.localPosition = new Vector3(4.4f, 0.4f, 0.0f);
            }

            return base.UpdateVoteUI();
        }
    }
}
