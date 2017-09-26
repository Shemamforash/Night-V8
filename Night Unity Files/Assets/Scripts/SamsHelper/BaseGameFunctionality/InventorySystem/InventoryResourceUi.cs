using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Facilitating.UI.Inventory
{
    public class InventoryResourceUi : InventoryItemUi
    {
        public InventoryResourceUi(MyGameObject item, Transform parent, Direction direction = Direction.None) : base(item, parent, direction)
        {
        }
    }
}