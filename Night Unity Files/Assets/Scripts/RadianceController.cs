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
    private bool _activated;
    private static bool _skillWasUsed;
    private static bool _shotFired;

    public void Awake()
    {
        _instance = this;
        _radianceAvailable = Inventory.GetResourceQuantity("Radiance");
        _skillWasUsed = false;
        _shotFired = false;
    }

    private class RadianceBehaviour : MonoBehaviour
    {
        private AudioSource _audioSource;
        private float _pitchAnchor;

        public void Awake()
        {
            _audioSource = gameObject.FindChildWithName<AudioSource>("Drone");
            _pitchAnchor = Random.Range(0.9f, 1.1f);
        }

        public void Update()
        {
            float noise = Mathf.PerlinNoise(Time.timeSinceLevelLoad, 0);
            noise = noise / 5f - 0.1f;
            float pitch = _pitchAnchor + noise;
            _audioSource.pitch = pitch;
        }
    }

    public float InRange()
    {
        if (_activated) return -1;
        if (!CombatManager.AllEnemiesDead()) return -1;
        RegionType region = CombatManager.Region().GetRegionType();
        if (region == RegionType.Nightmare
            || region == RegionType.Temple
            || region == RegionType.Tomb
            || region == RegionType.Rite) return -1;
        return _radianceAvailable > 0 ? 1 : -1;
    }

    public string GetEventText()
    {
        return "Claim region [T] (" + _radianceAvailable + " radiance left)";
    }

    public void Activate()
    {
        Inventory.DecrementResource("Essence", 1);
        PlayerCombat.Instance.Player.TravelAction.GetCurrentNode().Claim();
        if (_stonePrefab == null) _stonePrefab = Resources.Load<GameObject>("Prefabs/Combat/Effects/Radiance Stone");
        GameObject stoneObject = Instantiate(_stonePrefab);
        stoneObject.AddComponent<RadianceBehaviour>();
        stoneObject.transform.position = transform.position;
        _activated = true;
        if (!_shotFired && !_skillWasUsed)
            PlayerCombat.Instance.Player.BrandManager.IncreaseBattlesNoSkills();
    }

    public static void SetSkillWasUsed()
    {
        _skillWasUsed = true;
    }

    public static void SetHasFiredShot()
    {
        _shotFired = true;
    }

    public static RadianceController Instance()
    {
        return _instance;
    }
}