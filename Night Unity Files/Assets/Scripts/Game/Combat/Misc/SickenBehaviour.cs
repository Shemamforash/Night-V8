using System.Collections;
using System.Collections.Generic;
using Game.Combat.Generation;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class SickenBehaviour : MonoBehaviour
    {
        private static readonly ObjectPool<SickenBehaviour> _sicknessPool = new ObjectPool<SickenBehaviour>("Sicken Effects", "Prefabs/Combat/Visuals/Sicken Effect");
        private ParticleSystem[] _particleSystems;
        private List<CanTakeDamage> _ignoreTargets;
        private AudioSource _audioSource;

        public void Awake()
        {
            _particleSystems = transform.GetComponentsInChildren<ParticleSystem>();
            _audioSource = GetComponent<AudioSource>();
        }

        public static List<CanTakeDamage> Create(Vector2 position, List<CanTakeDamage> ignoreTargets, int stacks = 1)
        {
            List<CanTakeDamage> characters = CombatManager.GetCharactersInRange(position, 1);
            characters.ForEach(c =>
            {
                if (ignoreTargets.Contains(c)) return;
                c.Sicken(stacks);
                SickenBehaviour sickness = _sicknessPool.Create();
                sickness.StartCoroutine(sickness.Sicken(c.transform.position));
            });
            return characters;
        }

        public static List<CanTakeDamage> Create(Vector2 position, int stacks = 1)
        {
            return Create(position, new List<CanTakeDamage>(), stacks);
        }

        public static void Create(Vector2 position, CanTakeDamage target)
        {
            SickenBehaviour sickness = _sicknessPool.Create();
            sickness.StartCoroutine(sickness.Sicken(position));
            target.Sicken();
        }

        private IEnumerator Sicken(Vector2 position)
        {
            _audioSource.pitch = Random.Range(0.9f, 1.1f);
            _audioSource.Play();
            transform.position = position;
            foreach (ParticleSystem system in _particleSystems) system.Play();
            yield return new WaitForSeconds(2f);
            _sicknessPool.Return(this);
        }

        public void OnDestroy()
        {
            _sicknessPool.Dispose(this);
        }
    }
}