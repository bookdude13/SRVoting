using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRVoting.Util
{
    public class SRScene
    {
        private static readonly string SCENE_NAME_GAME_START = "GameStart";
        private static readonly string SCENE_NAME_WARNING = "0.AWarning";
        private static readonly string SCENE_NAME_GAME_END = "3.GameEnd";

        //TODO experience?
        public enum SRSceneType
        {
            GAME_START,
            WARNING,
            MAIN_MENU,
            NORMAL_STAGE,
            SPIN_STAGE,
            SPIRAL_STAGE,
            GAME_END,
            OTHER
        }

        public string SceneName { get; private set; }
        public SRSceneType SceneType { get; private set; }

        public SRScene(string sceneName)
        {
            SceneName = sceneName;
            SceneType = GetSceneType(sceneName);
        }

        public bool IsStage()
        {
            return SceneType == SRSceneType.NORMAL_STAGE || SceneType == SRSceneType.SPIN_STAGE || SceneType == SRSceneType.SPIRAL_STAGE;
        }

        private SRSceneType GetSceneType(string sceneName)
        {
            if (sceneName == null)
            {
                return SRSceneType.OTHER;
            }

            if (sceneName == SCENE_NAME_GAME_START)
            {
                return SRSceneType.GAME_START;
            }

            if (sceneName == SCENE_NAME_WARNING)
            {
                return SRSceneType.WARNING;
            }

            var mainMenuScenes = new List<string>()
            {
                "01.The Room",
                "02.The Void",
                "03.Roof Top",
                "04.The Planet",
                "SongSelection"
            };
            if (mainMenuScenes.Contains(sceneName))
            {
                return SRSceneType.MAIN_MENU;
            }

            string subName = "";
            if (sceneName.Contains("."))
            {
                subName = sceneName.Split('.')[1];
            }

            bool isNormalStage = subName.StartsWith("Stage");
            if (isNormalStage)
            {
                return SRSceneType.NORMAL_STAGE;
            }

            bool isSpinStage = subName.StartsWith("Static Stage");
            if (isSpinStage)
            {
                return SRSceneType.SPIN_STAGE;
            }

            bool isSpiralStage = subName.StartsWith("Spiral Stage");
            if (isSpiralStage)
            {
                return SRSceneType.SPIRAL_STAGE;
            }

            if (sceneName == SCENE_NAME_GAME_END)
            {
                return SRSceneType.GAME_END;
            }

            return SRSceneType.OTHER;
        }
    }
}
