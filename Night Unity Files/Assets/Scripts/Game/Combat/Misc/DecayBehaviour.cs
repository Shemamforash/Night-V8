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
        private ParticleSystem _shardParticles, _burstParticles;
        private static readonly ObjectPool<DecayBehaviour> _decayPool = new ObjectPool<DecayBehaviour>("Decay Areas", "Prefabs/Combat/Effects/Decay Area");
        private float _radius;
        private DecayDamageDeal _decayDamage;

        private void Awake()
        {
            _shardParticles = gameObject.FindChildWithName<ParticleSystem>("Shards");
            _burstParticles = gameObject.FindChildWithName<ParticleSystem>("Burst");
            _decayDamage = GetComponent<DecayDamageDeal>();
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
            _decayDamage.SetRadius(_radius);
            StartCoroutine(EmitAndDie(position));
        }

        public void AddIgnoreTarget(CanTakeDamage _ignoreTarget)
        {
            _decayDamage.AddIgnoreTarget(_ignoreTarget);
        }

        private IEnumerator EmitAndDie(Vector2 position)
        {
            transform.position = position;
            ParticleSystem.ShapeModule shape = _shardParticles.shape;
            shape.radius = _radius - 0.5f;
            shape = _burstParticles.shape;
            shape.radius = _radius - 0.7f;
            _shardParticles.randomSeed = (uint) Random.Range(uint.MinValue, uint.MaxValue);
            int emitCount = (int) (150 * _radius);
            _shardParticles.Emit(emitCount);
            _burstParticles.Emit(emitCount);
            bool active = CombatManager.IsCombatActive();
            while (_shardParticles.particleCount > 0)
            {
                if (!CombatManager.IsCombatActive() && active)
                {
                    _shardParticles.PauseParticles();
                    _burstParticles.PauseParticles();
                }
                else if (CombatManager.IsCombatActive() && !active)
                {
                    _shardParticles.ResumeParticles();
                    _burstParticles.ResumeParticles();
                }
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
            _decayDamage.AddIgnoreTargets(targetsToIgnore);
        }
    }
}