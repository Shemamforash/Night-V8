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
        private List<ITakeDamageInterface> _ignoreTargets;
        private float _radius;

        public static DecayBehaviour Create(Vector3 position, float radius = 1f)
        {
            DecayBehaviour decayBehaviour = _decayPool.Create();
            decayBehaviour.Initialise(position, radius);
            return decayBehaviour;
        }

        private void Initialise(Vector3 position, float radius)
        {
            transform.position = position;
            _radius = radius;
            _ignoreTargets = new List<ITakeDamageInterface>();
        }

        public void AddIgnoreTarget(ITakeDamageInterface _ignoreTarget)
        {
            _ignoreTargets.Add(_ignoreTarget);
        }

        private void Awake()
        {
            _particles = GetComponent<ParticleSystem>();
            StartCoroutine(EmitAndDie());
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            CharacterCombat character = other.GetComponent<CharacterCombat>();
            if (character == null) return;
            if (_ignoreTargets.Contains(other.GetComponent<ITakeDamageInterface>())) return;
            character.Decay();
        }

        private IEnumerator EmitAndDie()
        {
            ParticleSystem.ShapeModule shape = _particles.shape;
            shape.radius = _radius;
            _particles.Emit((int) (50 * _radius));
            while (_particles.particleCount > 0)
            {
                if (!CombatManager.IsCombatActive()) _particles.PauseParticles();
                else _particles.ResumeParticles();
                yield return null;
            }

            _decayPool.Return(this);
        }

        private void OnDestroy()
        {
            _decayPool.Dispose(this);
        }

        public void AddIgnoreTargets(List<ITakeDamageInterface> targetsToIgnore)
        {
            _ignoreTargets.AddRange(targetsToIgnore);
        }
    }
}