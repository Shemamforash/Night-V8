﻿using System.Collections;
using System.Threading;
using DG.Tweening;
using Facilitating.Persistence;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class SaveIconController : MonoBehaviour
{
    private ParticleSystem _spinParticles;
    private static GameObject _spinnerPrefab;

    public void Awake()
    {
        _spinParticles = gameObject.FindChildWithName<ParticleSystem>("Particles");
        GetComponent<Canvas>().worldCamera = Camera.main;
        StartCoroutine(Spin());
    }

    public static void Save()
    {
        if (_spinnerPrefab == null) _spinnerPrefab = Resources.Load<GameObject>("Prefabs/Save Spinner");
        Instantiate(_spinnerPrefab);
    }

    private IEnumerator Spin()
    {
        Debug.Log("saving");
        Thread thread = new Thread(SaveController.SaveGame);
        thread.Start();
        ParticleSystem.MainModule main = _spinParticles.main;
        main.startColor = Color.white;
        _spinParticles.Play();
        float minTime = 1f;
        while (minTime > 0f && thread.IsAlive)
        {
            minTime -= Time.deltaTime;
            yield return null;
        }

        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() => Destroy(gameObject));

        minTime = 1f;
        while (minTime > 0f)
        {
            minTime -= Time.deltaTime;
            main.startColor = Color.Lerp(Color.white, UiAppearanceController.InvisibleColour, 1f - minTime);
            yield return null;
        }

        _spinParticles.Stop();
    }
}