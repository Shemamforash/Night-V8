using System.Collections.Generic;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

public class ContainerController //: DesolationInventory 
{
    public static readonly List<ContainerBehaviour> Containers = new List<ContainerBehaviour>();
    private readonly Vector2 _position;
    public readonly Inventory Inventory;
    private string _prefabLocation;

    private ContainerController(Vector2 position, string name)
    {
        _position = position;
        Inventory = new Inventory(name);
    }

    public static ContainerController CreateWaterSource(Vector2 position)
    {
        ContainerController container = new ContainerController(position, "Source");
        container.Inventory.IncrementResource("Water", 5);
        container.Inventory.SetReadonly(true);
        container._prefabLocation = "Puddle";
        return container;
    }

    public static ContainerController CreateFoodSource(Vector2 position)
    {
        string sourceName;
        int quantity;
        int rand = Random.Range(0, 4);
        if (rand != 0)
        {
            sourceName = "Bush";
            quantity = 1;
        }
        else
        {
            sourceName = "Tree";
            quantity = 3;
        }

        ContainerController container = new ContainerController(position, sourceName);
        container.Inventory.IncrementResource("Fruit", quantity);
        container.Inventory.SetReadonly(true);
        container._prefabLocation = sourceName;
        return container;
    }

    public static ContainerController CreateEnemyLoot(Vector2 position, Enemy e)
    {
        ContainerController container = new ContainerController(position, e.Name);
        if (Random.Range(0, 10) != 0 || e.Weapon == null) return null;
        container.Inventory.Move(e.Weapon, 1);
        container.Inventory.SetReadonly(true);
        container._prefabLocation = "Container";
        return container;
    }

    public void CreateObject() //InventoryResourceType type)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Combat/" + _prefabLocation);
        GameObject container = GameObject.Instantiate(prefab);
        container.transform.position = _position;
        container.transform.localScale = Vector3.one;
        ContainerBehaviour cb = container.GetComponent<ContainerBehaviour>();
        cb.SetContainerController(this);
    }
}