using System.Collections.Generic;
using System.Xml;
using DefaultNamespace;
using Extensions;
using Facilitating.Persistence;
using Facilitating.UIControllers;
using Facilitating.UIControllers.Inventories;
using Game.Global;

using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class UiJournalController : UiInventoryMenuController
{
	private static EnhancedText   _journalDescription;
	private static bool           _unlocked;
	private        ListController _journalList;
	private        CanvasGroup    _noJournalGroup;

	public static void Load(XmlNode root)
	{
		_unlocked = root.ParseBool("Journal");
	}

	public static void Save(XmlNode root)
	{
		root.CreateChild("Journal", _unlocked);
	}

	public override bool Unlocked()
	{
		if (!_unlocked) _unlocked = JournalEntry.GetUnlockedEntries().Count != 0;
		return _unlocked;
	}

	protected override void Initialise()
	{
		_journalList.Initialise(typeof(JournalElement), o => { }, null, GetAvailableJournalEntries);
		_journalList.SetOnItemHover(UpdateJournalDescription);
	}

	protected override void CacheElements()
	{
		_journalList        = gameObject.FindChildWithName<ListController>("List");
		_journalDescription = gameObject.FindChildWithName<EnhancedText>("Text");
		_noJournalGroup     = gameObject.FindChildWithName<CanvasGroup>("No Journals");
	}

	private void UpdateJournalDescription(object obj)
	{
		if (obj == null)
		{
			_journalDescription.SetText("");
			_noJournalGroup.alpha = 1;
			return;
		}

		JournalEntry entry = (JournalEntry) obj;
		_noJournalGroup.alpha = 0;
		_journalDescription.SetText(entry.Text);
	}

	protected override void OnShow()
	{
		UiGearMenuController.SetCloseButtonAction(UiGearMenuController.Close);
		_journalList.Show();
	}

	private List<object> GetAvailableJournalEntries() => JournalEntry.GetUnlockedEntries().ToObjectList();

	private class JournalElement : ListElement
	{
		private EnhancedText _nameText;

		protected override void SetVisible(bool visible)
		{
			_nameText.SetColor(visible ? Color.white : UiAppearanceController.InvisibleColour);
		}

		protected override void CacheUiElements(Transform transform)
		{
			_nameText = transform.GetComponent<EnhancedText>();
		}

		protected override void Update(object o, bool isCentreItem)
		{
			JournalEntry entry = (JournalEntry) o;
			_nameText.SetText(entry.Title);
		}

		protected override void UpdateCentreItemEmpty()
		{
			_nameText.SetText("");
			_journalDescription.SetText("No Journal Entries Found");
		}

		public override void SetColour(Color c)
		{
			_nameText.SetColor(c);
		}
	}
}