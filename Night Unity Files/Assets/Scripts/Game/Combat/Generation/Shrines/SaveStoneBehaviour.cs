using DG.Tweening;
using Facilitating.Persistence;
using Facilitating.UIControllers;
using Game.Combat.Generation;
using Game.Combat.Generation.Shrines;
using Game.Combat.Misc;
using Game.Exploration.Regions;
using UnityEngine;

public class SaveStoneBehaviour : BasicShrineBehaviour, ICombatEvent
{
    private static GameObject _saveStonePrefab;
    private Region _region;
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
        saveStoneObject.GetComponent<SaveStoneBehaviour>().Initialise(region);
    }

    private void Initialise(Region region)
    {
        _region = region;
        transform.position = _region.ShrinePosition;
        PathingGrid.AddBlockingArea(_region.ShrinePosition, 0.5f);
        if (!_region.Saved) return;
        GetComponent<CompassItem>().Die();
        DisableParticles(true);
        Destroy(this);
    }

    private void DisableParticles(bool clear)
    {
        foreach (ParticleSystem ps in transform.GetComponentsInChildren<ParticleSystem>(transform))
        {
            ps.Stop();
            if (clear) ps.Clear();
        }

        foreach (TrailRenderer trailRenderer in transform.GetComponentsInChildren<TrailRenderer>(transform))
        {
            if (clear) trailRenderer.Clear();
            else trailRenderer.DOTime(0, 1);
        }
    }

    public float InRange()
    {
        if (_region.Saved) return -1;
        return IsInRange ? 1 : -1;
    }

    public string GetEventText()
    {
        return "Carve your progress into the stone";
    }

    public void Activate()
    {
        Destroy(gameObject.GetComponent<Collider2D>());
        GetComponent<CompassItem>().Die();
        Triggered = true;
        SaveController.SaveGame();
        DisableParticles(false);
        SaveIconController.Save();
        _region.Saved = true;
    }
}