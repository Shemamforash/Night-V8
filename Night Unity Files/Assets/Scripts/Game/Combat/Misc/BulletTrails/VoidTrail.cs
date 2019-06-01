using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class VoidTrail : BulletTrail
    {
        private static readonly ObjectPool<BulletTrail> _pool = new ObjectPool<BulletTrail>("Void Trails", "Prefabs/Combat/Shots/Void Trail");
        private ParticleSystem _path;

        public void Awake()
        {
            _path = GetComponent<ParticleSystem>();
        }

        public static VoidTrail Create()
        {
            VoidTrail trail = (VoidTrail) _pool.Create();
            return trail;
        }

        protected override bool Done() => _path.particleCount == 0;
        protected override ObjectPool<BulletTrail> GetObjectPool() => _pool;
        protected override void ClearTrails() => _path.Clear();
    }
}