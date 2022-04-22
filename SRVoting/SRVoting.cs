using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MelonLoader;
using SRVoting.Services;
using SRVoting.Util;
using UnityEngine;
using UnityEngine.UI;

namespace SRVoting
{
    public class SRVoting : MelonMod
    {
        private static Util.ILogger logger;
        private SynthriderzService synthriderzService;
        private VotingMonoBehavior votingBehavior;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            logger = new MelonLoggerWrapper(LoggerInstance);
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
                var votingGO = new GameObject("srvoting_main");
                votingBehavior = votingGO.AddComponent<VotingMonoBehavior>();
                votingBehavior.Init(logger, synthriderzService);

                Synth.Data.SongsProvider.GetInstance.ItemClicked += (index) => { SongChanged(); };
                Synth.Data.SongsProvider.GetInstance.ItemsLoaded += (totalSongs) => { SongChanged(); };
            }
        }

        private void SongChanged()
        {
            logger.Msg("SongChanged");
            votingBehavior.RefreshVotes();
            logger.Msg("-----------");
        }
    }
}
