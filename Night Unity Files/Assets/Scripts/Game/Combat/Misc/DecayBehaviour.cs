using System.Collections;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class DecayBehaviour : MonoBehaviour
    {
        private ParticleSystem _particles;
        private static readonly ObjectPool<DecayBehaviour> _decayPool = new ObjectPool<DecayBehaviour>("Prefabs/Combat/Effects/Decay Area");

        public static DecayBehaviour Create(Vector3 position)
        {
            DecayBehaviour decayBehaviour = _decayPool.Create();
            decayBehaviour.transform.position = position;
            return decayBehaviour;
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
            character.Decay();
        }
        
        private IEnumerator EmitAndDie()
        {
            _particles.Emit(50);
            while (_particles.particleCount > 0)
            {
                yield return null;
            }
            _decayPool.Return(this);
        }

        private void OnDestroy()
        {
            _decayPool.Dispose(this);
        }
    }
}