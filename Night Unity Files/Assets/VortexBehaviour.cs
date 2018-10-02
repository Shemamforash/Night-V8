using System.Collections;
using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

public class VortexBehaviour : MonoBehaviour
{
    private static readonly ObjectPool<VortexBehaviour> _vortexPool = new ObjectPool<VortexBehaviour>("Vortices", "Prefabs/Combat/Effects/Vortex");
    private ParticleSystem[] _particleSystems;
    private Vector2 _position;

    private void Awake()
    {
        _particleSystems = transform.GetComponentsInChildren<ParticleSystem>();
    }

    public static void Create(Vector2 position)
    {
        VortexBehaviour vortex = _vortexPool.Create();
        vortex.Initialise(position);
    }

    public void Initialise(Vector2 position)
    {
        _position = position;
        transform.position = _position;
        foreach (ParticleSystem system in _particleSystems)
        {
            system.Play();
        }

        StartCoroutine(WaitAndDie());
    }

    private IEnumerator WaitAndDie()
    {
        float time = 1f;
        while (time > 0f)
        {
            List<ITakeDamageInterface> charactersInRange = CombatManager.GetCharactersInRange(_position, 2f);
            charactersInRange.Remove(PlayerCombat.Instance);
            charactersInRange.ForEach(c =>
            {
                CharacterCombat character = c as CharacterCombat;
                if (character == null) return;
                Vector2 direction = (_position - (Vector2) character.transform.position).normalized;
                float distance = direction.magnitude;
                character.MovementController.KnockBack(direction * distance * 3);
            });
            time -= Time.deltaTime;
            yield return null;
        }

        Explosion.CreateExplosion(_position, 25, 0.5f).InstantDetonate();
        _vortexPool.Return(this);
    }

    private void OnDestroy()
    {
        _vortexPool.Dispose(this);
    }
}