using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Gear.Armour
{
    public class ArmourController
    {
        private readonly Character _character;
        private Armour _chest, _head;
        private bool _justTookDamage;

        public ArmourController(Character character)
        {
            _character = character;
        }

        public void Load(XmlNode doc)
        {
            XmlNode chestNode = doc.SelectSingleNode("Chest");
            if (chestNode != null)
            {
                int chestId = doc.IntFromNode("Chest");
                Armour armour = Inventory.FindArmour(chestId);
                SetChestArmour(armour);
            }

            XmlNode headNode = doc.SelectSingleNode("Head");
            if (headNode != null)
            {
                int headId = doc.IntFromNode("Head");
                Armour armour = Inventory.FindArmour(headId);
                SetHeadArmour(armour);
            }
        }

        public void Save(XmlNode doc)
        {
            if (_chest != null) doc.CreateChild("Chest", _chest.ID());
            if (_head != null) doc.CreateChild("Head", _head.ID());
        }

        public void TakeDamage(float amount)
        {
            DivideDamage(amount);
        }

        public void Repair(float amount)
        {
            float remaining = amount;
            if (_chest != null) remaining = _chest.Repair(amount);
            _head?.Repair(remaining);
        }

        public bool DidJustTakeDamage()
        {
            bool didTakeDamage = _justTookDamage;
            _justTookDamage = false;
            return didTakeDamage;
        }

        private void TakePlateDamage(ref Armour plate, float damage)
        {
            if (plate == null) return;
            float plateProtection = plate.GetCurrentProtection();
            float totalProtection = GetCurrentProtection();
            float proportion = plateProtection / totalProtection;
            _justTookDamage = true;
            if (!plate.TakeDamage(proportion * damage)) return;
            plate = null;
        }

        private void DivideDamage(float amount)
        {
            TakePlateDamage(ref _chest, amount);
            TakePlateDamage(ref _head, amount);
        }

        public void SetArmour(Armour armour)
        {
            if (armour == null) return;
            if (armour.GetArmourType() == Armour.ArmourType.Chest)
                SetChestArmour(armour);
            else
                SetHeadArmour(armour);
        }

        public float CalculateDamageModifier()
        {
            float currentProtection = GetCurrentProtection();
            currentProtection /= 20f;
            return 1 - currentProtection;
        }

        private void SetChestArmour(Armour chest)
        {
            _chest?.UnEquip();
            if (_character != null) chest?.Equip(_character);
            _chest = chest;
            UpdateArmourView();
        }

        private void SetHeadArmour(Armour head)
        {
            _head?.UnEquip();
            if (_character != null) head?.Equip(_character);
            _head = head;
            UpdateArmourView();
        }

        private void UpdateArmourView()
        {
            if (PlayerCombat.Instance != null) return;
            Player player = _character as Player;
            CharacterView characterView = player?.CharacterView();
            if (characterView == null || characterView.gameObject == null) return;
            characterView.ArmourController.UpdateArmour();
        }

        public int GetCurrentProtection()
        {
            int plateOneCurrent = _head?.GetCurrentProtection() ?? 0;
            int plateTwoCurrent = _chest?.GetCurrentProtection() ?? 0;
            return plateOneCurrent + plateTwoCurrent;
        }

        public void AutoFillSlots(int range)
        {
            int plateOne = range > 5 ? 5 : range;
            int plateTwo = range > 5 ? range - 5 : 0;
            if (plateOne != 0) SetChestArmour(Armour.Create((ItemQuality) plateOne - 1, Armour.ArmourType.Chest));
            if (plateTwo != 0) SetHeadArmour(Armour.Create((ItemQuality) plateTwo - 1, Armour.ArmourType.Head));
        }

        public int GetTotalProtection()
        {
            int chestProtection = _chest?.GetMaxProtection() ?? 0;
            int headProtection = _head?.GetMaxProtection() ?? 0;
            return chestProtection + headProtection;
        }

        public Armour GetChestArmour() => _chest;
        public Armour GetHeadArmour() => _head;
    }
}