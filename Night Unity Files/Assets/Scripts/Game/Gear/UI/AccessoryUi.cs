using Game.Gear.Armour;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Gear.UI
{
    public class AccessoryUi : GearUi
    {
        public AccessoryUi(MyGameObject linkedObject, Transform parent, string prefabLocation = "Prefabs/Inventory/WeaponItem") : base(linkedObject, parent, prefabLocation)
        {
        }

        public override void Update()
        {
            base.Update();
            Accessory accessory = (Accessory)LinkedObject;
            TitleText.gameObject.SetActive(false);
            SubTitleText.text = accessory.Effect;
            _modifierSection.SetActive(false);
            _statsSection.SetActive(false);
        }
    }
}