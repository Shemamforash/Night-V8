using Game.Combat.Misc;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

public class MaelstromShotTrail : BulletTrail
{
    private static readonly ObjectPool<BulletTrail> _pool = new ObjectPool<BulletTrail>("Maelstrom Shot Trails", "Prefabs/Combat/Shots/Maelstrom Shot Trail");
    private TrailRenderer _trail;
    private ParticleSystem _paths, _points;

    public void Awake()
    {
        if (_trail != null) return;
        _trail= gameObject.FindChildWithName<TrailRenderer>("Trail");
        _paths = gameObject.FindChildWithName<ParticleSystem>("Paths");
        _points = gameObject.FindChildWithName<ParticleSystem>("Points");
    }

    public static MaelstromShotTrail Create()
    {
        return (MaelstromShotTrail) _pool.Create();
    }

    protected override bool Done()
    {
        return _paths.particleCount == 0 && _points.particleCount == 0 && _trail.positionCount == 0;
    }

    protected override ObjectPool<BulletTrail> GetObjectPool()
    {
        return _pool;
    }

    protected override void ClearTrails()
    {
        _trail.Clear();
        _paths.Clear();
        _points.Clear();
    }
}