﻿using Il2CppInterop.Runtime.Attributes;
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
                    var rootGO = GameObject.Find("Main Stage Prefab/Z-Wrap/Multiplayer/RoomPanel/MultiplayerRoomPanel/BottomPanel");
                    logger.Msg("Root " + rootGO);
                    
                    var arrowsContainer = rootGO.transform.Find("VolumeControl");
                    //logger.Msg("ctr " + arrowsContainer);
                    var volumeText = arrowsContainer.Find("VALUE");
                    //logger.Msg("txt " + volumeText);
                    var volumeLeft = arrowsContainer.Find("Arrow Up");
                    //logger.Msg("l " + volumeLeft);
                    var volumeRight = arrowsContainer.Find("Arrow Down");
                    //logger.Msg("r " + volumeRight);

                    var favoriteWrapGO = rootGO.transform.Find("SongInfo/Favorite Wrap");
                    //logger.Msg("fav" + favoriteWrapGO);

                    // Create new pieces
                    downVoteComponent.CreateUIForHorizontal(
                        favoriteWrapGO, -1.5f, Il2CppTMPro.TextAlignmentOptions.Right, volumeRight, volumeText.gameObject
                    );
                    upVoteComponent.CreateUIForHorizontal(
                        favoriteWrapGO, 1.5f, Il2CppTMPro.TextAlignmentOptions.Left, volumeLeft, volumeText.gameObject
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
