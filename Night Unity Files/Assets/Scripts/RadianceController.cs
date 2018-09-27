using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Global;
using UnityEngine;

public class RadianceController : MonoBehaviour, ICombatEvent
{
    private static bool _active;
    private static GameObject _stonePrefab;
    private int _radianceAvailable;
    private static RadianceController _instance;
    private bool _activated;

    public void Awake()
    {
        _instance = this;
        _radianceAvailable = WorldState.HomeInventory().GetResourceQuantity("Radiance");
    }

    public float InRange()
    {
        if (_activated) return -1;
        if (!CombatManager.AllEnemiesDead()) return -1;
        return _radianceAvailable > 0 ? 1 : -1;
    }

    public string GetEventText()
    {
        return "Claim region [T] (" + _radianceAvailable + " radiance left)";
    }

    public void Activate()
    {
        WorldState.HomeInventory().DecrementResource("Essence", 1);
        PlayerCombat.Instance.Player.TravelAction.GetCurrentNode().Claim();
        if (_stonePrefab == null) _stonePrefab = Resources.Load<GameObject>("Prefabs/Combat/Effects/Radiance Stone");
        GameObject stoneObject = Instantiate(_stonePrefab);
        stoneObject.transform.position = transform.position;
        _activated = true;
    }

    public static RadianceController Instance()
    {
        return _instance;
    }
}