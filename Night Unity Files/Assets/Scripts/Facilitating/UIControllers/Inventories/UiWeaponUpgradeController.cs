using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using DefaultNamespace;
using Facilitating.Persistence;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Combat.Player;
using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;
using Game.Global.Tutorial;
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
        private EnhancedButton _infuseButton, _channelButton, _swapButton;
        private ListController _weaponList;
        private ListController _inscriptionList;
        private bool _upgradingAllowed;
        private WeaponDetailController _weaponDetail;
        private GameObject _infoGameObject;
        private Weapon _equippedWeapon;
        private List<TutorialOverlay> _startingOverlays, _channelOverlays, _infuseOverlays;

        public override bool Unlocked() => true;

        protected override void CacheElements()
        {
            _weaponList = gameObject.FindChildWithName<ListController>("Weapon List");
            _inscriptionList = gameObject.FindChildWithName<ListController>("Inscription List");
            _weaponDetail = gameObject.FindChildWithName<WeaponDetailController>("Stats");
            _infuseButton = gameObject.FindChildWithName<EnhancedButton>("Inscribe");
            _swapButton = gameObject.FindChildWithName<EnhancedButton>("Swap");
            _channelButton = gameObject.FindChildWithName<EnhancedButton>("Infuse");
            _infoGameObject = gameObject.FindChildWithName("Info");

            _startingOverlays = new List<TutorialOverlay>
            {
                new TutorialOverlay(GetComponent<RectTransform>()),
                new TutorialOverlay(_weaponDetail.DurabilityRect()),
                new TutorialOverlay(_weaponDetail.DurabilityRect())
            };
            _channelOverlays = new List<TutorialOverlay>
            {
                new TutorialOverlay(_channelButton.GetComponent<RectTransform>())
            };
            _infuseOverlays = new List<TutorialOverlay>
            {
                new TutorialOverlay(_infuseButton.GetComponent<RectTransform>())
            };

            List<ItemQuality> qualities = new List<ItemQuality>();
#if UNITY_EDITOR
            foreach (ItemQuality value in Enum.GetValues(typeof(ItemQuality))) qualities.Add(value);
            for (int i = 0; i < 0; ++i)
            {
                Weapon weapon = WeaponGenerator.GenerateWeapon();
                Inventory.Move(weapon);
                Inscription inscription = Inscription.Generate();
                Inventory.Move(inscription);
                Armour plate = Armour.Create(qualities.RandomElement(), Armour.ArmourType.Chest);
                Inventory.Move(plate);
                Armour head = Armour.Create(qualities.RandomElement(), Armour.ArmourType.Head);
                Inventory.Move(head);
                Accessory accessory = Accessory.Generate();
                Inventory.Move(accessory);
            }
#endif

            _swapButton.AddOnClick(() =>
            {
                if (!WeaponsAreAvailable()) return;
                UiGearMenuController.SetCloseButtonAction(Show);
                _weaponList.Show();
                InputHandler.UnregisterInputListener(this);
                _infoGameObject.SetActive(false);
            });
            _infuseButton.AddOnClick(() =>
            {
                if (!InscriptionsAreAvailable()) return;
                UiGearMenuController.SetCloseButtonAction(Show);
                _inscriptionList.Show();
                InputHandler.UnregisterInputListener(this);
                _infoGameObject.SetActive(false);
            });
            _channelButton.AddOnClick(Infuse);
            _channelButton.AddOnHold(Infuse, 0.5f);
        }

        protected override void Initialise()
        {
            List<ListElement> weaponListElements = new List<ListElement>();
            weaponListElements.Add(new WeaponElement());
            weaponListElements.Add(new WeaponElement());
            weaponListElements.Add(new WeaponElement());
            weaponListElements.Add(new DetailedWeaponElement());
            weaponListElements.Add(new WeaponElement());
            weaponListElements.Add(new WeaponElement());
            weaponListElements.Add(new WeaponElement());
            _weaponList.Initialise(weaponListElements, Equip, BackToWeaponInfo, GetAvailableWeapons);
            _weaponList.Hide();
            _inscriptionList.Initialise(typeof(InscriptionElement), Inscribe, BackToWeaponInfo, GetAvailableInscriptions);
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
            SelectButton(_channelButton);
        }

        protected override void OnShow()
        {
            UiGearMenuController.SetCloseButtonAction(UiGearMenuController.Close);
            _infoGameObject.SetActive(true);
            _weaponList.Hide();
            _inscriptionList.Hide();
            _swapButton.gameObject.SetActive(WeaponsAreAvailable());
            SelectButton(_swapButton);

            InputHandler.RegisterInputListener(this);
            SetWeapon();
            StartCoroutine(TryShowWeaponTutorial());
        }

        private IEnumerator TryShowWeaponTutorial()
        {
            if (TutorialManager.TryOpenTutorial(11, _startingOverlays))
            {
                while (TutorialManager.IsTutorialVisible()) yield return null;
            }

            if (_channelButton.gameObject.activeInHierarchy)
            {
                if (TutorialManager.TryOpenTutorial(16, _channelOverlays))
                {
                    while (TutorialManager.IsTutorialVisible()) yield return null;
                }
            }

            if (_infuseButton.gameObject.activeInHierarchy)
            {
                if (TutorialManager.TryOpenTutorial(17, _infuseOverlays))
                {
                    while (TutorialManager.IsTutorialVisible()) yield return null;
                }
            }
        }

        protected override void OnHide()
        {
            InputHandler.UnregisterInputListener(this);
        }

        private void Equip(object weaponObject)
        {
            Weapon weapon = (Weapon) weaponObject;
            CharacterManager.SelectedCharacter.EquipWeapon(weapon);
            UiGearMenuController.PlayAudio(AudioClips.EquipWeapon);
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
            SelectButton(_infuseButton);
        }

        private void SetWeapon()
        {
            _equippedWeapon = CharacterManager.SelectedCharacter.EquippedWeapon;
            _weaponDetail.SetWeapon(_equippedWeapon);
            if (_equippedWeapon == null)
            {
                _infuseButton.gameObject.SetActive(false);
                _channelButton.gameObject.SetActive(false);
            }
            else UpdateWeaponActions();
        }

        private void SelectButton(EnhancedButton from)
        {
            if (from.gameObject.activeInHierarchy)
            {
                from.Select();
                return;
            }

            if (_swapButton.gameObject.activeInHierarchy) _swapButton.Select();
            else if (_channelButton.gameObject.activeInHierarchy) _channelButton.Select();
            else if (_infuseButton.gameObject.activeInHierarchy) _infuseButton.Select();
        }

        private void UpdateWeaponActions()
        {
            WeaponAttributes attr = _equippedWeapon.WeaponAttributes;
            bool reachedMaxDurability = attr.GetDurability().ReachedMax() || Inventory.GetResourceQuantity("Essence") == 0;
            bool inscriptionsAvailable = InscriptionsAreAvailable();
            _channelButton.gameObject.SetActive(!reachedMaxDurability);
            _infuseButton.gameObject.SetActive(inscriptionsAvailable);
        }

        private static bool WeaponsAreAvailable() => GetAvailableWeapons().Count != 0;
        private static bool InscriptionsAreAvailable() => GetAvailableInscriptions().Count != 0;

        private class WeaponElement : BasicListElement
        {
            protected override void UpdateCentreItemEmpty()
            {
            }

            protected override void Update(object o, bool isCentreItem)
            {
                Weapon weapon = (Weapon) o;
                CentreText.SetText(weapon.GetDisplayName());
                LeftText.SetText(weapon.WeaponType().ToString());
                RightText.SetText(weapon.WeaponAttributes.DPS().Round(1) + " DPS");
            }
        }

        private class InscriptionElement : WeaponElement
        {
            protected override void Update(object o, bool isCentreItem)
            {
                Inscription inscription = (Inscription) o;
                string inscriptionString = inscription.Name;
                CentreText.SetText(inscriptionString);
                string costText = inscription.InscriptionCost() + " Essence";
                if (!inscription.CanAfford()) costText = "Requires " + costText;
                LeftText.SetText(costText);
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

            protected override void Update(object o, bool isCentreItem)
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
            if (isHeld || axis != InputAxis.Menu) return;
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