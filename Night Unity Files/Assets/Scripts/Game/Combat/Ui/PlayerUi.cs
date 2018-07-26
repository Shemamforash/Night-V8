using System.Collections.Generic;
using DG.Tweening;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;

namespace Game.Combat.Ui
{
    public class PlayerUi : CharacterUi
    {
        private static PlayerUi _instance;
        private Coroutine _fadeInCoroutine, _fadeOutCoroutine;
        private static TextMeshProUGUI _eventText;

        public override void Awake()
        {
            base.Awake();
            _instance = this;
            _eventText = GameObject.Find("Event Text").GetComponent<TextMeshProUGUI>();
            _eventText.color = UiAppearanceController.InvisibleColour;
        }

        public static PlayerUi Instance()
        {
            if (_instance != null) return _instance;
            _instance = FindObjectOfType<PlayerUi>();
            return _instance;
        }

        public void Update()
        {
            List<CharacterCombat> chars = CombatManager.GetEnemiesInRange(PlayerCombat.Instance.transform.position, 10f);
            Vector2 playerDir = PlayerCombat.Instance.transform.up;
            float nearestAngle = 360;
            CharacterCombat nearestCharacter = null;
            chars.ForEach(c =>
            {
                if (!Helper.IsObjectInCameraView(c.gameObject)) return;
                Vector2 enemyDir = c.transform.position - PlayerCombat.Instance.transform.position;
                float enemyAngle = Vector2.Angle(playerDir, enemyDir);
                if (enemyAngle > 10) return;
                if (enemyAngle > nearestAngle) return;
                nearestAngle = enemyAngle;
                nearestCharacter = c;
            });

            PlayerCombat.Instance.SetTarget(nearestCharacter as EnemyBehaviour);
        }

        public static void SetEventText(string text)
        {
            _eventText.text = text;
            FadeTextIn();
        }

        public static void SetEventText(string text, float duration)
        {
            SetEventText(text);
            FadeTextOut(1, 1);
        }

        public static void FadeTextIn()
        {
            _eventText.DOColor(UiAppearanceController.FadedColour, 1f);
        }

        public static void FadeTextOut(float duration = 1f, float pause = 0f)
        {
            Sequence s = DOTween.Sequence();
            s.AppendInterval(pause);
            s.Append(_eventText.DOColor(UiAppearanceController.InvisibleColour, duration));
        }
    }
}