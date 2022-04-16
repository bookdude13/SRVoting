using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using SRVoting.Auth;
using SRVoting.Util;

namespace SRVoting
{
    public class SRVoting : MelonMod
    {
        private ILogger logger;

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
                // After some things have initialized, but before main scene, try to authenticate
                logger.Msg("Logging into Synthriderz...");
                var steamAuthService = new SteamAuthService(logger);
                steamAuthService.LoginAsync(token => {
                    logger.Msg("Steam token: " + token);
                });
            }

            if (scene.SceneType == SRScene.SRSceneType.MAIN_MENU)
            {
                logger.Msg("MainMenu loaded, looking for vote buttons");
            }
        }
    }
}
