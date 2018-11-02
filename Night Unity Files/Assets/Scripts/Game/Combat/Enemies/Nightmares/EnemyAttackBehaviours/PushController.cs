﻿using System.Collections;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

public class PushController : MonoBehaviour
{
    private ParticleSystem _pushParticles;
    private static readonly ObjectPool<PushController> _pushPool = new ObjectPool<PushController>("Pushes", "Prefabs/Combat/Visuals/Push Burst");

    public static void Create(Vector2 position, float rotation, bool player, float arcSize = 20f)
    {
        PushController pushObject = _pushPool.Create();
        pushObject.transform.position = position;
        pushObject.SetArcSize(rotation, arcSize, player);
    }

    private void SetArcSize(float rotation, float arcSize, bool player)
    {
        ParticleSystem.ShapeModule shape = _pushParticles.shape;
        float offset = arcSize / 2f + 90;
        rotation = offset - rotation;
        rotation = 180 - rotation;
        shape.rotation = new Vector3(0, 0, rotation);
        shape.arc = arcSize;
        int emitCount = (int) (0.265f * (arcSize - 20) + 10);
        ParticleSystem.CollisionModule collision = _pushParticles.collision;
        int mask = player ? 1 << 10 : 1 << 17;
        collision.collidesWith = mask;
        _pushParticles.Emit(emitCount);
        StartCoroutine(WaitAndDie());
    }

    private IEnumerator WaitAndDie()
    {
        while (_pushParticles.particleCount > 0) yield return null;
        _pushPool.Return(this);
    }

    public void OnDestroy()
    {
        _pushPool.Dispose(this);
    }

    public void Awake()
    {
        _pushParticles = GetComponent<ParticleSystem>();
    }
}