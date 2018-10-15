using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class RifleTrail : BulletTrail
    {
        private static readonly ObjectPool<BulletTrail> _pool = new ObjectPool<BulletTrail>("Rifle Trails", "Prefabs/Combat/Shots/Rifle Trail");
        private ParticleSystem _path1;// _path2;

        public void Awake()
        {
            _path1 = gameObject.FindChildWithName<ParticleSystem>("Trail 1");
//            _path2 = gameObject.FindChildWithName<ParticleSystem>("Trail 2");
        }

        public static RifleTrail Create()
        {
            return (RifleTrail) _pool.Create();
        }

        protected override ObjectPool<BulletTrail> GetObjectPool()
        {
            return _pool;
        }

        protected override void ClearTrails()
        {
            _path1.Clear();
//            _path2.Clear();
        }

        protected override Color GetColour()
        {
            ParticleSystem.TrailModule trails = _path1.trails;
            return trails.colorOverTrail.gradient.colorKeys[1].color;
        }

        protected override void SetColour(Color color)
        {
            ParticleSystem.TrailModule trails = _path1.trails;
            trails.colorOverTrail.gradient.colorKeys[1].color = color;
//            trails = _path2.trails;
//            trails.colorOverTrail.gradient.colorKeys[1].color = color;
        }
    }
}