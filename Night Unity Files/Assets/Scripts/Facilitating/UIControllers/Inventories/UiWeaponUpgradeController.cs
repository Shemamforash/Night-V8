using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Extensions;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Combat.Player;
using Game.Gear;
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
		private       EnhancedButton         _infuseButton, _channelButton;
		private       ListController         _inscriptionList;
		private       bool                   _seenAttributeTutorial, _seenChannelTutorial, _seenInfuseTutorial;
		private       bool                   _upgradingAllowed;
		private       WeaponDetailController _weaponDetail;

		public override bool Unlocked() => !Locked;

		protected override void CacheElements()
		{
			_inscriptionList = gameObject.FindChildWithName<ListController>("Inscription List");
			_weaponDetail    = gameObject.FindChildWithName<WeaponDetailController>("Stats");
			_infuseButton    = gameObject.FindChildWithName<EnhancedButton>("Inscribe");
			_channelButton   = gameObject.FindChildWithName<EnhancedButton>("Infuse");
			_infoGameObject  = gameObject.FindChildWithName("Info");

#if UNITY_EDITOR
			for (int i = 0; i < 10; ++i)
			{
//                Weapon weapon = WeaponGenerator.GenerateWeapon();
//                Inventory.Move(weapon);
//                Inscription inscription = Inscription.Generate();
//                Inventory.Move(inscription);
//                Accessory accessory = Accessory.Generate();
//                Inventory.Move(accessory);
			}
#endif

			_infuseButton.AddOnClick(() =>
			{
				if (!InscriptionsAreAvailable()) return;
				UiGearMenuController.SetCloseButtonAction(Show);
				_inscriptionList.Show();
				_infoGameObject.SetActive(false);
			});
			_channelButton.AddOnClick(Channel);
		}

		protected override void Initialise()
		{
			_inscriptionList.Initialise(typeof(InscriptionElement), Inscribe, BackToWeaponInfo, GetAvailableInscriptions);
			_inscriptionList.Hide();
		}

		private void BackToWeaponInfo()
		{
			Show();
			UiGearMenuController.FlashCloseButton();
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

		private void Channel()
		{
			WeaponAttributes attributes = CharacterManager.SelectedCharacter.Weapon.WeaponAttributes;
			if (!attributes.CanIncreaseUpgradeLevel()) return;
			if (Inventory.GetResourceQuantity("Essence") == 0) return;
			UiGearMenuController.PlayAudio(AudioClips.Channel);
			Inventory.DecrementResource("Essence", 10);
			attributes.IncreaseUpgradeLevel();
			_weaponDetail.UpdateWeaponInfo();
			UpdateWeaponActions();
			SelectButton(_channelButton);
		}

		protected override void OnShow()
		{
			UiGearMenuController.SetCloseButtonAction(UiGearMenuController.Close);
			_infoGameObject.SetActive(true);
			_inscriptionList.Hide();
			SelectButton(_infuseButton);
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
				new TutorialOverlay(_weaponDetail.DurabilityRect()),
				new TutorialOverlay(_weaponDetail.DurabilityRect())
			};
			if (TutorialManager.Instance.TryOpenTutorial(12, startingOverlays))
			{
				while (TutorialManager.Instance.IsTutorialVisible()) yield return null;
			}

			_seenAttributeTutorial = true;
		}

		private IEnumerator ShowChannelTutorial()
		{
			if (_seenChannelTutorial) yield break;
			if (!_channelButton.gameObject.activeInHierarchy) yield break;
			TutorialOverlay overlay = new TutorialOverlay(_channelButton.GetComponent<RectTransform>());
			if (TutorialManager.Instance.TryOpenTutorial(17, overlay))
			{
				while (TutorialManager.Instance.IsTutorialVisible()) yield return null;
			}

			_seenChannelTutorial = true;
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
			if (_seenAttributeTutorial && _seenInfuseTutorial && _seenChannelTutorial) yield break;
			yield return StartCoroutine(ShowWeaponAttributeTutorial());
			yield return StartCoroutine(ShowChannelTutorial());
			yield return StartCoroutine(ShowInfuseTutorial());
		}

		private void Inscribe(object inscriptionObject)
		{
			Inscription inscription = (Inscription) inscriptionObject;
			if (!inscription.CanAfford()) return;
			Weapon weapon = CharacterManager.SelectedCharacter.Weapon;
			weapon.ApplyInscription(inscription);
			if (!weapon.WeaponAttributes.CanIncreaseUpgradeLevel() && inscription.Quality() == ItemQuality.Radiant)
				AchievementManager.Instance().MaxOutWeapon();

			Show();
			UiGearMenuController.PlayAudio(AudioClips.Infuse);
			if (PlayerCombat.Instance == null) return;
			PlayerCombat.Instance.EquipInscription();
			UpdateWeaponActions();
			SelectButton(_infuseButton);
		}

		private void SetWeapon()
		{
			_equippedWeapon = CharacterManager.SelectedCharacter.Weapon;
			_weaponDetail.SetWeapon(_equippedWeapon);
			UpdateWeaponActions();
		}

		private void SelectButton(EnhancedButton from)
		{
			if (from.gameObject.activeInHierarchy)
			{
				from.Select();
				return;
			}

			if (_channelButton.gameObject.activeInHierarchy)
				_channelButton.Select();
			else if (_infuseButton.gameObject.activeInHierarchy)
				_infuseButton.Select();
		}

		private void UpdateWeaponActions()
		{
			WeaponAttributes attributes            = _equippedWeapon.WeaponAttributes;
			bool             reachedMaxDurability  = !attributes.CanIncreaseUpgradeLevel() || Inventory.GetResourceQuantity("Essence") == 0;
			bool             inscriptionsAvailable = InscriptionsAreAvailable();
			_channelButton.gameObject.SetActive(!reachedMaxDurability);
			_infuseButton.gameObject.SetActive(inscriptionsAvailable);
		}

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