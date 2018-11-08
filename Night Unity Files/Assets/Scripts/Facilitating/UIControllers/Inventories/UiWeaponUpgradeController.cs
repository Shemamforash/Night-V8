using System;
using System.Collections.Generic;
using DefaultNamespace;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Combat.Player;
using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UiWeaponUpgradeController : UiInventoryMenuController, IInputListener
    {
        private EnhancedButton _inscribeButton, _infuseButton, _swapButton;
        private TextMeshProUGUI _infuseText, _inscribeText, _swapText;
        private ListController _weaponList;
        private ListController _inscriptionList;
        private bool _upgradingAllowed;
        private WeaponDetailController _weaponDetail;
        private GameObject _infoGameObject;
        private Weapon _equippedWeapon;

        protected override void CacheElements()
        {
            _weaponList = gameObject.FindChildWithName<ListController>("Weapon List");
            _inscriptionList = gameObject.FindChildWithName<ListController>("Inscription List");
            _weaponDetail = gameObject.FindChildWithName<WeaponDetailController>("Stats");
            _inscribeButton = gameObject.FindChildWithName<EnhancedButton>("Inscribe");
            _inscribeText = _inscribeButton.gameObject.FindChildWithName<TextMeshProUGUI>("Text");
            _swapButton = gameObject.FindChildWithName<EnhancedButton>("Swap");
            _swapText = _swapButton.gameObject.FindChildWithName<TextMeshProUGUI>("Text");
            _infuseButton = gameObject.FindChildWithName<EnhancedButton>("Infuse");
            _infuseText = _infuseButton.gameObject.FindChildWithName<TextMeshProUGUI>("Text");
            _infoGameObject = gameObject.FindChildWithName("Info");

            List<ItemQuality> qualities = new List<ItemQuality>();
//#if UNITY_EDITOR
            foreach (ItemQuality value in Enum.GetValues(typeof(ItemQuality))) qualities.Add(value);
            for (int i = 0; i < 50; ++i)
            {
                Weapon weapon = WeaponGenerator.GenerateWeapon();
                Inventory.Move(weapon);
                Inscription inscription = Inscription.Generate();
                Inventory.Move(inscription);
                Armour plate = Armour.Create(qualities.RandomElement());
                Inventory.Move(plate);
                Accessory accessory = Accessory.Generate();
                Inventory.Move(accessory);
            }
//#endif

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
            _infuseButton.AddOnHold(Infuse, 0.5f);
        }

        protected override void Initialise()
        {
            List<ListElement> weaponListElements = new List<ListElement>();
            weaponListElements.Add(new WeaponElement());
            weaponListElements.Add(new WeaponElement());
            weaponListElements.Add(new DetailedWeaponElement());
            weaponListElements.Add(new WeaponElement());
            weaponListElements.Add(new WeaponElement());
            _weaponList.Initialise(weaponListElements, Equip, BackToWeaponInfo);
            _weaponList.Hide();
            _inscriptionList.Initialise(typeof(InscriptionElement), Inscribe, BackToWeaponInfo);
            _inscriptionList.Hide();
        }

        private void BackToWeaponInfo()
        {
            Show();
            UiGearMenuController.FlashCloseButton();
        }

        private static List<object> GetAvailableWeapons()
        {
            return Inventory.GetAvailableWeapons().ToObjectList();
        }

        private static List<object> GetAvailableInscriptions()
        {
            return Inventory.Inscriptions.ToObjectList();
        }

        private void Infuse()
        {
            if (CharacterManager.SelectedCharacter.EquippedWeapon == null) return;
            if (CharacterManager.SelectedCharacter.EquippedWeapon.WeaponAttributes.GetDurability().ReachedMax()) return;
            if (Inventory.GetResourceQuantity("Essence") == 0) return;
            Inventory.DecrementResource("Essence", 1);
            CharacterManager.SelectedCharacter.BrandManager.IncreaseEssenceInfused();
            int durabilityGain = 1 + (int) CharacterManager.SelectedCharacter.Attributes.EssenceRecoveryModifier;
            CharacterManager.SelectedCharacter.EquippedWeapon.WeaponAttributes.IncreaseDurability(durabilityGain);
            _weaponDetail.UpdateWeaponInfo();
            UpdateWeaponActions();
            SelectButton(_infuseButton);
        }

        protected override void OnShow()
        {
            _infoGameObject.SetActive(true);
            _weaponList.Hide();
            _inscriptionList.Hide();
            _swapButton.SetEnabled(WeaponsAreAvailable());
            _swapText.color = _swapButton.enabled ? Color.white : UiAppearanceController.FadedColour;
            SelectButton(_swapButton);

            InputHandler.RegisterInputListener(this);
            SetWeapon();
            TutorialManager.TryOpenTutorial(6);
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
            if (PlayerCombat.Instance == null) return;
            PlayerCombat.Instance.EquipInscription();
            UpdateWeaponActions();
            SelectButton(_inscribeButton);
        }

        private void SetWeapon()
        {
            _equippedWeapon = CharacterManager.SelectedCharacter.EquippedWeapon;
            _weaponDetail.SetWeapon(_equippedWeapon);
            if (_equippedWeapon == null)
            {
                _inscribeButton.SetEnabled(false);
                _inscribeText.color = UiAppearanceController.FadedColour;
                _infuseButton.SetEnabled(false);
                _infuseText.color = UiAppearanceController.FadedColour;
            }
            else UpdateWeaponActions();
        }

        private void SelectButton(EnhancedButton from)
        {
            if (from.IsEnabled())
            {
                from.Select();
                return;
            }

            if (_swapButton.IsEnabled()) _swapButton.Select();
            else if (_infuseButton.IsEnabled()) _infuseButton.Select();
            else if (_inscribeButton.isActiveAndEnabled) _inscribeButton.Select();
        }

        private void UpdateWeaponActions()
        {
            WeaponAttributes attr = _equippedWeapon.WeaponAttributes;
            bool reachedMaxDurability = attr.GetDurability().ReachedMax() || Inventory.GetResourceQuantity("Essence") == 0;
            bool inscriptionsAvailable = InscriptionsAreAvailable();
            _infuseButton.SetEnabled(!reachedMaxDurability);
            _infuseText.color = !reachedMaxDurability ? Color.white : UiAppearanceController.FadedColour;
            _inscribeButton.SetEnabled(inscriptionsAvailable);
            _inscribeText.color = inscriptionsAvailable ? Color.white : UiAppearanceController.FadedColour;
        }

        private static bool WeaponsAreAvailable() => GetAvailableWeapons().Count != 0;
        private static bool InscriptionsAreAvailable() => GetAvailableInscriptions().Count != 0;

        private class WeaponElement : BasicListElement
        {
            protected override void UpdateCentreItemEmpty()
            {
            }

            protected override void Update(object o)
            {
                Weapon weapon = (Weapon) o;
                CentreText.SetText(weapon.GetDisplayName());
                LeftText.SetText(weapon.WeaponType().ToString());
                RightText.SetText(weapon.WeaponAttributes.DPS().Round(1) + " DPS");
            }
        }

        private class InscriptionElement : WeaponElement
        {
            protected override void Update(object o)
            {
                Inscription inscription = (Inscription) o;
                bool canAfford = inscription.CanAfford();
                string inscriptionString = inscription.Name + (canAfford ? "" : " - Cannot Afford");
                CentreText.SetText(inscriptionString);
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