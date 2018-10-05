using System.Collections;
using System.Collections.Generic;
using Game.Combat.Generation;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class DecayBehaviour : MonoBehaviour
    {
        private ParticleSystem _particles;
        private static readonly ObjectPool<DecayBehaviour> _decayPool = new ObjectPool<DecayBehaviour>("Decay Areas", "Prefabs/Combat/Effects/Decay Area");
        private List<CanTakeDamage> _ignoreTargets;
        private float _radius;
        private CircleCollider2D _collider;

        private void Awake()
        {
            _particles = gameObject.FindChildWithName<ParticleSystem>("Shards");
            _collider = GetComponent<CircleCollider2D>();
        }

        public static DecayBehaviour Create(Vector3 position, float radius = 1f)
        {
            DecayBehaviour decayBehaviour = _decayPool.Create();
            decayBehaviour.Initialise(position, radius);
            return decayBehaviour;
        }

        private void Initialise(Vector3 position, float radius)
        {
            _radius = radius;
            _ignoreTargets = new List<CanTakeDamage>();
            _collider.radius = _radius;
            StartCoroutine(EmitAndDie(position));
        }

        public void AddIgnoreTarget(CanTakeDamage _ignoreTarget)
        {
            _ignoreTargets.Add(_ignoreTarget);
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            CharacterCombat character = other.GetComponent<CharacterCombat>();
            if (character == null) return;
            if (_ignoreTargets.Contains(other.GetComponent<CanTakeDamage>())) return;
            character.Decay();
        }

        private IEnumerator EmitAndDie(Vector2 position)
        {
            transform.position = position;
            ParticleSystem.ShapeModule shape = _particles.shape;
            shape.radius = _radius - 0.5f;
            _particles.randomSeed = (uint) Random.Range(uint.MinValue, uint.MaxValue);
            _particles.Emit((int) (150 * _radius));
            bool active = CombatManager.IsCombatActive();
            while (_particles.particleCount > 0)
            {
                if (!CombatManager.IsCombatActive() && active) _particles.PauseParticles();
                else if (CombatManager.IsCombatActive() && !active) _particles.ResumeParticles();
                active = CombatManager.IsCombatActive();
                yield return null;
            }

            _decayPool.Dispose(this);
        }

        private void OnDestroy()
        {
            _decayPool.Dispose(this);
        }

        public void AddIgnoreTargets(List<CanTakeDamage> targetsToIgnore)
        {
            _ignoreTargets.AddRange(targetsToIgnore);
        }
    }
}