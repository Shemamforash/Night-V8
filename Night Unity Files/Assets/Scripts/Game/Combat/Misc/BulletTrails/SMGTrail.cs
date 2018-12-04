using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class SMGTrail : BulletTrail
    {
        private static readonly ObjectPool<BulletTrail> _pool = new ObjectPool<BulletTrail>("SMG Trails", "Prefabs/Combat/Shots/SMG Trail");
        private ParticleSystem _path;

        public void Awake()
        {
            _path = GetComponent<ParticleSystem>();
        }

        public static SMGTrail Create()
        {
            return (SMGTrail) _pool.Create();
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