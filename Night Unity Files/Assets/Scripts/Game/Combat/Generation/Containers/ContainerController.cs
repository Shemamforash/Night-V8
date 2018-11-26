﻿using System.Collections.Generic;
using System.Linq;
using Game.Characters;
using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Exploration.Environment;
using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;
using InventorySystem;
using NUnit.Framework;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

public abstract class ContainerController
{
    public static readonly List<ContainerBehaviour> Containers = new List<ContainerBehaviour>();
    private readonly Vector2 _position;
    protected NamedItem Item;
    protected string PrefabLocation = "Container";
    protected string ImageLocation;

    protected ContainerController(Vector2 position)
    {
        _position = position;
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

    public virtual void Take()
    {
        Player player = CharacterManager.SelectedCharacter;

        int resourceBonus = (int) PlayerCombat.Instance.Player.Attributes.ResourceFindModifier;
        if (resourceBonus != 0)
        {
            ResourceItem resourceItem = Item as ResourceItem;
            if (resourceItem != null)
            {
                ResourceTemplate resourceTemplate = resourceItem.Template;
                if (resourceTemplate == null) return;
                if (resourceTemplate.ResourceType != ResourceType.Resource) return;
                if (resourceBonus > 0) resourceItem.Increment(resourceBonus);
                else
                {
                    int quantity = resourceItem.Quantity();
                    if (quantity - resourceBonus <= 0) resourceBonus = quantity - 1;
                    resourceItem.Decrement(resourceBonus);
                }

                switch (resourceTemplate.ResourceType)
                {
                    case ResourceType.Water:
                        player.BrandManager.IncreaseWaterFound();
                        break;
                    case ResourceType.Plant:
                        player.BrandManager.IncreaseFoodFound();
                        break;
                    case ResourceType.Meat:
                        player.BrandManager.IncreaseFoodFound();
                        break;
                    case ResourceType.Resource:
                        player.BrandManager.IncreaseResourceFound();
                        break;
                }
            }
        }

        if (Item is GearItem) player.BrandManager.IncreaseItemsFound();

        if (Item is Weapon) Inventory.Move((Weapon) Item);
        else if (Item is Armour) Inventory.Move((Armour) Item);
        else if (Item is Accessory) Inventory.Move((Accessory) Item);
        else if (Item is Inscription) Inventory.Move((Inscription) Item);
        else Inventory.Move((ResourceItem) Item);
    }

    public virtual string GetContents()
    {
        Assert.IsNotNull(Item);
        int quantity = Item is ResourceItem ? ((ResourceItem) Item).Quantity() : 1;
        string contentsName = Item.Name;
        if (quantity != 1) contentsName += " x" + quantity;
        return contentsName;
    }

    public string GetImageLocation()
    {
        return ImageLocation;
    }
}