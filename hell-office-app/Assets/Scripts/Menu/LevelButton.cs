using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Menu
{
    public class LevelButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        Button button;

        [SerializeField]
        TMP_Text text;

        [SerializeField]
        Color unlock_color;

        [SerializeField]
        private int level;

        [SerializeField]
        private MainMenu main_menu;

        private Vector3 originalScale = Vector3.one;

        public int Level => level;

        private void Awake()
        {
            button.onClick.AddListener(Clicked);
            button.interactable = false;
        }

        private void Clicked()
        {
            main_menu.LoadLevel(level);
        }

        public void Unlock()
        {
            text.color = unlock_color;
            button.interactable = true;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button.interactable)
                _ = transform.DOScale(originalScale * 1.1f, 0.2f).SetEase(Ease.OutBack); // Scale up smoothly
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (button.interactable)
                _ = transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutBack); // Reset scale smoothly
        }
    }
}
