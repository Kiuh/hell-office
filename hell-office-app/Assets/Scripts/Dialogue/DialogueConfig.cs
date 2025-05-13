using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Dialogue
{
    [Serializable]
    [CreateAssetMenu(fileName = "DialogueConfig", menuName = "Level/DialogueConfig")]
    public class DialogueConfig : ScriptableObject
    {
        private float per_char_speed = 0.05f;
        public float PerCharSpeed => per_char_speed;

        [SerializeField]
        private Sprite character_icon;
        public Sprite CharacterIcon => character_icon;

        [TextArea]
        [SerializeField]
        private List<string> dialogues;
        public List<string> Dialogues => dialogues;
    }
}
