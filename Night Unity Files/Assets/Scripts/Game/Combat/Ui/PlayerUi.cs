using System.Collections;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Combat.Player;
using UnityEngine;

namespace Game.Combat.Ui
{
    public class PlayerUi : CharacterUi
    {
        private static PlayerUi _instance;
        private float _currentAlpha;
        private Coroutine _fadeInCoroutine, _fadeOutCoroutine;
        private bool _shown;
        private const float RayDistance = 20f;
        private const int LayerMask = (1 << 8) | (1 << 10);
        private Transform _playerTransform;

        public override void Awake()
        {
            base.Awake();
            _instance = this;
            _currentAlpha = CanvasGroup.alpha;
        }

        public void Start()
        {
            _playerTransform = PlayerCombat.Instance.transform;
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
            Vector3 forwardDir = _playerTransform.up;
            Vector3 start = _playerTransform.position + forwardDir * 0.1f;
            RaycastHit2D hit = Physics2D.Raycast(start, forwardDir, RayDistance, LayerMask);
            if (hit.collider != null && hit.collider.CompareTag("UiCollider"))
            {
                EnemyBehaviour enemyBehaviour = hit.collider.transform.parent.GetComponent<EnemyBehaviour>();
                PlayerCombat.Instance.SetTarget(enemyBehaviour);
            }
            else
            {
                PlayerCombat.Instance.SetTarget(null);
            }
        }
    }
}