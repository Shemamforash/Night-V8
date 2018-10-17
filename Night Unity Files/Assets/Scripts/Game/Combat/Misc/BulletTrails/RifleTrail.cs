using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class RifleTrail : BulletTrail
    {
        private static readonly ObjectPool<BulletTrail> _pool = new ObjectPool<BulletTrail>("Rifle Trails", "Prefabs/Combat/Shots/Rifle Trail");
        private ParticleSystem _path;

        public void Awake()
        {
            _path = gameObject.FindChildWithName<ParticleSystem>("Trail 1");
        }

        public static RifleTrail Create()
        {
            return (RifleTrail) _pool.Create();
        }

        protected override void StopEmitting()
        {
            _path.Stop();
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
    }
}