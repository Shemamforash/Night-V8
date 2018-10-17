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

        public static PistolTrail Create()
        {
            return (PistolTrail) _pool.Create();
        }

        protected override bool Done()
        {
            return _path.particleCount == 0;
        }

        protected override ObjectPool<BulletTrail> GetObjectPool()
        {
            return _pool;
        }

        protected override void ClearTrails()
        {
            _path.Clear();
        }

        protected override void StopEmitting()
        {
            _path.Stop();
        }
    }
}