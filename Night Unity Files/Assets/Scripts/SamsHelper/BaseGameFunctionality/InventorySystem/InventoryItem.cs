namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class InventoryItem : BasicInventoryContents
    {
        public InventoryItem(string name, float weight) : base(name, weight)
        {
        }
        
        public InventoryItem Clone()
        {
            InventoryItem clone = new InventoryItem(Name(), Weight());
            return clone;
        }
    }
}