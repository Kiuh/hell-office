using Assets.Scripts.Menu;
using UnityEngine;

namespace Level
{
    [AddComponentMenu("Scripts/Level/Level.ConfigHandler")]
    public class ConfigHandler : MonoBehaviour
    {
        [SerializeField]
        private LevelsConfig levelsConfig;

        public Config.LevelConfig Config => levelsConfig.LevelsInfo[levelsConfig.CurrentSelectedLevel].Config;
    }
}
