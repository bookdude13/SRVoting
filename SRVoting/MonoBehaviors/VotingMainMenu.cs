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

                // MP?
                // Multiplayer/RoomPanel/HandMenu/VisibleWrap/PositionWrap/RotationWrap/Panel/Canvas/Track Preview Panel/Volume Wrap/Arrow UP
                // Volume Wrap/Arrow Down

                // MultiplayerRoomPanel/BottomPanel/VolumeControl/Arrow Up
                // Arrow Down

                // Find existing pieces
                var songSelectPanel = GameObject.Find("Main Stage Prefab/Z-Wrap/SongSelection/SelectionSongPanel");
                var rightPanelGO = songSelectPanel.transform.Find("CentralPanel/Song Selection/VisibleWrap/Main Background Image/DetailsPanel(Right)/Sectional BG - Details");
                var controlsGO = rightPanelGO.transform.Find("Controls-Buttons/PreviewVolumeControl");
                var volumeText = controlsGO.Find("VolValue");
                var volumeLeft = controlsGO.Find("Volume Down");
                var volumeRight = controlsGO.Find("Volume UP");

                var selectedTrackGO = rightPanelGO.transform.Find("Selected Track");
                var difficultiesGO = selectedTrackGO.transform.Find("Difficulties");
                Transform hardButton = difficultiesGO.Find("StandardButton - Hard");
                Transform customButton = difficultiesGO.Find("StandardButton - Custom");

                // Create new pieces
                upVoteComponent.CreateUIForVertical(selectedTrackGO, difficultiesGO, hardButton, volumeRight, volumeText.gameObject);
                downVoteComponent.CreateUIForVertical(selectedTrackGO, difficultiesGO, customButton, volumeLeft, volumeText.gameObject);

                logger.Msg("Done creating main menu UI");
            }

            yield return null;
        }
    }
}
