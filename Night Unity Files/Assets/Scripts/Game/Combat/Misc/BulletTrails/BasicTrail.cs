using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class BasicTrail : BulletTrail
    {
        private static readonly ObjectPool<BulletTrail> _pool = new ObjectPool<BulletTrail>("Bullet Trails", "Prefabs/Combat/Shots/Bullet Trail");
        private TrailRenderer _trail;

        public void Awake()
        {
            _trail = GetComponent<TrailRenderer>();
        }

        public static BasicTrail Create()
        {
            return (BasicTrail) _pool.Create();
        }

        protected override ObjectPool<BulletTrail> GetObjectPool()
        {
            return _pool;
        }

        protected override void ClearTrails()
        {
            _trail.Clear();
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