using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class FireTrail : BulletTrail
    {
        private static readonly ObjectPool<BulletTrail> _pool = new ObjectPool<BulletTrail>("Fire Trails", "Prefabs/Combat/Shots/Fire Trail");
        private ParticleSystem _path;

        public void Awake()
        {
            _path = GetComponent<ParticleSystem>();
        }

        public static FireTrail Create()
        {
            FireTrail trail = (FireTrail) _pool.Create();
            return trail;
        }

        protected override bool Done() => _path.particleCount == 0;
        protected override ObjectPool<BulletTrail> GetObjectPool() => _pool;
        protected override void ClearTrails() => _path.Clear();
    }
}