using System;
using System.Collections;
using System.Collections.Generic;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

public class UiCompassController : MonoBehaviour
{
    private ParticleSystem _compassPulse;
    private GameObject _indicatorPrefab;
    private float _showItemTimeCurrent;
    private const float ShowItemTimeMax = 5f;
    private static UiCompassController _instance;
    private const float MaxDetectDistance = 10f;
    private AudioSource _audioSource;

    public void Awake()
    {
        _indicatorPrefab = Resources.Load<GameObject>("Prefabs/Combat/Indicator");
        _compassPulse = gameObject.FindChildWithName<ParticleSystem>("Compass Pulse");
        _instance = this;
        _audioSource = GetComponent<AudioSource>();
    }

    public static bool EmitPulse()
    {
        if (_instance._compassPulse.particleCount != 0) return false;
        _instance._compassPulse.Play();
        _instance.StartCoroutine(_instance.ShowItems());
        _instance.StartCoroutine(_instance.HighlightContainers());
        _instance._audioSource.pitch = Random.Range(0.9f, 1.1f);
        _instance._audioSource.Play();
        return true;
    }

    private IEnumerator HighlightContainers()
    {
        float currentTime = 0f;
        float endTime = _compassPulse.main.startLifetime.constant;
        float speed = _compassPulse.main.startSpeed.constant;
        HashSet<ContainerBehaviour> pulsedContainers = new HashSet<ContainerBehaviour>();
        while (currentTime < endTime)
        {
            currentTime += Time.deltaTime;
            float distance = speed * currentTime;
            ContainerController.Containers.ForEach(c =>
            {
                if (pulsedContainers.Contains(c)) return;
                float containerDistance = Vector2.Distance(transform.position, c.transform.position);
                if (containerDistance > distance) return;
                c.Pulse();
                pulsedContainers.Add(c);
            });
            yield return null;
        }
    }

    private IEnumerator ShowItems()
    {
        List<Tuple<ContainerBehaviour, SpriteRenderer>> _indicators = new List<Tuple<ContainerBehaviour, SpriteRenderer>>();
        _showItemTimeCurrent = 0f;
        ContainerController.Containers.ForEach(c =>
        {
            if (c.Revealed() && Helper.IsObjectInCameraView(c.gameObject)) return;
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
                float modifiedLerpVal = 1 - lerpVal;
                if (i.Item1 != null)
                {
                    float distance = Vector2.Distance(transform.position, i.Item1.transform.position);
                    if (distance > MaxDetectDistance) modifiedLerpVal = 0;
                    else distance /= MaxDetectDistance;
                    distance = 1 - distance;
                    modifiedLerpVal *= distance;
                    i.Item2.transform.rotation = Quaternion.Euler(new Vector3(0, 0, AdvancedMaths.AngleFromUp(transform.position, i.Item1.transform.position)));
                    if (i.Item1.Revealed()) modifiedLerpVal = Mathf.Pow(modifiedLerpVal, 3f);
                }

                i.Item2.color = new Color(1, 1, 1, modifiedLerpVal);
            });
            yield return null;
        }

        _indicators.ForEach(i => Destroy(i.Item2.gameObject));
        _indicators.Clear();
    }
}