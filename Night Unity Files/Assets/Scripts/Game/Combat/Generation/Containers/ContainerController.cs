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
    protected Sprite Sprite;

    protected ContainerController(Vector2 position)
    {
        _position = position;
        Sprite = ResourceTemplate.GetSprite("Loot");
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
        if (Item is GearItem) cb.HideOutline();
        return cb;
    }

    private void TakeResource(Player player)
    {
        ResourceItem resourceItem = (ResourceItem) Item;
        int resourceBonus = (int) PlayerCombat.Instance.Player.Attributes.ResourceFindModifier;
        ResourceTemplate resourceTemplate = resourceItem.Template;
        Assert.IsNotNull(resourceTemplate);

        if (resourceBonus != 0 && resourceTemplate.ResourceType == ResourceType.Resource)
        {
            resourceItem.Increment(resourceBonus);
        }

        switch (resourceTemplate.ResourceType)
        {
            case ResourceType.Water:
                --CharacterManager.CurrentRegion().WaterSourceCount;
                player.BrandManager.IncreaseWaterFound();
                break;
            case ResourceType.Plant:
                --CharacterManager.CurrentRegion().FoodSourceCount;
                player.BrandManager.IncreaseFoodFound();
                break;
            case ResourceType.Meat:
                player.BrandManager.IncreaseFoodFound();
                break;
            case ResourceType.Resource:
                --CharacterManager.CurrentRegion().ResourceSourceCount;
                player.BrandManager.IncreaseResourceFound();
                break;
        }

        player.BrandManager.IncreaseItemsFound();
        Inventory.IncrementResource(resourceItem.Name, resourceItem.Quantity());
    }

    private void TakeItem(Player player)
    {
        player.BrandManager.IncreaseItemsFound();
        switch (Item)
        {
            case Weapon weapon:
                Inventory.Move(weapon);
                break;
            case Accessory accessory:
                Inventory.Move(accessory);
                break;
            case Inscription inscription:
                Inventory.Move(inscription);
                break;
        }
    }

    public virtual void Take()
    {
        CharacterManager.CurrentRegion().Containers.Remove(this);
        Player player = CharacterManager.SelectedCharacter;
        switch (Item)
        {
            case ResourceItem _:
                TakeResource(player);
                break;
            case GearItem _:
                TakeItem(player);
                break;
        }

        CombatLogController.PostLog(GetLogText());
    }

    protected abstract string GetLogText();

    public virtual string GetContents()
    {
        Assert.IsNotNull(Item);
        int quantity = Item is ResourceItem item ? item.Quantity() : 1;
        string contentsName = Item.Name;
        if (quantity != 1) contentsName += " x" + quantity;
        return contentsName;
    }

    public Sprite GetSprite() => Sprite;
}