using System;
using System.Collections;
using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

public class VortexBehaviour : MonoBehaviour
{
    private static readonly ObjectPool<VortexBehaviour> _vortexPool = new ObjectPool<VortexBehaviour>("Vortices", "Prefabs/Combat/Effects/Vortex");
    private ParticleSystem _particles;

    private Vector2 _position;
    private Action _vortexEndAction;

    private void Awake()
    {
        _particles = GetComponent<ParticleSystem>();
    }

    public static void Create(Vector2 position, Action vortexEndAction)
    {
        VortexBehaviour vortex = _vortexPool.Create();
        vortex.Initialise(position, vortexEndAction);
    }

    private void Initialise(Vector2 position, Action vortexEndAction)
    {
        _position = position;
        transform.position = _position;
        _vortexEndAction = vortexEndAction;
        _particles.Play();
        StartCoroutine(WaitAndDie());
    }

    private IEnumerator WaitAndDie()
    {
        float time = 0.75f;
        while (time > 0f)
        {
            List<CanTakeDamage> charactersInRange = CombatManager.GetCharactersInRange(_position, 2f);
            charactersInRange.Remove(PlayerCombat.Instance);
            charactersInRange.ForEach(c =>
            {
                CharacterCombat character = c as CharacterCombat;
                if (character == null) return;
                Vector2 direction = (_position - (Vector2) character.transform.position).normalized;
                float distance = direction.magnitude;
                character.MovementController.KnockBack(direction * distance * 5);
            });
            time -= Time.deltaTime;
            yield return null;
        }
        
        _vortexEndAction?.Invoke();
        _vortexPool.Return(this);
    }

    private void OnDestroy()
    {
        _vortexPool.Dispose(this);
    }
}