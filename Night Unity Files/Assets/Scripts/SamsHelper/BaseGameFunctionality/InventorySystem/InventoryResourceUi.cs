using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
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