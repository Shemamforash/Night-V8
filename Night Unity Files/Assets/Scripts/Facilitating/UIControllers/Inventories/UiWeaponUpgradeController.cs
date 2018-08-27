using System.Collections.Generic;
using DefaultNamespace;
using Game.Characters;
using Game.Gear;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UiWeaponUpgradeController : MonoBehaviour
    {
        private EnhancedButton _inscribeButton, _infuseButton, _swapButton;
        private ListController _weaponList;
        private ListController _inscriptionList;
        private bool _upgradingAllowed;
        private WeaponDetailController _weaponDetail;
        private GameObject _infoGameObject;

        private class WeaponElement : ListElement
        {
            protected EnhancedText _nameText;
            protected EnhancedText _durabilityText;
            protected EnhancedText _dpsText;

            protected override void SetVisible(bool visible)
            {
                _nameText.gameObject.SetActive(visible);
                _durabilityText.gameObject.SetActive(visible);
                _dpsText.gameObject.SetActive(visible);
            }

            protected override void CacheUiElements(Transform transform)
            {
                _nameText = transform.gameObject.FindChildWithName<EnhancedText>("Name");
                _durabilityText = transform.gameObject.FindChildWithName<EnhancedText>("Type");
                _dpsText = transform.gameObject.FindChildWithName<EnhancedText>("Dps");
            }

            protected override void Update(object o)
            {
                Weapon weapon = (Weapon) o;
                _nameText.SetText(weapon.Name);
                Inscription inscription = weapon.GetInscription();
                string inscriptionText = inscription == null ? "No Inscription" : inscription.Name;
                _durabilityText.SetText(inscriptionText);
                _dpsText.SetText(weapon.WeaponAttributes.DPS().Round(1) + " DPS");
            }

            public override void SetColour(Color c)
            {
                _nameText.SetColor(c);
                _durabilityText.SetColor(c);
                _dpsText.SetColor(c);
            }
        }

        private class InscriptionElement : WeaponElement
        {
            protected override void Update(object o)
            {
                Inscription inscription = (Inscription) o;
                bool canAfford = inscription.CanAfford();
                _nameText.SetStrikeThroughActive(!canAfford);
                _durabilityText.SetStrikeThroughActive(!canAfford);
                _dpsText.SetStrikeThroughActive(!canAfford);

                _nameText.SetText(inscription.Name);
                _durabilityText.SetText(inscription.InscriptionCost() + " Essence");
                _dpsText.SetText(inscription.GetSummary());
            }
        }

        private class DetailedWeaponElement : ListElement
        {
            private WeaponDetailController _detailController;

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

        public void Awake()
        {
            _weaponList = gameObject.FindChildWithName<ListController>("Weapon List");
            _inscriptionList = gameObject.FindChildWithName<ListController>("Inscription List");
            _weaponDetail = gameObject.FindChildWithName<WeaponDetailController>("Stats");
            _inscribeButton = gameObject.FindChildWithName<EnhancedButton>("Inscribe");
            _swapButton = gameObject.FindChildWithName<EnhancedButton>("Swap");
            _infuseButton = gameObject.FindChildWithName<EnhancedButton>("Infuse");
            _infoGameObject = gameObject.FindChildWithName("Info");

            for (int i = 0; i < 5; ++i)
            {
                Weapon weapon = WeaponGenerator.GenerateWeapon(ItemQuality.Shining);
                WorldState.HomeInventory().Move(weapon, 1);
                Inscription inscription = Inscription.Generate();
                WorldState.HomeInventory().Move(inscription, 1);
            }

            _swapButton.AddOnClick(() =>
            {
                if (!GearIsAvailable()) return;
                _weaponList.Show(GetAvailableWeapons);
                _infoGameObject.SetActive(false);
            });
            _inscribeButton.AddOnClick(() =>
            {
                if (!InscriptionsAreAvailable()) return;
                _inscriptionList.Show(GetAvailableInscriptions);
                _infoGameObject.SetActive(false);
            });
            _infuseButton.AddOnClick(Infuse);
        }

        public void Start()
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

        public void Show()
        {
            _infoGameObject.SetActive(true);
            _weaponList.Hide();
            _inscriptionList.Hide();
            _swapButton.Select();
            _swapButton.SetDownNavigation(UiGearMenuController.GetCloseButton());
            SetWeapon();
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
            CharacterManager.SelectedCharacter.EquippedWeapon.SetInscription(inscription);
            Show();
        }

        public void Hide()
        {
            _weaponDetail.Hide();
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
                Debug.Log(attr.GetDurability().ReachedMax() + " " + attr.GetDurability().CurrentValue() + " " + attr.GetDurability().Max);
                bool reachedMaxDurability = attr.GetDurability().ReachedMax();
                bool inscriptionsAvailable = InscriptionsAreAvailable();
                _infuseButton.Button().enabled = !reachedMaxDurability;
                _infuseButton.gameObject.GetComponentInChildren<EnhancedText>().SetStrikeThroughActive(reachedMaxDurability);
                _inscribeButton.Button().enabled = inscriptionsAvailable;
                _inscribeButton.gameObject.GetComponentInChildren<EnhancedText>().SetStrikeThroughActive(!inscriptionsAvailable);
            }
        }

        private static bool GearIsAvailable() => UiGearMenuController.Inventory().Weapons.Count != 0;
        private static bool InscriptionsAreAvailable() => UiGearMenuController.Inventory().Inscriptions.Count != 0;
    }
}