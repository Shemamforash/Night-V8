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

        protected override ObjectPool<BulletTrail> GetObjectPool()
        {
            return _pool;
        }

        protected override void ClearTrails()
        {
            _path.Clear();
        }

        protected override Color GetColour()
        {
            ParticleSystem.TrailModule trails = _path.trails;
            return trails.colorOverTrail.gradient.colorKeys[1].color;
        }

        protected override void SetColour(Color color)
        {
            ParticleSystem.TrailModule trails = _path.trails;
            trails.colorOverTrail.gradient.colorKeys[1].color = color;
        }
    }
}