using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Exploration.Environment;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

public abstract class ContainerController
{
    public static readonly List<ContainerBehaviour> Containers = new List<ContainerBehaviour>();
    private readonly Vector2 _position;
    protected readonly Inventory _inventory;
    protected string PrefabLocation = "Container";
    protected string ImageLocation;

    protected ContainerController(Vector2 position, string name)
    {
        _position = position;
        _inventory = new Inventory(name);
        ImageLocation = "Loot";
    }

    public virtual ContainerBehaviour CreateObject(bool autoReveal = false)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Combat/Containers/" + PrefabLocation);
        GameObject container = GameObject.Instantiate(prefab);
        container.transform.position = _position;
        container.transform.localScale = Vector3.one;
        ContainerBehaviour cb = container.GetComponent<ContainerBehaviour>();
        cb.SetContainerController(this);
        if (autoReveal) cb.Reveal();
        return cb;
    }

    private Inventory Inventory()
    {
        if (!EnvironmentManager.BelowFreezing()) return _inventory;
        int waterQuantity = _inventory.GetResourceQuantity("Water");
        if (waterQuantity <= 0) return _inventory;
        _inventory.DecrementResource("Water", waterQuantity);
        _inventory.IncrementResource("Ice", waterQuantity);
        return _inventory;
    }

    public virtual void Take()
    {
        int resourceBonus = (int) PlayerCombat.Instance.Player.Attributes.ResourceFindModifier;
        if (resourceBonus != 0)
        {
            Inventory().Contents().ForEach(i =>
            {
                ResourceTemplate resourceTemplate = i.Template;
                if (resourceTemplate == null) return;
                if (resourceTemplate.ResourceType != "Resource") return;
                if (resourceBonus > 0) i.Increment(resourceBonus);
                else
                {
                    int quantity = i.Quantity();
                    if (quantity - resourceBonus <= 0) resourceBonus = quantity - 1;
                    i.Decrement(resourceBonus);
                }
            });
        }

        Inventory().MoveAllResources(CharacterManager.SelectedCharacter.Inventory());
    }

    public string GetContents()
    {
        return _inventory.Contents()[0].Name;
    }

    public string GetImageLocation()
    {
        return ImageLocation;
    }
}