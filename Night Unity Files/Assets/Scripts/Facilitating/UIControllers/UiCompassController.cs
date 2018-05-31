using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class UiCompassController : MonoBehaviour
{
    private ParticleSystem _compassPulse;
    private GameObject _indicatorPrefab;
    private float _showItemTimeCurrent;
    private const float ShowItemTimeMax = 2f;
    private static UiCompassController _instance;
    private const float MaxDetectDistance = 10f;
    
    public void Awake()
    {
        _indicatorPrefab = Resources.Load<GameObject>("Prefabs/Combat/Indicator");
        _compassPulse = Helper.FindChildWithName<ParticleSystem>(gameObject, "Compass Pulse");
        _instance = this;
    }

    public static void EmitPulse()
    {
        if (_instance._compassPulse.particleCount != 0) return;
        _instance._compassPulse.Play();
        _instance.StartCoroutine(_instance.ShowItems());
    }

    private IEnumerator ShowItems()
    {
        List<Tuple<ContainerController.ContainerBehaviour, SpriteRenderer>> _indicators = new List<Tuple<ContainerController.ContainerBehaviour, SpriteRenderer>>();
        _showItemTimeCurrent = 0f;
        ContainerController.Containers.ForEach(c =>
        {
            if (Vector2.Distance(c.transform.position, transform.position) > MaxDetectDistance) return;
            GameObject indicator = Instantiate(_indicatorPrefab);
            indicator.transform.SetParent(transform, false);
            _indicators.Add(Tuple.Create(c, indicator.GetComponent<SpriteRenderer>()));
        });
        while (_showItemTimeCurrent < ShowItemTimeMax)
        {
            _showItemTimeCurrent += Time.deltaTime;
            if (_showItemTimeCurrent > ShowItemTimeMax) _showItemTimeCurrent = ShowItemTimeMax;
            float lerpVal = _showItemTimeCurrent / ShowItemTimeMax;
            _indicators.ForEach(i =>
            {
                float distance = Vector2.Distance(transform.position, i.Item1.transform.position);
                float modifiedLerpVal = 1- lerpVal;
                if (distance > MaxDetectDistance) modifiedLerpVal = 0;
                else distance /= MaxDetectDistance;
                distance = 1 - distance;
                modifiedLerpVal *= distance;
                i.Item2.transform.rotation = Quaternion.Euler(new Vector3(0, 0, AdvancedMaths.AngleFromUp(transform.position, i.Item1.transform.position)));
                i.Item2.color = new Color(1,1,1, modifiedLerpVal);
            });
            yield return null;
        }
        _indicators.Clear();
    }
}