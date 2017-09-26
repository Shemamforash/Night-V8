using Facilitating.UI.Inventory;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.World.Region
{
    public class RegionUi : InventoryItemUi
    {
        private bool _discovered;

        public RegionUi(Region item, Transform parent, Direction direction = Direction.None) : base(item, parent, direction)
        {
            LeftActionButton.gameObject.SetActive(false);
            RightActionButton.gameObject.SetActive(false);
            WeightText.gameObject.SetActive(false);
            OnActionPress(delegate { RegionManager.UpdateRegionInfo(item); });
            DisableBorder();
        }

        public override void Update()
        {
            NameText.text = LinkedObject.Name;
            TypeText.text = ((Region) LinkedObject).Type();
            SummaryText.text = ((Region) LinkedObject).Distance() + " hrs";
        }

        public bool Discovered => _discovered;
        public void Discover() => _discovered = true;
    }
}