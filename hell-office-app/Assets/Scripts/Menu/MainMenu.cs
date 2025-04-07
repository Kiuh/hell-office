using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Menu
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField]
        private LevelsConfig levelsConfig;

        [SerializeField]
        private string scene;

        private void Start()
        {
            var buttons = GameObject.FindObjectsByType<LevelButton>(FindObjectsSortMode.None);
            foreach (var button in buttons)
            {
                if (button.Level == 1)
                {
                    button.Unlock();
                    continue;
                }

                if (levelsConfig.LevelsInfo[button.Level - 2].Is_passed)
                {
                    button.Unlock();
                }
            }
        }

        public void LoadLevel(int level)
        {
            levelsConfig.CurrentSelectedLevel = level - 1;
            SceneManager.LoadScene(scene);
        }
    }
}
