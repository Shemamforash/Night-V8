using System.Collections.Generic;
using Game.Characters;
using Game.Characters.CharacterActions;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UiCreateAmmoController : Menu
    {
        private static Player _player;
        private static UiCreateAmmoController _instance;
        private List<EnhancedButton> _canAfford;
        private List<EnhancedButton> _cantAfford;
        private EnhancedButton _pistolButton, _rifleButton, _shotgunButton, _smgButton, _lmgButton, _closeButton;
        private EnhancedText _scrapLeft;

        public override void Awake()
        {
            base.Awake();
            _instance = this;
            _pistolButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Pistol Ammo");
            _rifleButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Rifle Ammo");
            _shotgunButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Shotgun Ammo");
            _smgButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "SMG Ammo");
            _lmgButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "LMG Ammo");
            _closeButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Close Menu");
            _scrapLeft = Helper.FindChildWithName<EnhancedText>(gameObject, "Scrap Left");
        }

        public void Start()
        {
            _pistolButton.AddOnClick(() => CraftAmmo(InventoryResourceType.PistolMag));
            _rifleButton.AddOnClick(() => CraftAmmo(InventoryResourceType.RifleMag));
            _shotgunButton.AddOnClick(() => CraftAmmo(InventoryResourceType.ShotgunMag));
            _smgButton.AddOnClick(() => CraftAmmo(InventoryResourceType.SmgMag));
            _lmgButton.AddOnClick(() => CraftAmmo(InventoryResourceType.LmgMag));
            _closeButton.AddOnClick(MenuStateMachine.GoToInitialMenu);
            SetAmmoCostText();
        }

        private void SetAmmoCostText()
        {
            Helper.FindChildWithName<EnhancedText>(_pistolButton.gameObject, "Centre Text").Text(WeaponGenerator.GetWeaponClassWithType(WeaponType.Pistol).AmmoCost + " Scrap");
            Helper.FindChildWithName<EnhancedText>(_rifleButton.gameObject, "Centre Text").Text(WeaponGenerator.GetWeaponClassWithType(WeaponType.Rifle).AmmoCost + " Scrap");
            Helper.FindChildWithName<EnhancedText>(_shotgunButton.gameObject, "Centre Text").Text(WeaponGenerator.GetWeaponClassWithType(WeaponType.Shotgun).AmmoCost + " Scrap");
            Helper.FindChildWithName<EnhancedText>(_smgButton.gameObject, "Centre Text").Text(WeaponGenerator.GetWeaponClassWithType(WeaponType.SMG).AmmoCost + " Scrap");
            Helper.FindChildWithName<EnhancedText>(_lmgButton.gameObject, "Centre Text").Text(WeaponGenerator.GetWeaponClassWithType(WeaponType.LMG).AmmoCost + " Scrap");
        }

        public void ShowMenu(Player player)
        {
            _player = player;
            _canAfford = new List<EnhancedButton>();
            _cantAfford = new List<EnhancedButton>();
            UpdateScrap();
            UpdateHaveAmounts();

            CheckCanAfford(WeaponType.Pistol, _pistolButton);
            CheckCanAfford(WeaponType.Rifle, _rifleButton);
            CheckCanAfford(WeaponType.Shotgun, _shotgunButton);
            CheckCanAfford(WeaponType.SMG, _smgButton);
            CheckCanAfford(WeaponType.LMG, _lmgButton);

            for (int i = 0; i < _canAfford.Count; ++i)
            {
                _canAfford[i].GetComponent<CanvasGroup>().alpha = 1;
                if (i != 0) _canAfford[i].SetUpNavigation(_canAfford[i - 1]);
            }

            _cantAfford.ForEach(t => t.GetComponent<CanvasGroup>().alpha = 0.4f);
            if (_canAfford.Count > 0)
            {
                DefaultSelectable = _canAfford[0].Button();
                _closeButton.SetUpNavigation(_canAfford[_canAfford.Count - 1]);
            }
            else
            {
                DefaultSelectable = _closeButton.Button();
                _closeButton.SetUpNavigation(null);
            }

            MenuStateMachine.ShowMenu("Create Ammo Menu");
        }

        private void CheckCanAfford(WeaponType type, EnhancedButton button)
        {
            if (WorldState.HomeInventory().GetResourceQuantity(InventoryResourceType.Scrap) >= WeaponGenerator.GetWeaponClassWithType(type).AmmoCost)
            {
                _canAfford.Add(button);
                return;
            }

            _cantAfford.Add(button);
        }

        private void UpdateHaveAmounts()
        {
            Helper.FindChildWithName<EnhancedText>(_pistolButton.gameObject, "Right Text").Text(NumberOfMagazinesOfType(WeaponType.Pistol));
            Helper.FindChildWithName<EnhancedText>(_rifleButton.gameObject, "Right Text").Text(NumberOfMagazinesOfType(WeaponType.Rifle));
            Helper.FindChildWithName<EnhancedText>(_shotgunButton.gameObject, "Right Text").Text(NumberOfMagazinesOfType(WeaponType.Shotgun));
            Helper.FindChildWithName<EnhancedText>(_smgButton.gameObject, "Right Text").Text(NumberOfMagazinesOfType(WeaponType.SMG));
            Helper.FindChildWithName<EnhancedText>(_lmgButton.gameObject, "Right Text").Text(NumberOfMagazinesOfType(WeaponType.LMG));
        }

        private string NumberOfMagazinesOfType(WeaponType t)
        {
            int quantity = 0;
            switch (t)
            {
                case WeaponType.Pistol:
                    quantity = (int) WorldState.HomeInventory().GetResourceQuantity(InventoryResourceType.PistolMag);
                    break;
                case WeaponType.Rifle:
                    quantity = (int) WorldState.HomeInventory().GetResourceQuantity(InventoryResourceType.RifleMag);
                    break;
                case WeaponType.Shotgun:
                    quantity = (int) WorldState.HomeInventory().GetResourceQuantity(InventoryResourceType.ShotgunMag);
                    break;
                case WeaponType.SMG:
                    quantity = (int) WorldState.HomeInventory().GetResourceQuantity(InventoryResourceType.SmgMag);
                    break;
                case WeaponType.LMG:
                    quantity = (int) WorldState.HomeInventory().GetResourceQuantity(InventoryResourceType.LmgMag);
                    break;
            }

            return "have " + quantity;
        }

        private void CraftAmmo(InventoryResourceType type)
        {
            CraftAmmo craftState = (CraftAmmo) _player.States.GetState("Craft Ammo");
            craftState.SetAmmoType(type);
        }

        private void UpdateScrap()
        {
            _scrapLeft.Text(WorldState.HomeInventory().GetResourceQuantity(InventoryResourceType.Scrap) + " scrap left");
        }

        public static UiCreateAmmoController Instance()
        {
            return _instance;
        }
    }
}