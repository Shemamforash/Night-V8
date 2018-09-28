using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Facilitating.UIControllers;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using TriangleNet.Topology.DCEL;
using UnityEngine;
using Random = UnityEngine.Random;

public class UiCompassController : MonoBehaviour
{
    private ParticleSystem _compassPulse;
    private float _showItemTimeCurrent;
    private const float ShowItemTimeMax = 5f;
    private static UiCompassController _instance;
    private const float MaxDetectDistance = 10f;
    private AudioSource _audioSource;
    private static List<CompassItem> _compassItems;
    private readonly List<CompassIndicatorBehaviour> _indicators = new List<CompassIndicatorBehaviour>();

    public void Awake()
    {
        _compassPulse = gameObject.FindChildWithName<ParticleSystem>("Compass Pulse");
        _instance = this;
        _audioSource = GetComponent<AudioSource>();
        _compassItems = new List<CompassItem>();
        for (int i = 0; i < 18; ++i) _indicators.Add(CompassIndicatorBehaviour.Create(transform));
    }

    public static bool EmitPulse()
    {
        if (_instance._compassPulse.particleCount != 0) return false;
        _instance._compassPulse.Play();
        _instance.StartCoroutine(_instance.ShowItems());
        _instance.StartCoroutine(_instance.HighlightNearbyItems());
        _instance._audioSource.pitch = Random.Range(0.9f, 1.1f);
        _instance._audioSource.Play();
        return true;
    }

    private IEnumerator HighlightNearbyItems()
    {
        float currentTime = 0f;
        float endTime = _compassPulse.main.startLifetime.constant;
        float speed = _compassPulse.main.startSpeed.constant;
        HashSet<CompassItem> pulsedItems = new HashSet<CompassItem>();
        while (currentTime < endTime)
        {
            currentTime += Time.deltaTime;
            float distance = speed * currentTime;
            _compassItems.ForEach(c =>
            {
                if (pulsedItems.Contains(c)) return;
                float containerDistance = Vector2.Distance(transform.position, c.transform.position);
                if (containerDistance > distance) return;
                c.Pulse();
                pulsedItems.Add(c);
            });
            yield return null;
        }
    }

    private IEnumerator ShowItems()
    {
        _showItemTimeCurrent = 0f;
        while (_showItemTimeCurrent < ShowItemTimeMax)
        {
            _showItemTimeCurrent += Time.deltaTime;
            if (_showItemTimeCurrent > ShowItemTimeMax) _showItemTimeCurrent = ShowItemTimeMax;
            float baseAlpha = 1 - _showItemTimeCurrent / ShowItemTimeMax;
            List<Tuple<float, float>> alphaRotations = GetItemsInRange(baseAlpha);
            SquashRotations(alphaRotations);
            yield return null;
        }
    }

    private void SquashRotations(List<Tuple<float, float>> alphaRotations)
    {
        alphaRotations.Sort((a, b) => a.Item2.CompareTo(b.Item2));
        for (int i = 0; i < 18; ++i)
        {
            float angleFrom = i * 20;
            float angleTo = angleFrom + 20;
            List<Tuple<float, float>> itemsWithinAngle = alphaRotations.Where(c => c.Item2 >= angleFrom && c.Item2 <= angleTo).ToList();
            if (itemsWithinAngle.Count == 0)
            {
                _indicators[i].SetAlpha(0f);
                continue;
            }

            float alpha = 0;
            float angle = 0;
            itemsWithinAngle.ForEach(tup =>
            {
                alpha += tup.Item1;
                angle += tup.Item2;
            });
            alpha /= itemsWithinAngle.Count;
            angle /= itemsWithinAngle.Count;
            _indicators[i].SetAlpha(alpha);
            _indicators[i].SetRotation(angle);
        }
    }

//    private List<Tuple<float, float>> SquashRotations(List<Tuple<float, float>> alphaRotations)
//    {
//        alphaRotations.Sort((a, b) => a.Item2.CompareTo(b.Item2));
//        bool changed = true;
//        int count = 10;
//        while (changed && count > 0)
//        {
//            changed = false;
//            List<Tuple<float, float>> newAlphaRotations = new List<Tuple<float, float>>();
//            for (int i = 0; i < alphaRotations.Count; ++i)
//            {
//                int next = Helper.NextIndex(i, alphaRotations);
//                float angleA = alphaRotations[i].Item2;
//                float angleB = alphaRotations[next].Item2;
//                if (angleB < angleA) angleB += 360;
//
//                float angleBetween = angleB - angleA;
//                if (angleBetween > 20f)
//                {
//                    newAlphaRotations.Add(alphaRotations[i]);
//                    continue;
//                }
//
//                float newAngle = (angleA + angleB) / 2f;
//                float newAlpha = (alphaRotations[i].Item1 + alphaRotations[next].Item1) / 2f;
//                newAlphaRotations.Add(Tuple.Create(newAlpha, newAngle));
//                ++i;
//                changed = true;
//            }
//
//            --count;
//            alphaRotations = newAlphaRotations;
//        }
//
//        return alphaRotations;
//    }

    private List<Tuple<float, float>> GetItemsInRange(float baseAlpha)
    {
        List<Tuple<float, float>> alphaRotations = new List<Tuple<float, float>>();
        _compassItems.ForEach(c =>
        {
            float distance = Vector2.Distance(transform.position, c.transform.position);
            Debug.Log(distance + "/" + MaxDetectDistance);
            if (distance > MaxDetectDistance) return;
            distance = 1 - distance / MaxDetectDistance;
            float newAlpha = baseAlpha * distance;
            float rotation = AdvancedMaths.AngleFromUp(transform.position, c.transform.position);
            alphaRotations.Add(Tuple.Create(newAlpha, rotation));
        });
        return alphaRotations;
    }

    public static void RegisterCompassItem(CompassItem compassItem)
    {
        _compassItems.Add(compassItem);
    }

    public static void UnregisterCompassItem(CompassItem compassItem)
    {
        _compassItems.Remove(compassItem);
    }
}