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
        private Util.ILogger logger;
        private SynthriderzService synthriderzService;

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
                var votingBehavior = votingGO.AddComponent<VotingMonoBehavior>();
                votingBehavior.Init(logger, synthriderzService);

                // TODO instead of this, do whenever the panel updates with Harmony, to catch first update
                Synth.Data.SongsProvider.GetInstance.ItemClicked += (index) => { votingBehavior.RefreshVotes(); };
            }
        }
    }
}
