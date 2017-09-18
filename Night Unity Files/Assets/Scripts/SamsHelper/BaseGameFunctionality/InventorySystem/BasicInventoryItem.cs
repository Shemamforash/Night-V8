namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public abstract class BasicInventoryItem
    {
        private readonly string _name;
        private readonly float _weight;

        public BasicInventoryItem(string name, float weight)
        {
            _name = name;
            _weight = weight;
        }

        public string Name()
        {
            return _name;
        }

        public virtual string ExtendedName()
        {
            return _name;
        }

        public float Weight()
        {
            return _weight;
        }

        public virtual int Quantity()
        {
            return 1;
        }

        public float TotalWeight()
        {
            return Helper.Round(Weight() * Quantity(), 1);
        }
    }
}