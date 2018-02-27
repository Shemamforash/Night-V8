using System;
using System.Collections.Generic;
using System.Xml;
using Game.Characters;
using Game.Gear.Weapons;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;

namespace Game.Gear.Armour
{
    public class ArmourController : IPersistenceTemplate
    {
        private ArmourPlate _plateOne, _plateTwo;
        private readonly Character _character;
        private event Action _onArmourChange;

        public ArmourController(Character character)
        {
            _character = character;
        }

        public void SetPlateOne(ArmourPlate plate)
        {
            _plateOne?.Unequip();
            plate.Equip(_character.Inventory());
            _plateOne = plate;
            _onArmourChange?.Invoke();
        }

        public void AddOnArmourChange(Action a)
        {
            _onArmourChange += a;
        }

        public void SetPlateTwo(ArmourPlate plate)
        {
            _plateTwo.Unequip();
            plate.Equip(_character.Inventory());
            _plateTwo = plate;
            _onArmourChange?.Invoke();
        }

        public void AutoFillSlots()
        {
            SetPlateOne(ArmourPlate.GeneratePlate(ItemQuality.Radiant));
            SetPlateTwo(ArmourPlate.GeneratePlate(ItemQuality.Radiant));
        }

        public int GetProtectionLevel()
        {
            //todo plate damage
            return (int) (_plateOne.Weight + _plateTwo.Weight);
        }

        public void Load(XmlNode doc, PersistenceType saveType)
        {
        }

        public XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            return doc;
        }

        public int GetMaxProtectionLevel()
        {
            return (int) (_plateOne.Weight + _plateTwo.Weight);
        }

        public ArmourPlate GetPlateOne()
        {
            return _plateOne;
        }
        
        public ArmourPlate GetPlateTwo()
        {
            return _plateTwo;
        }
    }
}