using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Generation;
using Game.Exploration.Environment;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

public abstract class ContainerController
{
    public static readonly List<ContainerBehaviour> Containers = new List<ContainerBehaviour>();
    private readonly Vector2 _position;
    protected readonly Inventory _inventory;
    protected string PrefabLocation = "Container";
    private ContainerBehaviour _containerBehaviour;
    protected string ImageLocation;

    protected ContainerController(Vector2 position, string name)
    {
        _position = position;
        _inventory = new Inventory(name);
        ImageLocation = "Loot";
    }

    public void CreateObject(bool autoReveal = false)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Combat/Containers/" + PrefabLocation);
        GameObject container = GameObject.Instantiate(prefab);
        container.transform.position = _position;
        container.transform.localScale = Vector3.one;
        ContainerBehaviour cb = container.GetComponent<ContainerBehaviour>();
        _containerBehaviour = cb;
        cb.SetContainerController(this);
        if (autoReveal) cb.Reveal();
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

    public void Take()
    {
        Inventory().MoveAllResources(CharacterManager.SelectedCharacter.Inventory());
        _containerBehaviour.StartCoroutine(_containerBehaviour.Fade());
    }

    public string GetContents()
    {
        Debug.Log(Inventory().Name + "  " + Inventory().Contents().Count);
        return _inventory.Contents()[0].Name;
    }

    public string GetImageLocation()
    {
        return ImageLocation;
    }
}