using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using System;
using System.Collections;
using UnityEngine;

namespace SRVoting.MonoBehaviors
{
    [RegisterTypeInIl2Cpp]
    public class VotingMainMenu : VotingMonoBehavior
    {
        public VotingMainMenu(IntPtr ptr) : base(ptr) { }

        protected override System.Collections.IEnumerator EnsureUIExists()
        {
            if (!upVoteComponent.IsUiCreated || !downVoteComponent.IsUiCreated)
            {
                logger.Msg("Initializing Main Menu UI...");

                // Find existing pieces
                var songSelectPanel = GameObject.Find("Main Stage Prefab/Z-Wrap/SongSelection/SelectionSongPanel");
                var rightPanelGO = songSelectPanel.transform.Find("CentralPanel/Song Selection/VisibleWrap/Canvas/DetailsPanel(Right)/Sectional BG - Details");
                var controlsGO = rightPanelGO.transform.Find("Controls-Buttons/PreviewVolumeControl");
                var volumeText = controlsGO.Find("VolValue");
                var volumeLeft = controlsGO.Find("Volume Down");
                var volumeRight = controlsGO.Find("Volume UP");

                var selectedTrackGO = rightPanelGO.transform.Find("Selected Track");

                var songDetailsGO = selectedTrackGO.transform.Find("Standard Song Info/SongDetails - Stats");

                //var difficultiesGO = selectedTrackGO.transform.Find("Difficulties");
                //Transform hardButton = difficultiesGO.Find("ChipButton - Hard");
                //Transform customButton = difficultiesGO.Find("ChipButton - Custom");

                // The vertical stack, from top to bottom, goes:
                // + button
                // Up vote count text
                // Down vote count text
                // - button

                // For some reason the bottom wants to offset the button to the right, but the text is correct.
                // Adjust both to compensate (it's easier than figuring out why)
                var bottomHorizontalCompensate = 1.0f;

                var topArrowOffset = new Vector3(5.7f, 1.5f, 0f);
                var bottomArrowOffset = topArrowOffset + new Vector3(-bottomHorizontalCompensate, -2.4f, 0f);

                var topTextOffset = new Vector3(-0.5f, -0.7f, 0f);
                var bottomTextOffset = new Vector3(-0.5f + bottomHorizontalCompensate, 0.7f, 0f);

                // Create new pieces
                upVoteComponent.CreateUIForVertical(selectedTrackGO, songDetailsGO, topArrowOffset, topTextOffset, volumeRight, volumeText.gameObject);
                downVoteComponent.CreateUIForVertical(selectedTrackGO, songDetailsGO, bottomArrowOffset, bottomTextOffset, volumeLeft, volumeText.gameObject);

                logger.Msg("Done creating main menu UI");
            }

            yield return null;
        }
    }
}
