using System.Collections.Generic;
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
        if (Item is GearItem) cb.HideOutline();
        return cb;
    }

    private void TakeResource(Player player)
    {
        ResourceItem resourceItem = (ResourceItem) Item;
        int resourceBonus = (int) PlayerCombat.Instance.Player.Attributes.ResourceFindModifier;
        if (resourceBonus != 0)
        {
            ResourceTemplate resourceTemplate = resourceItem.Template;
            Assert.IsNotNull(resourceTemplate);
            Assert.IsTrue(resourceTemplate.ResourceType == ResourceType.Resource);
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
                    --CombatManager.GetCurrentRegion().WaterSourceCount;
                    player.BrandManager.IncreaseWaterFound();
                    break;
                case ResourceType.Plant:
                    --CombatManager.GetCurrentRegion().FoodSourceCount;
                    player.BrandManager.IncreaseFoodFound();
                    break;
                case ResourceType.Meat:
                    player.BrandManager.IncreaseFoodFound();
                    break;
                case ResourceType.Resource:
                    --CombatManager.GetCurrentRegion().ResourceSourceCount;
                    player.BrandManager.IncreaseResourceFound();
                    break;
            }
        }

        Inventory.IncrementResource(resourceItem.Name, resourceItem.Quantity());
    }

    private void TakeItem(Player player)
    {
        if (Item is GearItem) player.BrandManager.IncreaseItemsFound();

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
        CombatManager.GetCurrentRegion().Containers.Remove(this);
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

    public string GetImageLocation()
    {
        return ImageLocation;
    }
}