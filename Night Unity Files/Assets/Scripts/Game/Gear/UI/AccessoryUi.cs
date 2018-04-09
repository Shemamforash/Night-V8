using Game.Gear.Armour;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gear.UI
{
    public class AccessoryUi : GearUi
    {
        public AccessoryUi(MyGameObject linkedObject, Transform parent, string prefabLocation = "Prefabs/Inventory/WeaponItem") : base(linkedObject, parent, prefabLocation)
        {
            DetailedSection.GetComponent<LayoutElement>().minHeight = 100;
        }

        public override void Update()
        {
            base.Update();
            Accessory accessory = (Accessory) LinkedObject;
            TitleText.text = accessory.GetSummary();
            SubTitleText.text = "effect";
            ModifierSection.SetActive(false);
            StatsSection.SetActive(false);
        }
    }
}