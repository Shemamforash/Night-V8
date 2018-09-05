using System;
using System.Collections.Generic;
using DefaultNamespace;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UiWeaponUpgradeController : UiInventoryMenuController, IInputListener
    {
        private EnhancedButton _inscribeButton, _infuseButton, _swapButton;
        private ListController _weaponList;
        private ListController _inscriptionList;
        private bool _upgradingAllowed;
        private WeaponDetailController _weaponDetail;
        private GameObject _infoGameObject;

        protected override void CacheElements()
        {
            _weaponList = gameObject.FindChildWithName<ListController>("Weapon List");
            _inscriptionList = gameObject.FindChildWithName<ListController>("Inscription List");
            _weaponDetail = gameObject.FindChildWithName<WeaponDetailController>("Stats");
            _inscribeButton = gameObject.FindChildWithName<EnhancedButton>("Inscribe");
            _swapButton = gameObject.FindChildWithName<EnhancedButton>("Swap");
            _infuseButton = gameObject.FindChildWithName<EnhancedButton>("Infuse");
            _infoGameObject = gameObject.FindChildWithName("Info");

            List<ItemQuality> qualities = new List<ItemQuality>();
            foreach (ItemQuality value in Enum.GetValues(typeof(ItemQuality))) qualities.Add(value);
            for (int i = 0; i < 5; ++i)
            {
                Weapon weapon = WeaponGenerator.GenerateWeapon(ItemQuality.Glowing);
                WorldState.HomeInventory().Move(weapon, 1);
                Inscription inscription = Inscription.Generate();
                WorldState.HomeInventory().Move(inscription, 1);
                ArmourPlate plate = ArmourPlate.Create(qualities.RandomElement());
                WorldState.HomeInventory().Move(plate, 1);
            }

            _swapButton.AddOnClick(() =>
            {
                if (!WeaponsAreAvailable()) return;
                _weaponList.Show(GetAvailableWeapons);
                InputHandler.UnregisterInputListener(this);
                _infoGameObject.SetActive(false);
            });
            _inscribeButton.AddOnClick(() =>
            {
                if (!InscriptionsAreAvailable()) return;
                _inscriptionList.Show(GetAvailableInscriptions);
                InputHandler.UnregisterInputListener(this);
                _infoGameObject.SetActive(false);
            });
            _infuseButton.AddOnClick(Infuse);
        }

        protected override void Initialise()
        {
            List<ListElement> weaponListElements = new List<ListElement>();
            weaponListElements.Add(new WeaponElement());
            weaponListElements.Add(new WeaponElement());
            weaponListElements.Add(new DetailedWeaponElement());
            weaponListElements.Add(new WeaponElement());
            weaponListElements.Add(new WeaponElement());
            _weaponList.Initialise(weaponListElements, Equip, Show);
            _weaponList.Hide();
            _inscriptionList.Initialise(typeof(InscriptionElement), Inscribe, Show);
            _inscriptionList.Hide();
        }

        private static List<object> GetAvailableWeapons()
        {
            Player player = CharacterManager.SelectedCharacter;
            Inventory inventory = player.TravelAction.AtHome() ? WorldState.HomeInventory() : player.Inventory();
            return inventory.Weapons.ToObjectList();
        }

        private static List<object> GetAvailableInscriptions()
        {
            Player player = CharacterManager.SelectedCharacter;
            Inventory inventory = player.TravelAction.AtHome() ? WorldState.HomeInventory() : player.Inventory();
            return inventory.Inscriptions.ToObjectList();
        }

        private void Infuse()
        {
            if (CharacterManager.SelectedCharacter.EquippedWeapon == null) return;
            if (CharacterManager.SelectedCharacter.EquippedWeapon.WeaponAttributes.GetDurability().ReachedMax()) return;
            if (WorldState.HomeInventory().GetResourceQuantity("Essence") == 0) return;
            WorldState.HomeInventory().DecrementResource("Essence", 1);
            CharacterManager.SelectedCharacter.BrandManager.IncreaseEssenceInfused();
            int durabilityGain = 1 + (int) CharacterManager.SelectedCharacter.Attributes.Val(AttributeType.EssenceRecoveryBonus);
            CharacterManager.SelectedCharacter.EquippedWeapon.WeaponAttributes.IncreaseDurability(durabilityGain);
            _weaponDetail.UpdateWeaponInfo();
        }

        protected override void OnShow()
        {
            _infoGameObject.SetActive(true);
            _weaponList.Hide();
            _inscriptionList.Hide();
            _swapButton.Select();
            InputHandler.RegisterInputListener(this);
            SetWeapon();
        }

        protected override void OnHide()
        {
            InputHandler.UnregisterInputListener(this);
        }
        
        private void Equip(object weaponObject)
        {
            Weapon weapon = (Weapon) weaponObject;
            CharacterManager.SelectedCharacter.EquipWeapon(weapon);
            Show();
        }

        private void Inscribe(object inscriptionObject)
        {
            Inscription inscription = (Inscription) inscriptionObject;
            if (!inscription.CanAfford()) return;
            Weapon weapon = CharacterManager.SelectedCharacter.EquippedWeapon;
            weapon.SetInscription(inscription);
            Show();
        }

        private void SetWeapon()
        {
            Weapon weapon = CharacterManager.SelectedCharacter.EquippedWeapon;
            _weaponDetail.SetWeapon(weapon);
            if (weapon == null)
            {
                _inscribeButton.Button().interactable = false;
                _infuseButton.Button().interactable = false;
                _inscribeButton.gameObject.GetComponentInChildren<EnhancedText>().SetStrikeThroughActive(true);
                _infuseButton.gameObject.GetComponentInChildren<EnhancedText>().SetStrikeThroughActive(true);
            }
            else
            {
                WeaponAttributes attr = weapon.WeaponAttributes;
                bool reachedMaxDurability = attr.GetDurability().ReachedMax();
                bool inscriptionsAvailable = InscriptionsAreAvailable();
                _infuseButton.Button().enabled = !reachedMaxDurability;
                _infuseButton.gameObject.GetComponentInChildren<EnhancedText>().SetStrikeThroughActive(reachedMaxDurability);
                _inscribeButton.Button().enabled = inscriptionsAvailable;
                _inscribeButton.gameObject.GetComponentInChildren<EnhancedText>().SetStrikeThroughActive(!inscriptionsAvailable);
            }
        }

        private static bool WeaponsAreAvailable() => UiGearMenuController.Inventory().Weapons.Count != 0;
        private static bool InscriptionsAreAvailable() => UiGearMenuController.Inventory().Inscriptions.Count != 0;

        private class WeaponElement : BasicListElement
        {
            protected override void UpdateCentreItemEmpty()
            {
            }

            protected override void Update(object o)
            {
                Weapon weapon = (Weapon) o;
                CentreText.SetText(weapon.Name);
                Inscription inscription = weapon.GetInscription();
                string inscriptionText = inscription == null ? "No Inscription" : inscription.Name;
                LeftText.SetText(inscriptionText);
                RightText.SetText(weapon.WeaponAttributes.DPS().Round(1) + " DPS");
            }
        }

        private class InscriptionElement : WeaponElement
        {
            protected override void Update(object o)
            {
                Inscription inscription = (Inscription) o;
                bool canAfford = inscription.CanAfford();
                CentreText.SetStrikeThroughActive(!canAfford);
                LeftText.SetStrikeThroughActive(!canAfford);
                RightText.SetStrikeThroughActive(!canAfford);

                CentreText.SetText(inscription.Name);
                LeftText.SetText(inscription.InscriptionCost() + " Essence");
                RightText.SetText(inscription.GetSummary());
            }
        }

        private class DetailedWeaponElement : ListElement
        {
            private WeaponDetailController _detailController;

            protected override void UpdateCentreItemEmpty()
            {
                _detailController.SetWeapon(null);
            }

            public override void SetColour(Color colour)
            {
            }

            protected override void SetVisible(bool visible)
            {
            }

            protected override void CacheUiElements(Transform transform)
            {
                _detailController = transform.GetComponent<WeaponDetailController>();
            }

            protected override void Update(object o)
            {
                Weapon weapon = (Weapon) o;
                _detailController.SetWeapon(weapon);
                Weapon equippedWeapon = CharacterManager.SelectedCharacter.EquippedWeapon;
                if (equippedWeapon == null) return;
                _detailController.CompareTo(equippedWeapon);
            }
        }

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (isHeld || axis != InputAxis.Cover) return;
            UiGearMenuController.Close();
        }

        public void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }
    }
}