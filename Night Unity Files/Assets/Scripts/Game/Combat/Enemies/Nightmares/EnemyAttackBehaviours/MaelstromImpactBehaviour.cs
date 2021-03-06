﻿using System.Collections;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

public class MaelstromImpactBehaviour : MonoBehaviour
{
    private static readonly ObjectPool<MaelstromImpactBehaviour> _impactPool = new ObjectPool<MaelstromImpactBehaviour>("Impacts", "Prefabs/Combat/Visuals/Maelstrom Impact");
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public static void Create(Vector2 position)
    {
        MaelstromImpactBehaviour impact = _impactPool.Create();
        impact.transform.position = position;
        impact.StartCoroutine(impact.WaitAndDie());
    }

    private IEnumerator WaitAndDie()
    {
        _audioSource.Play();
        float timer = 3f;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        _impactPool.Return(this);
    }

    private void OnDestroy()
    {
        _impactPool.Dispose(this);
    }
}