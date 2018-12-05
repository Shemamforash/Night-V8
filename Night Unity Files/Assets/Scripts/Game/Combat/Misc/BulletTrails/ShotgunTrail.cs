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

        public static ShotgunTrail Create(bool isPlayer)
        {
            ShotgunTrail trail = (ShotgunTrail) _pool.Create();
            trail.Initialise(isPlayer);
            return trail;
        }

        protected override bool Done()
        {
            return _points.particleCount == 0 && _trail.positionCount == 0;
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

        private void Initialise(bool isPlayer)
        {
            ParticleSystem.MainModule main = _points.main;
            main.startColor = isPlayer ? Color.white : Color.red;
            _trail.startColor = isPlayer ? Color.white : Color.red;
            _trail.endColor = isPlayer ? new Color(1f, 1f, 1f, 0f) : new Color(1f, 0f, 0f, 0f);
        }
    }
}