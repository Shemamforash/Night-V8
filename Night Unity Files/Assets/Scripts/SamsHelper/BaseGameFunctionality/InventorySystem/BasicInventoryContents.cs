namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class BasicInventoryContents
    {
        private readonly string _name;
        private readonly float _weight;

        public BasicInventoryContents(string name, float weight)
        {
            _name = name;
            _weight = weight;
        }

        public string Name()
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
    }
}