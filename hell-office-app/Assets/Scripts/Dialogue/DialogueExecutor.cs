using CameraController;
using DG.Tweening;
using Level;
using TileUnion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Dialogue
{
    public class DialogueExecutor : MonoBehaviour
    {
        [SerializeField]
        private Executor executor;

        [SerializeField]
        private Button skip_button;

        [SerializeField]
        private TMP_Text dialogue_text;

        [SerializeField]
        private Logic cam_logic;

        [SerializeField]
        private Image image;

        DialogueConfig config;
        int current_index = 0;
        Tween lerp_tween;

        private void Awake()
        {
            skip_button.onClick.AddListener(Skip);
        }

        public void Skip()
        {
            if (lerp_tween != null)
            {
                lerp_tween.Kill(true);
                return;
            }
            else if (config.Dialogues.Count > current_index)
            {
                CreateSpeech();
                return;
            }
            else
            {
                cam_logic.IsFixedMode = false;
                executor.CompleteDialogue();
            }
        }

        public void ExecuteDialogue(DialogueConfig config)
        {
            cam_logic.IsFixedMode = true;
            cam_logic.TargetPosition = FindAnyObjectByType<MeetingRoomLogics>().GetComponent<Transform>();
            current_index = 0;
            this.config = config;
            image.sprite = config.CharacterIcon;
            CreateSpeech();
        }

        public void CreateSpeech()
        {
            var len = config.Dialogues[current_index].Length;
            lerp_tween = DOVirtual.Int(0, len, len * config.PerCharSpeed, (val) =>
            {
                var chars = ((float)val / (float)len) * len;
                dialogue_text.SetText(config.Dialogues[current_index].Substring(0, (int)chars));
            }).SetUpdate(true).SetEase(Ease.Linear).OnComplete(() =>
            {
                dialogue_text.SetText(config.Dialogues[current_index]);
                current_index = current_index + 1;
                lerp_tween = null;
            });
        }
    }
}
