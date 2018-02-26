using System;
using System.Collections.Generic;
using System.Xml;
using Game.Characters;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;
namespace Game.Gear.Armour
{
    public class ArmourController : IPersistenceTemplate
    {
        private readonly List<ArmourPlate> _plates = new List<ArmourPlate>();
        private Number ArmourSlots = new Number(0, 0, 0);
        private Character _character;

        public ArmourController(Character character)
        {
            _character = character;
        }

        public void AddOnArmourChange(Action<Number> a)
        {
            ArmourSlots.AddOnValueChange(a);
        }

        public List<ArmourPlate> GetPlates()
        {
            return _plates;
        }

        public void RemovePlate(ArmourPlate plate)
        {
            _plates.Remove(plate);
            plate.Unequip();
            ArmourSlots.Decrement(plate.Weight);
        }

        public bool DoesPlateFit(ArmourPlate plate)
        {
            return ArmourSlots.CurrentValue() + plate.Weight <= ArmourSlots.Max;
        }

        public void AddPlate(ArmourPlate plate)
        {
            plate.Equip(_character.Inventory());
            _plates.Add(plate);
            ArmourSlots.Increment(plate.Weight);
        }

        public void UpgradeSlots()
        {
            ArmourSlots.Max = ArmourSlots.Max + 1;
        }

        public void AutoFillSlots(int slots)
        {
            ArmourSlots.Max = slots;
            while (GetProtectionLevel() != slots)
            {
                ArmourPlate plate = ArmourPlate.CreatePlate(ArmourPlateType.Leather);
                AddPlate(plate);
            }
        }

        public int GetProtectionLevel()
        {
            return (int) ArmourSlots.CurrentValue();
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
            return (int) ArmourSlots.Max;
        }
    }
}