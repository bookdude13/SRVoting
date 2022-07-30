using System.Collections;
using UnityEngine;

namespace SRVoting.MonoBehaviors
{
    public class VotingMainMenu : VotingMonoBehavior
    {
        protected override Synth.Retro.Game_Track_Retro GetSelectedTrack()
        {
            return Synth.SongSelection.SongSelectionManager.GetInstance?.SelectedGameTrack;
        }

        protected override IEnumerator EnsureUIExists()
        {
            if (!upVoteComponent.IsUiCreated || !downVoteComponent.IsUiCreated)
            {
                logger.Msg("Initializing Main Menu UI...");

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
                upVoteComponent.CreateUIForVertical(selectedTrackGO, difficultiesGO, hardButton, volumeLeft, volumeText.gameObject);
                downVoteComponent.CreateUIForVertical(selectedTrackGO, difficultiesGO, customButton, volumeRight, volumeText.gameObject);

                logger.Msg("Done creating UI");
            }

            yield return null;
        }
    }
}
