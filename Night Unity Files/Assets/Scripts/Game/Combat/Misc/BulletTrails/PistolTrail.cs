using System.Collections;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class PistolTrail : BulletTrail
    {
        private static readonly ObjectPool<BulletTrail> _pool = new ObjectPool<BulletTrail>("Pistol Trails", "Prefabs/Combat/Shots/Pistol Trail");
        private ParticleSystem _path;

        public void Awake()
        {
            _path = GetComponent<ParticleSystem>();
        }

        public static PistolTrail Create(bool isPlayer)
        {
            PistolTrail trail = (PistolTrail) _pool.Create();
            trail.Initialise(isPlayer);
            return trail;
        }

        protected override bool Done()
        {
            return _path.particleCount == 0;
        }

        public override void SetFinalPosition(Vector2 position)
        {
            base.SetFinalPosition(position);
            StartCoroutine(WaitToStop());

            IEnumerator WaitToStop()
            {
                yield return null;
                _path.Stop();
            }
        }

        protected override ObjectPool<BulletTrail> GetObjectPool()
        {
            return _pool;
        }

        protected override void ClearTrails()
        {
            _path.Clear();
        }

        private void Initialise(bool isPlayer)
        {
            ParticleSystem.MainModule main = _path.main;
            main.startColor = isPlayer ? Color.white : Color.red;
            _path.Play();
        }
    }
}