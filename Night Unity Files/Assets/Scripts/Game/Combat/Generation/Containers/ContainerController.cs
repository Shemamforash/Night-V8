using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Exploration.Environment;
using Game.Exploration.Weather;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

public abstract class ContainerController //: DesolationInventory 
{
    public static readonly List<ContainerBehaviour> Containers = new List<ContainerBehaviour>();
    private readonly Vector2 _position;
    protected readonly Inventory _inventory;
    protected string PrefabLocation = "Container";

    protected ContainerController(Vector2 position, string name)
    {
        _position = position;
        _inventory = new Inventory(name);
    }

    public void CreateObject(bool autoReveal = false)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Combat/" + PrefabLocation);
        GameObject container = GameObject.Instantiate(prefab);
        container.transform.position = _position;
        container.transform.localScale = Vector3.one;
        ContainerBehaviour cb = container.GetComponent<ContainerBehaviour>();
        cb.SetContainerController(this);
        if (autoReveal) cb.StartReveal();
    }

    public Inventory Inventory()
    {
        if (!EnvironmentManager.BelowFreezing()) return _inventory;
        int waterQuantity = _inventory.GetResourceQuantity("Water");
        if (waterQuantity <= 0) return _inventory;
        _inventory.DecrementResource("Water", waterQuantity);
        _inventory.IncrementResource("Ice", waterQuantity);
        return _inventory;
    }
}