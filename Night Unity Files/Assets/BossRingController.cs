using System.Collections.Generic;
using DG.Tweening;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

public class BossRingController : MonoBehaviour
{
    private static GameObject _bossPrefab;
    private readonly List<BossRing> _rings = new List<BossRing>();

    private void Awake()
    {
        _rings.Add(new BossRing("Ring 5", gameObject, 10));
        _rings.Add(new BossRing("Ring 4", gameObject, 8));
        _rings.Add(new BossRing("Ring 3", gameObject, 6));
        _rings.Add(new BossRing("Ring 2", gameObject, 4));
        _rings.Add(new BossRing("Ring 1", gameObject, 2.5f));
    }

    public static void Create()
    {
        if (_bossPrefab == null) _bossPrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Boss Rings");
        Instantiate(_bossPrefab, Vector2.zero, Quaternion.identity);
    }

    private class BossRing
    {
        private readonly SpriteRenderer _ring, _glow;
        private readonly float _targetDistance;
        private bool _shown;

        public BossRing(string name, GameObject parent, float targetDistance)
        {
            _ring = parent.FindChildWithName<SpriteRenderer>(name);
            _glow = _ring.gameObject.FindChildWithName<SpriteRenderer>("Glow");
            _ring.SetAlpha(0.2f);
            _glow.SetAlpha(0f);
            _targetDistance = targetDistance;
        }

        public void Update(float playerDistance)
        {
            if (_shown) return;
            if (playerDistance > _targetDistance) return;
            TombPortalBehaviour.ActivateRing();
            _shown = true;
            _glow.SetAlpha(1f);
            _glow.DOFade(0f, 1f);
            _ring.SetAlpha(0.8f);
            _ring.DOFade(0.2f, 2f);
        }
    }

    private void Update()
    {
        float playerDistance = PlayerCombat.Position().magnitude;
        _rings.ForEach(r => r.Update(playerDistance));
    }
}