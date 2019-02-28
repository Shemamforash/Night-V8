using System;
using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Generation.Shrines;
using Game.Combat.Misc;
using SamsHelper.Libraries;
using UnityEngine;

public class SaveStoneBehaviour : BasicShrineBehaviour, ICombatEvent
{
    private static GameObject _saveStonePrefab;
    private static SaveStoneBehaviour _instance;

    public void Awake()
    {
        _instance = this;
        List<Vector2> points = new List<Vector2>();
        points.Add(new Vector2(-0.2f, 0.2f));
        points.Add(new Vector2(0.2f, 0.2f));
        points.Add(new Vector2(0.2f, -0.2f));
        points.Add(new Vector2(-0.2f, -0.2f));
        Polygon b = new Polygon(points, Vector2.zero);
        WorldGrid.AddBarrier(b);
        if (CombatManager.GetCurrentRegion().MonumentUsed) return;
        ParticleSystem[] particles = _instance.transform.GetComponentsInChildren<ParticleSystem>();
        Array.ForEach(particles, p => p.Play());
    }

    public static void SetUsed()
    {
        CombatManager.GetCurrentRegion().MonumentUsed = true;
        ParticleSystem[] particles = _instance.transform.GetComponentsInChildren<ParticleSystem>();
        Array.ForEach(particles, p => p.Stop());
    }

    public static SaveStoneBehaviour Instance()
    {
        return _instance;
    }

    protected override void StartShrine()
    {
    }

    public static void Generate()
    {
        if (_saveStonePrefab == null) _saveStonePrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Save Stone");
        GameObject saveStoneObject = Instantiate(_saveStonePrefab);
        saveStoneObject.GetComponent<SaveStoneBehaviour>().Initialise();
    }

    private void Initialise()
    {
        transform.position = Vector2.zero;
        WorldGrid.AddBlockingArea(Vector2.zero, 0.5f);
    }

    public float InRange()
    {
        return IsInRange ? 1 : -1;
    }

    public string GetEventText()
    {
        return "Make an offering upon the alter [T]";
    }

    public void Activate()
    {
        DismantleMenuController.Show();
    }
}