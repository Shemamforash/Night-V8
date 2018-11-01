using System.Collections.Generic;
using Game.Combat.Misc;
using UnityEngine;

public class DecayDamageDeal : MonoBehaviour
{
    private CircleCollider2D _collider;
    private readonly List<CanTakeDamage> _ignoreTargets = new List<CanTakeDamage>();

    public void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
    }

    public void SetRadius(float radius)
    {
        _ignoreTargets.Clear();
        _collider.radius = radius;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        CharacterCombat character = other.GetComponent<CharacterCombat>();
        if (character == null) return;
        if (_ignoreTargets.Contains(other.GetComponent<CanTakeDamage>())) return;
        character.Decay();
    }

    public void AddIgnoreTarget(CanTakeDamage ignoreTarget)
    {
        _ignoreTargets.Add(ignoreTarget);
    }

    public void AddIgnoreTargets(List<CanTakeDamage> targetsToIgnore)
    {
        _ignoreTargets.AddRange(targetsToIgnore);
    }
}