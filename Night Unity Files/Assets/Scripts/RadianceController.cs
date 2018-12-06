using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Exploration.Regions;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

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
        _activated = false;
        Vector2? existingRadianceStone = CombatManager.GetCurrentRegion().RadianceStonePosition;
        if (existingRadianceStone == null) return;
        CreateRadianceStone(existingRadianceStone.Value);
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
    }

    public float InRange()
    {
        if (_activated) return -1;
        if (!CombatManager.AllEnemiesDead()) return -1;
        if (!CombatManager.Region().IsDynamic()) return -1;
        return _radianceAvailable > 0 ? 1 : -1;
    }

    public string GetEventText()
    {
        return "Claim region [T] (" + _radianceAvailable + " radiance left)";
    }

    private static void CreateRadianceStone(Vector2 position)
    {
        if (_stonePrefab == null) _stonePrefab = Resources.Load<GameObject>("Prefabs/Combat/Effects/Radiance Stone");
        GameObject stoneObject = Instantiate(_stonePrefab);
        stoneObject.AddComponent<RadianceBehaviour>();
        stoneObject.transform.position = position;
        _activated = true;
    }

    public void Activate()
    {
        Inventory.DecrementResource("Radiance", 1);
        PlayerCombat.Instance.Player.TravelAction.GetCurrentRegion().Claim(transform.position);
        CreateRadianceStone(transform.position);
    }

    public static RadianceController Instance()
    {
        return _instance;
    }
}