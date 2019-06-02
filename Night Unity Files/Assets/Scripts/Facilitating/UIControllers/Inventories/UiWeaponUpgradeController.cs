using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Extensions;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Combat.Player;
using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;
using Game.Global.Tutorial;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Facilitating.UIControllers
{
	public class UiWeaponUpgradeController : UiInventoryMenuController
	{
		public static bool                   Locked;
		private       Weapon                 _equippedWeapon;
		private       GameObject             _infoGameObject;
		private       EnhancedButton         _infuseButton, _swapButton;
		private       ListController         _inscriptionList;
		private       ListController         _weaponList;
		private       bool                   _seenAttributeTutorial, _seenInfuseTutorial;
		private       bool                   _upgradingAllowed;
		private       WeaponDetailController _weaponDetail;

		public override bool Unlocked() => !Locked;

		protected override void CacheElements()
		{
			_inscriptionList = gameObject.FindChildWithName<ListController>("Inscription List");
			_weaponDetail    = gameObject.FindChildWithName<WeaponDetailController>("Stats");
			_infuseButton    = gameObject.FindChildWithName<EnhancedButton>("Inscribe");
			_infoGameObject  = gameObject.FindChildWithName("Info");
			_swapButton      = gameObject.FindChildWithName<EnhancedButton>("Swap");
			_weaponList      = gameObject.FindChildWithName<ListController>("Weapon List");


#if UNITY_EDITOR
			for (int i = 0; i < 10; ++i)
			{
				Weapon weapon = WeaponGenerator.GenerateWeapon();
				Inventory.Move(weapon);
				Inscription inscription = Inscription.Generate(true);
				Inventory.Move(inscription);
				Accessory accessory = Accessory.Generate();
				Inventory.Move(accessory);
			}
#endif

			_swapButton.AddOnClick(() =>
			{
				if (!WeaponsAreAvailable()) return;
				UiGearMenuController.SetCloseButtonAction(Show);
				_weaponList.Show();
				_infoGameObject.SetActive(false);
			});

			_infuseButton.AddOnClick(TryShowInscriptions);
		}

		private void TryShowInscriptions()
		{
			if (!InscriptionsAreAvailable())
			{
				BackToWeaponInfo();
				return;
			}

			UiGearMenuController.SetCloseButtonAction(Show);
			_inscriptionList.Show();
			_infoGameObject.SetActive(false);
		}

		private void Inscribe(object inscriptionObject)
		{
			Inscription inscription = (Inscription) inscriptionObject;
			if (!inscription.CanAfford()) return;
			Weapon weapon = CharacterManager.SelectedCharacter.Weapon;
			weapon.AddInscription(inscription);
			if (weapon.Quality() == ItemQuality.Radiant && inscription.Quality() == ItemQuality.Radiant)
				AchievementManager.Instance().MaxOutWeapon();

			if (!InscriptionsAreAvailable()) Show();

			UiGearMenuController.PlayAudio(AudioClips.Infuse);
			if (PlayerCombat.Instance == null) return;
			PlayerCombat.Instance.EquipInscription();
			UpdateWeaponActions();
			SelectButton(_infuseButton);
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
			List<Weapon> weapons = Inventory.GetAvailableWeapons();
			weapons.Sort((a, b) =>
			{
				if ((int) a.Quality()    > (int) b.Quality()) return -1;
				if ((int) a.Quality()    < (int) b.Quality()) return 1;
				if ((int) a.WeaponType() < (int) b.WeaponType()) return -1;
				if ((int) a.WeaponType() > (int) b.WeaponType()) return 1;
				return string.Compare(a.Name, b.Name, StringComparison.InvariantCulture);
			});
			return weapons.ToObjectList();
		}

		private static List<object> GetAvailableInscriptions()
		{
			List<Inscription> inscriptions = Inventory.Inscriptions;
			inscriptions.Sort((a, b) =>
			{
				if ((int) a.Quality() > (int) b.Quality()) return -1;
				if ((int) a.Quality() < (int) b.Quality()) return 1;
				return string.Compare(a.Name, b.Name, StringComparison.InvariantCulture);
			});
			return inscriptions.ToObjectList();
		}

		protected override void OnShow()
		{
			UiGearMenuController.SetCloseButtonAction(UiGearMenuController.Close);
			_infoGameObject.SetActive(true);
			_weaponList.Hide();
			_inscriptionList.Hide();
			_swapButton.gameObject.SetActive(WeaponsAreAvailable());
			SelectButton(_swapButton);

			SetWeapon();
			StartCoroutine(TryShowWeaponTutorial());
		}

		private IEnumerator ShowWeaponAttributeTutorial()
		{
			if (_seenAttributeTutorial) yield break;
			if (!TutorialManager.FinishedIntroTutorial()) yield break;
			List<TutorialOverlay> startingOverlays = new List<TutorialOverlay>
			{
				new TutorialOverlay(GetComponent<RectTransform>()),
			};
			if (TutorialManager.Instance.TryOpenTutorial(12, startingOverlays))
			{
				while (TutorialManager.Instance.IsTutorialVisible()) yield return null;
			}

			_seenAttributeTutorial = true;
		}

		private IEnumerator ShowInfuseTutorial()
		{
			if (_seenInfuseTutorial) yield break;
			if (!_infuseButton.gameObject.activeInHierarchy) yield break;
			TutorialOverlay overlay = new TutorialOverlay(_infuseButton.GetComponent<RectTransform>());
			if (TutorialManager.Instance.TryOpenTutorial(18, overlay))
			{
				while (TutorialManager.Instance.IsTutorialVisible()) yield return null;
			}

			_seenInfuseTutorial = true;
		}

		private IEnumerator TryShowWeaponTutorial()
		{
			if (!TutorialManager.Active()) yield break;
			if (_seenAttributeTutorial && _seenInfuseTutorial) yield break;
			yield return StartCoroutine(ShowWeaponAttributeTutorial());
			yield return StartCoroutine(ShowInfuseTutorial());
		}

		private void Equip(object weaponObject)
		{
			Weapon weapon = (Weapon) weaponObject;
			CharacterManager.SelectedCharacter.EquipWeapon(weapon);
			UiGearMenuController.PlayAudio(AudioClips.EquipWeapon);
			Show();
		}

		private void SetWeapon()
		{
			_equippedWeapon = CharacterManager.SelectedCharacter.Weapon;
			_weaponDetail.SetWeapon(_equippedWeapon);
			if (_equippedWeapon == null)
				_infuseButton.gameObject.SetActive(false);
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
			else if (_infuseButton.gameObject.activeInHierarchy) _infuseButton.Select();
		}

		private void UpdateWeaponActions()
		{
			bool inscriptionsAvailable = InscriptionsAreAvailable();
			_infuseButton.gameObject.SetActive(inscriptionsAvailable);
		}

		private static bool WeaponsAreAvailable() => GetAvailableWeapons().Count != 0;

		private static bool InscriptionsAreAvailable() => GetAvailableInscriptions().Count != 0;

		private class InscriptionElement : BasicListElement
		{
			protected override void UpdateCentreItemEmpty()
			{
			}

			protected override void Update(object o, bool isCentreItem)
			{
				Inscription inscription       = (Inscription) o;
				string      inscriptionString = inscription.Name;
				CentreText.SetText(inscriptionString);
				string costText                        = inscription.InscriptionCost() + " Essence";
				if (!inscription.CanAfford()) costText = "Requires " + costText;
				LeftText.SetText(costText);
				RightText.SetText(inscription.GetSummary());
			}
		}

		private class WeaponElement : BasicListElement
		{
			protected override void UpdateCentreItemEmpty()
			{
			}

			protected override void Update(object o, bool isCentreItem)
			{
				Weapon weapon = (Weapon) o;
				CentreText.SetText(weapon.Name);
				LeftText.SetText(weapon.WeaponType().ToString());
				RightText.SetText(weapon.WeaponAttributes.DPS().Round(1) + " DPS");
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
				Weapon equippedWeapon = CharacterManager.SelectedCharacter.Weapon;
				if (equippedWeapon == null) return;
//                _detailController.CompareTo(equippedWeapon);
			}
		}
	}
}