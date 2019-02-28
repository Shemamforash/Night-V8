using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Misc;
using UnityEngine;

public class FireDamageDeal : MonoBehaviour
{
    private readonly List<CanTakeDamage> _ignoreTargets = new List<CanTakeDamage>();

    public void OnTriggerStay2D(Collider2D other)
    {
        if (!CombatManager.Instance().IsCombatActive()) return;
        CanTakeDamage character = other.GetComponent<CanTakeDamage>();
        if (character == null) return;
        if (_ignoreTargets.Contains(other.GetComponent<CanTakeDamage>())) return;
        character.Burn();
    }

    protected void Clear()
    {
        _ignoreTargets.Clear();
    }

    public void AddIgnoreTarget(CanTakeDamage _ignoreTarget)
    {
        _ignoreTargets.Add(_ignoreTarget);
    }

    public void AddIgnoreTargets(List<CanTakeDamage> targetsToIgnore)
    {
        _ignoreTargets.AddRange(targetsToIgnore);
    }
}