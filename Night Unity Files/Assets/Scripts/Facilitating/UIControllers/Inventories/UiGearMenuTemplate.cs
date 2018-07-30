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
        protected Player CurrentPlayer;

        public abstract bool GearIsAvailable();
        public abstract void SelectGearItem(MyGameObject item, UiGearMenuController.GearUi gearUi);

        public virtual void Show(Player player)
        {
            gameObject.SetActive(true);
            CurrentPlayer = player;
            UiGearMenuController.Instance().SelectGear();
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        public abstract void CompareTo(MyGameObject comparisonItem);
        public abstract void StopComparing();
        public abstract List<MyGameObject> GetAvailableGear();
        public abstract void Equip(int selectedGear);
        public abstract Button GetGearButton();
    }
}