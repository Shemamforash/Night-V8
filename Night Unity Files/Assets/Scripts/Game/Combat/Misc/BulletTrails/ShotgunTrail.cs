using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class ShotgunTrail : BulletTrail
    {
        private static readonly ObjectPool<BulletTrail> _pool = new ObjectPool<BulletTrail>("Shotgun Trails", "Prefabs/Combat/Shots/Shotgun Trail");
        private TrailRenderer _trail;
        private ParticleSystem _points;

        public void Awake()
        {
            _trail = GetComponent<TrailRenderer>();
            _points = gameObject.FindChildWithName<ParticleSystem>("Points");
        }

        public static ShotgunTrail Create()
        {
            return (ShotgunTrail) _pool.Create();
        }

        protected override ObjectPool<BulletTrail> GetObjectPool()
        {
            return _pool;
        }

        protected override void ClearTrails()
        {
            _trail.Clear();
            _points.Clear();
        }

        protected override Color GetColour()
        {
            return _trail.startColor;
        }

        protected override void SetColour(Color color)
        {
            _trail.startColor = color;
        }
    }
}