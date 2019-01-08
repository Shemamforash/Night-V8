using System.Collections.Generic;
using Game.Combat.Generation.Shrines;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

public class RiteColliderBehaviour : MonoBehaviour
{
    private RiteShrineBehaviour _riteShrine;
    private ParticleSystem _particles;
    private ParticleSystem[] _candles;
    private readonly Color _invisibleColor = new Color(1, 1, 1, 0f);
    private readonly Color _candleMax = new Color(1f, 0.6f, 0f, 0.4f);
    private readonly Color _candleMin = new Color(1f, 0.6f, 0f, 0.1f);
    private static readonly List<RiteColliderBehaviour> _riteColliders = new List<RiteColliderBehaviour>();

    public void Awake()
    {
        _riteShrine = transform.parent.parent.GetComponent<RiteShrineBehaviour>();
        _particles = GetComponent<ParticleSystem>();
        _riteColliders.Add(this);
        _candles = gameObject.FindChildWithName("Candles").GetComponentsInChildren<ParticleSystem>();
    }

    public void OnDestroy()
    {
        _riteColliders.Remove(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _riteShrine.EnterShrineCollider(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _riteShrine.ExitShrineCollider();
    }

    public void Update()
    {
        float nearestDistance = 1000;
        RiteColliderBehaviour nearest = null;
        float distanceToPlayer = 0;
        foreach (RiteColliderBehaviour riteCollider in _riteColliders)
        {
            distanceToPlayer = riteCollider.transform.Distance(PlayerCombat.Position());
            if (distanceToPlayer > 2f) continue;
            if (distanceToPlayer > nearestDistance) continue;
            nearestDistance = distanceToPlayer;
            nearest = riteCollider;
        }

        Debug.Log(nearest + " " + distanceToPlayer + " " + this);
        Color particleColour = _invisibleColor;
        Color candleColour = _candleMin;
        if (nearest == this)
        {
            float normalisedDistance = 1 - distanceToPlayer / 2f;
            particleColour = Color.Lerp(_invisibleColor, Color.white, normalisedDistance);
            candleColour = Color.Lerp(_candleMax, _candleMax, normalisedDistance);
        }

        ParticleSystem.MainModule main = _particles.main;
        main.startColor = particleColour;

        foreach (ParticleSystem candle in _candles)
        {
            main = candle.main;
            main.startColor = candleColour;
        }
    }
}