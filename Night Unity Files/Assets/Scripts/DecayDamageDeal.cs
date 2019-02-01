using System.Collections.Generic;
using Game.Combat.Misc;
using UnityEngine;

public class DecayDamageDeal : MonoBehaviour
{
    private readonly List<CanTakeDamage> _ignoreTargets = new List<CanTakeDamage>();

    public void Clear()
    {
        _ignoreTargets.Clear();
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        CharacterCombat character = other.GetComponent<CharacterCombat>();
        if (character == null) return;
        if (_ignoreTargets.Contains(other.GetComponent<CanTakeDamage>())) return;
        character.Decay();
        _ignoreTargets.Add(character);
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