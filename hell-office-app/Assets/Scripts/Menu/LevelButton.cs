using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menu
{
    public class LevelButton : MonoBehaviour
    {
        [SerializeField]
        Button button;

        [SerializeField]
        Image lock_image;

        [SerializeField]
        private int level;

        [SerializeField]
        private MainMenu main_menu;

        public int Level => level;

        private void Awake()
        {
            button.onClick.AddListener(Clicked);
        }

        private void Clicked()
        {
            main_menu.LoadLevel(level);
        }

        public void Unlock()
        {
            lock_image.gameObject.SetActive(false);
        }
    }
}
