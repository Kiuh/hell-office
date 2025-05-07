using Level;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Level.Aims
{
    public class Aims : MonoBehaviour
    {
        [SerializeField]
        ConfigHandler configHandler;

        [SerializeField]
        private TMP_Text daysRemainingText;

        [SerializeField]
        private TMP_Text soulsNeeded;

        [ReadOnly]
        [SerializeField]
        private int days_remaining;

        public int DaysRemaining
        {
            get => days_remaining;
            set => days_remaining = value;
        }

        private void Awake()
        {
            days_remaining = configHandler.Config.DaysGiven;
        }

        private void Update()
        {
            daysRemainingText.SetText($"{DaysRemaining} hell days remaining");
            soulsNeeded.SetText($"{configHandler.Config.SoulsNeed} souls needed");
        }
    }
}
