using System.Collections;
using System.Collections.Generic;
using Game.Combat.Generation;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class SickenBehaviour : MonoBehaviour
    {
        private static readonly ObjectPool<SickenBehaviour> _sicknessPool = new ObjectPool<SickenBehaviour>("Fire Areas", "Prefabs/Combat/Visuals/Sicken Effect");
        private ParticleSystem _particles;
        private List<ITakeDamageInterface> _ignoreTargets;

        public void Awake()
        {
            _particles = GetComponent<ParticleSystem>();
        }

        public static void Create(Vector2 position, List<ITakeDamageInterface> ignoreTargets, float radius = 1f)
        {
            List<ITakeDamageInterface> characters = CombatManager.GetCharactersInRange(position, radius);
            characters.ForEach(c =>
            {
                if (ignoreTargets.Contains(c)) return;
                MonoBehaviour character = (MonoBehaviour) c;
                c.Sicken();
                SickenBehaviour sickness = _sicknessPool.Create();
                sickness.StartCoroutine(sickness.Sicken(character.transform.position));
            });
        }

        public static void Create(Vector2 position, ITakeDamageInterface target)
        {
            SickenBehaviour sickness = _sicknessPool.Create();
            sickness.StartCoroutine(sickness.Sicken(position));
            target.Sicken();
        }

        private IEnumerator Sicken(Vector2 position)
        {
            transform.position = position;
            _particles.Emit(50);
            while (_particles.particleCount > 0) yield return null;
            _sicknessPool.Return(this);
        }

        public void OnDestroy()
        {
            _sicknessPool.Dispose(this);
        }
    }
}