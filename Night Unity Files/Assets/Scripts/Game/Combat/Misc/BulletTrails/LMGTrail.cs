using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class LMGTrail : BulletTrail
    {
        private static readonly ObjectPool<BulletTrail> _pool = new ObjectPool<BulletTrail>("LMG Trails", "Prefabs/Combat/Shots/LMG Trail");
        private ParticleSystem _path;

        public void Awake()
        {
            _path = GetComponent<ParticleSystem>();
        }

        public static LMGTrail Create()
        {
            return (LMGTrail) _pool.Create();
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