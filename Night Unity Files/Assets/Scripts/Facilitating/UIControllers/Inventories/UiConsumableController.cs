using System.Collections.Generic;
using System.Xml;
using DefaultNamespace;
using Extensions;
using Facilitating.Persistence;
using Facilitating.UIControllers;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Global;
using InventorySystem;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

public class UiConsumableController : UiInventoryMenuController
{
	private static UIAttributeController _uiAttributeController;
	private static bool                  _unlocked;
	private        ListController        _consumableList;

	protected override void Initialise()
	{
		List<ListElement> listElements = new List<ListElement>();
		listElements.Add(new InventoryElement());
		listElements.Add(new InventoryElement());
		listElements.Add(new InventoryElement());
		listElements.Add(new InventoryElement());
		listElements.Add(new CentreInventoryElement());
		listElements.Add(new InventoryElement());
		listElements.Add(new InventoryElement());
		listElements.Add(new InventoryElement());
		listElements.Add(new InventoryElement());
		_consumableList.Initialise(listElements, Consume, null, GetAvailableItems);
	}

	public static void Load(XmlNode root)
	{
		_unlocked = root.ParseBool("Consumable");
	}

	public static void Save(XmlNode root)
	{
		root.CreateChild("Consumable", _unlocked);
	}

	public override bool Unlocked()
	{
		if (!_unlocked) _unlocked = Inventory.Consumables().Count != 0;
		return _unlocked;
	}

	private static void UpdateConditions(ResourceItem resource = null)
	{
		float attributeOffset = 0;
		if (resource != null)
		{
			if (resource.Template.ResourceType == ResourceType.Plant) attributeOffset = resource.Template.EffectBonus;
		}

		if (attributeOffset == 0)
			_uiAttributeController.UpdateAttributes(CharacterManager.SelectedCharacter);
		else
			_uiAttributeController.UpdateAttributesOffset(CharacterManager.SelectedCharacter, resource.Template.AttributeType, attributeOffset);
	}

	protected override void CacheElements()
	{
		_uiAttributeController = gameObject.FindChildWithName<UIAttributeController>("Attributes");
		_consumableList        = gameObject.FindChildWithName<ListController>("List");
	}

	protected override void OnShow()
	{
		UiGearMenuController.SetCloseButtonAction(UiGearMenuController.Close);
		UpdateConditions();
		_consumableList.Show();
	}

	private List<object> GetAvailableItems() => Inventory.Consumables().ToObjectList();

	private void Consume(object consumableObject)
	{
		Consumable consumable = consumableObject as Consumable;
		if (consumable == null) return;
		if (!consumable.CanConsume()) return;
		consumable.Consume();
		switch (consumable.Template.ResourceType)
		{
			case ResourceType.Water:
				UiGearMenuController.PlayAudio(AudioClips.EatWater);
				break;
			case ResourceType.Meat:
				UiGearMenuController.PlayAudio(AudioClips.EatMeat);
				break;
			case ResourceType.Plant:
				UiGearMenuController.PlayAudio(AudioClips.EatPlant);
				break;
			case ResourceType.Potion:
				UiGearMenuController.PlayAudio(AudioClips.EatPotion);
				break;
		}

		UpdateConditions();
	}

	private class CentreInventoryElement : InventoryElement
	{
		private EnhancedText _descriptionText;

		protected override void UpdateCentreItemEmpty()
		{
			base.UpdateCentreItemEmpty();
			_descriptionText.SetText("-");
		}

		protected override void Update(object o, bool isCentreItem)
		{
			base.Update(o, isCentreItem);
			ResourceItem resource = (ResourceItem) o;
			_descriptionText.SetText(resource.Template.Description);
			UpdateConditions(resource);
		}

		protected override void SetVisible(bool visible)
		{
			base.SetVisible(visible);
			_descriptionText.gameObject.SetActive(visible);
		}

		protected override void CacheUiElements(Transform transform)
		{
			base.CacheUiElements(transform);
			_descriptionText = transform.gameObject.FindChildWithName<EnhancedText>("Description");
		}

		public override void SetColour(Color c)
		{
			base.SetColour(c);
			c.a = 0.6f;
			_descriptionText.SetColor(c);
		}
	}

	private class InventoryElement : ListElement
	{
		private EnhancedText _consumableText, _nameText, _effectText;
		private Image        _icon;

		protected override void UpdateCentreItemEmpty()
		{
			_consumableText.SetText("");
			_nameText.SetText("Inventory is Empty");
			_effectText.SetText("");
			_icon.SetAlpha(0f);
		}

		protected override void Update(object o, bool isCentreItem)
		{
			ResourceItem resource = (ResourceItem) o;
			string       nameText = resource.Quantity() > 1 ? resource.Name + " x" + resource.Quantity() : resource.Name;
			_nameText.SetText(nameText);
			string consumableText = "";
			if (resource is Consumable consumable)
			{
				consumableText = !consumable.CanConsume() ? "Cannot Consume" : "Consumable";
			}
			else
			{
				consumableText = "Crafting Resource";
			}

			_consumableText.SetText(consumableText);
			_effectText.SetText(resource.Template.EffectString);
			_icon.SetAlpha(1f);
			_icon.sprite = resource.Template.Sprite;
		}

		protected override void SetVisible(bool visible)
		{
			_consumableText.gameObject.SetActive(visible);
			_nameText.gameObject.SetActive(visible);
			_effectText.gameObject.SetActive(visible);
			_icon.gameObject.SetActive(visible);
		}

		protected override void CacheUiElements(Transform transform)
		{
			_consumableText = transform.gameObject.FindChildWithName<EnhancedText>("Consumable");
			_nameText       = transform.gameObject.FindChildWithName<EnhancedText>("Name");
			_effectText     = transform.gameObject.FindChildWithName<EnhancedText>("Effect");
			_icon           = transform.gameObject.FindChildWithName<Image>("Icon");
		}

		public override void SetColour(Color c)
		{
			_consumableText.SetColor(c);
			_nameText.SetColor(c);
			_effectText.SetColor(c);
			_icon.color = c;
		}
	}
}