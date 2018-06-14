using System.Collections;
using System.Collections.Generic;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Ui
{
    public class PlayerUi : CharacterUi
    {
        private static PlayerUi _instance;
        private float _currentAlpha;
        private Coroutine _fadeInCoroutine, _fadeOutCoroutine;
        private bool _shown;

        public override void Awake()
        {
            base.Awake();
            _instance = this;
            _currentAlpha = CanvasGroup.alpha;
        }

        public static PlayerUi Instance()
        {
            if (_instance != null) return _instance;
            _instance = FindObjectOfType<PlayerUi>();
            return _instance;
        }

        public void Hide()
        {
            if (_fadeInCoroutine != null) StopCoroutine(_fadeInCoroutine);
            _fadeOutCoroutine = StartCoroutine(FadeOut());
            _shown = false;
        }

        public void Show()
        {
            if (_fadeOutCoroutine != null) StopCoroutine(_fadeOutCoroutine);
            _fadeInCoroutine = StartCoroutine(FadeIn());
            _shown = true;
        }

        private IEnumerator FadeIn()
        {
            while (_currentAlpha < 1)
            {
                _currentAlpha += Time.deltaTime;
                if (_currentAlpha > 1) _currentAlpha = 1;
                SetAlpha(_currentAlpha);
                yield return null;
            }
        }

        private IEnumerator FadeOut()
        {
            while (_currentAlpha > 0)
            {
                _currentAlpha -= Time.deltaTime;
                if (_currentAlpha < 0) _currentAlpha = 0;
                SetAlpha(_currentAlpha);
                yield return null;
            }
        }

        public void Update()
        {
            if (!_shown) return;

            List<CharacterCombat> chars = CombatManager.GetCharactersInRange(transform.position, 10f);
            Vector2 playerDir = transform.up;
            float nearestAngle = 360;
            CharacterCombat nearestCharacter = null;
            chars.ForEach(c =>
            {
                if (c == PlayerCombat.Instance) return;
                if (!Helper.IsObjectInCameraView(c.gameObject)) return;
                Vector2 enemyDir = c.transform.position - transform.position;
                float enemyAngle = Vector2.Angle(playerDir, enemyDir);
                if (enemyAngle > 5) return;
                if (enemyAngle > nearestAngle) return;
                nearestAngle = enemyAngle;
                nearestCharacter = c;
            });

            PlayerCombat.Instance.SetTarget(nearestCharacter as EnemyBehaviour);
        }
    }
}