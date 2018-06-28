using System.Collections.Generic;
using Game.Combat.Generation;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

public abstract class ContainerController //: DesolationInventory 
{
    public static readonly List<ContainerBehaviour> Containers = new List<ContainerBehaviour>();
    private readonly Vector2 _position;
    public readonly Inventory Inventory;
    protected string PrefabLocation = "Container";

    protected ContainerController(Vector2 position, string name)
    {
        _position = position;
        Inventory = new Inventory(name);
    }

    public void CreateObject()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Combat/" + PrefabLocation);
        GameObject container = GameObject.Instantiate(prefab);
        container.transform.position = _position;
        container.transform.localScale = Vector3.one;
        ContainerBehaviour cb = container.GetComponent<ContainerBehaviour>();
        cb.SetContainerController(this);
    }
}