using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MelonLoader;
using SRModCore;
using SRVoting.MonoBehaviors;
using SRVoting.Services;
using Il2CppSynth.Versus.Types;
using UnityEngine;
using UnityEngine.UI;
using Il2CppSynth.Data;
using Il2Cppcom.Kluge.XR.Utils;

namespace SRVoting
{
    public class SRVoting : MelonMod
    {
        private readonly string rootMultiplayerName = "srvoting_multiplayer";

        public static SRVoting Instance { get; private set; }

        private SRLogger logger;
        private SynthriderzService synthriderzService;
        private VotingMainMenu votingBehaviorMainMenu;
        private VotingMultiplayer votingBehaviorMultiplayer;
        private GameObject rootGameObjectMultiplayer = null;


        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();

            logger = new MelonLoggerWrapper(LoggerInstance);
            Instance = this;
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            base.OnSceneWasInitialized(buildIndex, sceneName);

            SRScene scene = new SRScene(sceneName);
            logger.Msg("Scene initialized: " + sceneName + ". Type: " + scene.SceneType);

            if (scene.SceneType == SRScene.SRSceneType.WARNING)
            {
                // After some things have initialized, but before main scene, set up services (gets Steam ticket)
                logger.Msg("Setting up services...");
                var steamAuthService = new SteamAuthService(logger);
                synthriderzService = new SynthriderzService(logger, steamAuthService);
            }

            if (scene.SceneType == SRScene.SRSceneType.MAIN_MENU)
            {
                logger.Msg("MainMenu loaded, looking for vote buttons");
                logger.Msg("Setting up voting mono behavior");

                var zWrap = GameObject.Find("Main Stage Prefab/Z-Wrap");
                var votingGO = GameObject.Instantiate(new GameObject("srvoting_mainmenu"), zWrap.transform);
                logger.Msg("Created GO " + votingGO);
                votingBehaviorMainMenu = votingGO.AddComponent<VotingMainMenu>();
                logger.Msg("Added");
                votingBehaviorMainMenu.Init(logger, synthriderzService);
                logger.Msg("Initialized");

                Il2CppSynth.Data.SongsProvider.GetInstance.ItemClicked += (ListProviderEventHandler)((index) => { votingBehaviorMainMenu.Refresh(); });
                Il2CppSynth.Data.SongsProvider.GetInstance.ItemsLoaded += (ListProviderEventHandler)((totalSongs) => { votingBehaviorMainMenu.Refresh(); });

                // Refresh right away to catch the case when returning from a song
                votingBehaviorMainMenu.Refresh();
            }
        }

        public void OnOpenMultiplayerRoomMenu(Il2CppSynth.Versus.Room room)
        {
            EnsureMultiplayerObjects();
            votingBehaviorMultiplayer?.Refresh();
        }

        private void EnsureMultiplayerObjects()
        {
            if (rootGameObjectMultiplayer == null || votingBehaviorMultiplayer == null)
            {
                logger.Msg("Creating multiplayer objects");
                var zWrap = GameObject.Find("Main Stage Prefab/Z-Wrap");
                rootGameObjectMultiplayer = GameObject.Instantiate(new GameObject(rootMultiplayerName), zWrap.transform);
                votingBehaviorMultiplayer = rootGameObjectMultiplayer.AddComponent<VotingMultiplayer>();
                votingBehaviorMultiplayer.Init(logger, synthriderzService);
            }
        }

        public void OnMultiplayerRoomUpdateTrackData()
        {
            EnsureMultiplayerObjects();
            logger.Msg("Refreshing multiplayer on track data reload...");
            votingBehaviorMultiplayer?.Refresh();
        }
    }
}
