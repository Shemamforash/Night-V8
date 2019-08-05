using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Generation;
using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using InventorySystem;
using NUnit.Framework;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

public abstract class ContainerController
{
	public static readonly List<ContainerBehaviour> Containers = new List<ContainerBehaviour>();
	private readonly       Vector2                  _position;
	protected              NamedItem                Item;
	protected              string                   PrefabLocation = "Container";
	protected              Sprite                   Sprite;

	protected ContainerController(Vector2 position)
	{
		_position = position;
		Sprite    = ResourceTemplate.GetSprite("Loot");
	}

	public virtual ContainerBehaviour CreateObject(bool autoReveal = false)
	{
		GameObject prefab    = Resources.Load<GameObject>("Prefabs/Combat/Containers/" + PrefabLocation);
		GameObject container = Object.Instantiate(prefab);
		container.transform.position   = _position;
		container.transform.localScale = Vector3.one;
		ContainerBehaviour cb = container.GetComponent<ContainerBehaviour>();
		cb.SetContainerController(this);
		if (autoReveal) cb.Reveal();
		if (Item is GearItem) cb.HideOutline();
		return cb;
	}

	private void TakeResource(Player player)
	{
		ResourceItem     resourceItem     = (ResourceItem) Item;
		ResourceTemplate resourceTemplate = resourceItem.Template;
		Assert.IsNotNull(resourceTemplate);

		switch (resourceTemplate.ResourceType)
		{
			case ResourceType.Plant:
				--CharacterManager.CurrentRegion().FoodSourceCount;
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
			case Accessory accessory:
				Inventory.Move(accessory);
				break;
			case Inscription inscription:
				Inventory.Move(inscription);
				break;
			case Weapon weapon:
				Inventory.Move(weapon);
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
		int    quantity                 = Item is ResourceItem item ? item.Quantity() : 1;
		string contentsName             = Item.Name;
		if (quantity != 1) contentsName += " x" + quantity;
		return contentsName;
	}

	public Sprite GetSprite() => Sprite;
}