//namespace SamsHelper.BaseGameFunctionality.InventorySystem
//{
//    public class InventoryResource : InventoryItem
//    {
//        private readonly InventoryResourceType _inventoryResourceType;
//        private readonly Number _quantity = new Number();
//
//        public InventoryResource(InventoryResourceType inventoryResourceType, float weight) : base(inventoryResourceType.ToString(), GameObjectType.Resource, weight)
//        {
//            _inventoryResourceType = inventoryResourceType;
//        }
//
//        public override bool Equals(object obj)
//        {
//            InventoryResource other = obj as InventoryResource;
//            if (other != null) return other.Name == Name;
//            return false;
//        }
//
//        public InventoryResourceType GetResourceType()
//        {
//            return _inventoryResourceType;
//        }
//    }
//}

