using System;
using System.Collections;
using UnityEngine;

namespace SRVoting.MonoBehaviors
{
    public class VotingMultiplayer: VotingMonoBehavior
    {
        protected override IEnumerator EnsureUIExists()
        {
            if (!upVoteComponent.IsUiCreated || !downVoteComponent.IsUiCreated)
            {
                logger.Msg("Initializing Multiplayer UI...");

                try
                {
                    // Find existing pieces
                    var rootGO = GameObject.Find("Multiplayer/RoomPanel/Rooms/BottomPanel");

                    var arrowsContainerGO = rootGO.transform.Find("MuteWrap");
                    var volumeText = arrowsContainerGO.Find("VALUE");
                    var volumeLeft = arrowsContainerGO.Find("Arrow UP");
                    var volumeRight = arrowsContainerGO.Find("Arrow Down");

                    var favoriteWrapGO = rootGO.transform.Find("Favorite Wrap");

                    // Create new pieces
                    downVoteComponent.CreateUIForHorizontal(favoriteWrapGO, -1.5f, volumeRight, volumeText.gameObject);
                    upVoteComponent.CreateUIForHorizontal(favoriteWrapGO, 1.5f, volumeLeft, volumeText.gameObject);

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
