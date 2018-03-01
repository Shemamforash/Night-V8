using System.Collections.Generic;
using Game.Characters.Player;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public abstract class UiGearMenuTemplate : MonoBehaviour
    {
        protected Player CurrentPlayer;
        
        public abstract bool GearIsAvailable();
        public abstract void SelectGearItem(GearItem item, UiGearMenuController.GearUi gearUi);

        public virtual void Show(Player player)
        {
            gameObject.SetActive(true);
            CurrentPlayer = player;
            UiGearMenuController.SelectGear();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        public abstract void CompareTo(GearItem comparisonItem);
        public abstract void StopComparing();
        public abstract List<GearItem> GetAvailableGear();
        public abstract void Equip(int selectedGear);
        public abstract Button GetGearButton();
    }
}