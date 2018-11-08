using Game.Combat.Generation;
using Game.Combat.Generation.Shrines;
using Game.Combat.Misc;
using Game.Exploration.Regions;
using UnityEngine;

public class SaveStoneBehaviour : BasicShrineBehaviour, ICombatEvent
{
    private static GameObject _saveStonePrefab;
    private static SaveStoneBehaviour _instance;

    public void Awake()
    {
        _instance = this;
    }

    public static SaveStoneBehaviour Instance()
    {
        return _instance;
    }
    
    protected override void StartShrine()
    {
    }

    public static void Generate(Region region)
    {
        if (_saveStonePrefab == null) _saveStonePrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Save Stone");
        GameObject saveStoneObject = Instantiate(_saveStonePrefab);
        saveStoneObject.GetComponent<SaveStoneBehaviour>().Initialise();
    }

    private void Initialise()
    {
        transform.position = Vector2.zero;
        PathingGrid.AddBlockingArea(Vector2.zero, 0.5f);
    }

    public float InRange()
    {
        return IsInRange ? 1 : -1;
    }

    public string GetEventText()
    {
        return "Make an offering upon the alter";
    }

    public void Activate()
    {
        DismantleMenuController.Show();
    }
}