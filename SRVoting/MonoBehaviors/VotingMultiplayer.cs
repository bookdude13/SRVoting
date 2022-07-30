using System;
using System.Collections;
using UnityEngine;

namespace SRVoting.MonoBehaviors
{
    public class VotingMultiplayer: VotingMonoBehavior
    {
        protected override Synth.Retro.Game_Track_Retro GetSelectedTrack()
        {
            return null;
        }

        protected override IEnumerator EnsureUIExists()
        {
            if (!upVoteComponent.IsUiCreated || !downVoteComponent.IsUiCreated)
            {
                logger.Msg("Initializing Multiplayer UI...");

                try
                {
                    // Find existing pieces
                    var rootGO = GameObject.Find("Multiplayer/RoomPanel/Rooms/BottomPanel");
                    logger.Msg("Root GO? " + (rootGO != null));
                    var arrowsContainerGO = rootGO.transform.Find("MuteWrap");
                    logger.Msg("MuteWrap GO? " + (arrowsContainerGO != null));
                    var volumeText = arrowsContainerGO.Find("VALUE");
                    logger.Msg("volume text? " + (volumeText != null));
                    var volumeLeft = arrowsContainerGO.Find("Arrow UP");
                    logger.Msg("volume left? " + (volumeLeft != null));
                    var volumeRight = arrowsContainerGO.Find("Arrow Down");
                    logger.Msg("volume right? " + (volumeRight != null));

                    var favoriteWrapGO = rootGO.transform.Find("Favorite Wrap");
                    logger.Msg("favorite wrap? " + (favoriteWrapGO != null));

                    // Create new pieces
                    logger.Msg("Creating down");
                    downVoteComponent.CreateUIForHorizontal(favoriteWrapGO, -1.0f, volumeRight, volumeText.gameObject);
                    logger.Msg("Creating up");
                    upVoteComponent.CreateUIForHorizontal(favoriteWrapGO, 1.0f, volumeLeft, volumeText.gameObject);

                    logger.Msg("Done creating UI");
                }
                catch (Exception ex)
                {
                    logger.Error("Failed to initialize UI", ex);
                }
            }

            yield return null;
        }
    }
}
