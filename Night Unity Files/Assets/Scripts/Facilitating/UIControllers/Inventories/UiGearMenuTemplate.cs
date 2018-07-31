using System.Collections.Generic;
using Game.Characters;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public abstract class UiGearMenuTemplate : MonoBehaviour
    {
        public abstract bool GearIsAvailable();
        public abstract void SelectGearItem(MyGameObject item, UiGearMenuController.GearUi gearUi);

        public virtual void Show()
        {
            gameObject.SetActive(true);
            UiGearMenuController.SelectGear();
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        public virtual void CompareTo(MyGameObject comparisonItem)
        {
        }

        public virtual void StopComparing()
        {
        }

        public abstract List<MyGameObject> GetAvailableGear();
        public abstract void Equip(int selectedGear);

        public virtual Button GetGearButton()
        {
            return GetAvailableGear().Count == 0 ? UiGearMenuController.GetCloseButton().Button() : UiGearMenuController.GetCentreButton().Button();
        }
    }
}