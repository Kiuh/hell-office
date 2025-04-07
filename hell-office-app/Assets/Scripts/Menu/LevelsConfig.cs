using System;
using System.Collections.Generic;
using Level.Config;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Assets.Scripts.Menu
{
    [Serializable]
    public class LevelInfo
    {
        [SerializeField]
        public LevelConfig Config;

        [SerializeField]
        public bool Is_passed;
    }

    [Serializable]
    [CreateAssetMenu(fileName = "LevelsConfig", menuName = "Menu/LevelsConfig")]
    public class LevelsConfig : ScriptableObject
    {
        [SerializeField]
        private List<LevelInfo> levels_info = new();
        public List<LevelInfo> LevelsInfo => levels_info;

        [SerializeField]
        private int current_selected_level;

        public int CurrentSelectedLevel
        {
            get => current_selected_level; set => current_selected_level = value;

        }

        [Button]
        private void ResetConfig()
        {
            for (int i = 0; i < levels_info.Count; i++)
            {
                levels_info[i].Is_passed = false;
            }
        }
    }
}
