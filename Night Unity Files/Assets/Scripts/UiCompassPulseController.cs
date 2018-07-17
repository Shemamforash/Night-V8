using System.Collections.Generic;
using SamsHelper.Libraries;
using UnityEngine;

public class UiCompassPulseController : MonoBehaviour
{
    private static GameObject _pulsePrefab;
    private static Transform _pulseContent;
    private static readonly List<Pulse> _remainingPulses = new List<Pulse>();

    public void Awake()
    {
        _pulseContent = transform;
        _pulsePrefab = Resources.Load("Prefabs/Combat/Visuals/Compass Pulse") as GameObject;
    }

    public static void InitialisePulses(int count)
    {
        _remainingPulses.ForEach(a => a.Destroy());
        _remainingPulses.Clear();
        for (int i = 0; i < count; ++i)
        {
            Pulse newRound = new Pulse(Helper.InstantiateUiObject(_pulsePrefab, _pulseContent));
            _remainingPulses.Add(newRound);
        }
    }

    public static void UpdateCompassPulses()
    {
        _remainingPulses[_remainingPulses.Count - 1].Destroy();
        _remainingPulses.RemoveAt(_remainingPulses.Count - 1);
    }

    private class Pulse
    {
        private readonly GameObject _ammoObject;

        public Pulse(GameObject ammoObject)
        {
            Vector3 position = ammoObject.transform.position;
            position.z = 0;
            ammoObject.transform.position = position;
            _ammoObject = ammoObject;
        }

        public void Destroy()
        {
            Object.Destroy(_ammoObject);
        }
    }
}