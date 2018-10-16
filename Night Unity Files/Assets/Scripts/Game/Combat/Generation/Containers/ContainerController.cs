using System.Collections.Generic;
using System.Linq;
using Game.Characters;
using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Exploration.Environment;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

public abstract class ContainerController
{
    public static readonly List<ContainerBehaviour> Containers = new List<ContainerBehaviour>();
    private readonly Vector2 _position;
    protected InventoryItem Item;
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
        int resourceBonus = (int) PlayerCombat.Instance.Player.Attributes.ResourceFindModifier;
        if (resourceBonus != 0)
        {
            ResourceTemplate resourceTemplate = Item.Template;
            if (resourceTemplate == null) return;
            if (resourceTemplate.ResourceType != "Resource") return;
            if (resourceBonus > 0) Item.Increment(resourceBonus);
            else
            {
                int quantity = Item.Quantity();
                if (quantity - resourceBonus <= 0) resourceBonus = quantity - 1;
                Item.Decrement(resourceBonus);
            }
        }

        Player player = CharacterManager.SelectedCharacter;
        if (Item.Template != null)
        {
            switch (Item.Template.ResourceType)
            {
                case "Water":
                    player.BrandManager.IncreaseWaterFound();
                    break;
                case "Plant":
                    player.BrandManager.IncreaseFoodFound();
                    break;
                case "Meat":
                    player.BrandManager.IncreaseFoodFound();
                    break;
                case "Resource":
                    player.BrandManager.IncreaseResourceFound();
                    break;
            }
        }
        else
        {
            player.BrandManager.IncreaseItemsFound();
        }

        Inventory.Move(Item, Item.Quantity());
    }

    public string GetContents()
    {
        return Item == null ? "" : Item.Name;
    }

    public string GetImageLocation()
    {
        return ImageLocation;
    }
}