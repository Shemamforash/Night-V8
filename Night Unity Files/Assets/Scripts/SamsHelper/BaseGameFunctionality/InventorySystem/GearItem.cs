using System.Collections.Generic;
using System.Xml;
using Extensions;
using Facilitating.Persistence;
using Game.Characters;
using Game.Gear;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
	public abstract class GearItem : NamedItem
	{
		private static int                     _idCounter;
		private        Dictionary<string, int> _dismantleRewards;
		private        int                     _id;
		private        ItemQuality             _itemQuality;
		public         Character               EquippedCharacter;

		protected GearItem(string name, ItemQuality itemQuality) : base(name)
		{
			SetId(_idCounter);
			++_idCounter;
			SetQuality(itemQuality);
		}

		private void SetId(int id)
		{
			_id = id;
			CalculateDismantleRewards();
		}

		public virtual XmlNode Save(XmlNode root)
		{
			root.CreateChild("Name",    Name);
			root.CreateChild("Id",      _id);
			root.CreateChild("Quality", (int) _itemQuality);
			return root;
		}

		protected virtual void Load(XmlNode root)
		{
			Name = root.ParseString("Name");
			_id  = root.ParseInt("Id");
			SetId(_id);
			if (_id > _idCounter) _idCounter = _id + 1;
			_itemQuality = (ItemQuality) root.ParseInt("Quality");
		}

		public ItemQuality Quality() => _itemQuality;

		private void SetQuality(ItemQuality quality)
		{
			_itemQuality = quality;
		}

		public virtual void Equip(Character character)
		{
			EquippedCharacter = character;
		}

		public virtual void UnEquip()
		{
			EquippedCharacter = null;
		}

		public abstract string GetSummary();

		public int ID() => _id;

		public Dictionary<string, int> GetDismantleRewards() => _dismantleRewards;

		protected virtual void CalculateDismantleRewards()
		{
			_dismantleRewards = new Dictionary<string, int>();
		}

		protected void AddReward(string reward, int quantity)
		{
			if (!_dismantleRewards.ContainsKey(reward)) _dismantleRewards.Add(reward, 0);
			quantity                  = _dismantleRewards[reward] + quantity;
			_dismantleRewards[reward] = quantity;
		}
	}
}