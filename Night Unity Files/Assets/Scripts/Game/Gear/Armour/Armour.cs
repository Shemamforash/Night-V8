namespace Game.Gear.Armour
{
    public class Armour : EquippableItem
    {
        protected Armour(string name, float weight) : base(name, weight, GearSlot.Body)
        {
        }
    }
}