namespace Game.Gear.Armour
{
    public class Accessory : EquippableItem
    {
        protected Accessory(string name, float weight) : base(name, weight, GearSlot.Accessory)
        {
        }
    }
}