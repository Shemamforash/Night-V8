using System;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;

public class RadianceController : MonoBehaviour, ICombatEvent
{
    private static bool _active;
    private static GameObject _stonePrefab;
    private int _radianceAvailable;
    private static RadianceController _instance;
    private static bool _activated;

    public void Awake()
    {
        _instance = this;
        _radianceAvailable = Inventory.GetResourceQuantity("Radiance");
        _activated = CombatManager.GetCurrentRegion().Claimed();
        if (!_activated) return;
        Vector2? existingRadianceStone = CombatManager.GetCurrentRegion().RadianceStonePosition;
        CreateRadianceStone(existingRadianceStone.Value, false);
    }

    private class RadianceBehaviour : MonoBehaviour
    {
        private AudioSource _audioSource;
        private float _pitchAnchor;

        public void Awake()
        {
            _audioSource = gameObject.FindChildWithName<AudioSource>("Drone");
            float noise = Mathf.PerlinNoise(Time.timeSinceLevelLoad, 0);
            noise = noise / 5f - 0.1f;
            _audioSource.pitch = 1 + noise;
        }

        public void Pulse()
        {
            gameObject.FindChildWithName<ParticleSystem>("Pulse").Play();
            gameObject.FindChildWithName<AudioSource>("Boom").Play();
        }
    }

    public float InRange()
    {
        if (_activated) return -1;
        if (_radianceAvailable == 0) return -1;
        if (!CombatManager.AllEnemiesDead()) return -1;
        if (!CombatManager.Region().IsDynamic()) return -1;
        if (CacheController.Active()) return -1;
        return 1;
    }

    public string GetEventText()
    {
        return "Claim region [T] (" + _radianceAvailable + " radiance left)";
    }

    private static void CreateRadianceStone(Vector2 position, bool playPulse)
    {
        if (_stonePrefab == null) _stonePrefab = Resources.Load<GameObject>("Prefabs/Combat/Effects/Radiance Stone");
        GameObject stoneObject = Instantiate(_stonePrefab);
        RadianceBehaviour radianceBehaviour = stoneObject.AddComponent<RadianceBehaviour>();
        if (playPulse) radianceBehaviour.Pulse();
        else
        {
            ParticleSystem[] particles = stoneObject.GetComponentsInChildren<ParticleSystem>();
            Array.ForEach(particles, p =>
            {
                ParticleSystem.MainModule main = p.main;
                main.prewarm = true;
            });
        }

        stoneObject.transform.position = position;
    }

    public void Activate()
    {
        Inventory.DecrementResource("Radiance", 1);
        Vector3 position = transform.position;
        PlayerCombat.Instance.Player.TravelAction.GetCurrentRegion().Claim(position);
        CreateRadianceStone(position, true);
        _activated = true;
    }

    public static RadianceController Instance() => _instance;
}