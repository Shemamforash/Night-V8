using System;
using System.Collections;
using System.Collections.Generic;
using Facilitating;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Generation.Shrines;
using Game.Combat.Player;
using Game.Combat.Ui;
using Game.Exploration.Environment;
using Game.Global.Tutorial;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Generation
{
	public class Tutorial : RegionGenerator
	{
		private PlayerCombat _player;
		private bool         _seenAccuracyTutorial;

		protected override void Generate()
		{
			_player = PlayerCombat.Instance;
			GameObject tutorialFloor = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Tutorial Floor");
			Instantiate(tutorialFloor, Vector3.zero, Quaternion.identity);
			CreateRocks();
			StartCoroutine(ShowTutorial());
		}

		private void CreateRocks()
		{
			float[] _radii        = {6, 9, 11};
			float   angleInterval = 360f / 25f;
			for (float angle = 0; angle < 360; angle += angleInterval)
			{
				Vector2 position = AdvancedMaths.CalculatePointOnCircle(angle, _radii[1], Vector2.zero);
				GenerateOneRock(SmallPolyWidth, MediumPolyWidth, 0.5f, 0.75f, position);

				position = AdvancedMaths.CalculatePointOnCircle(angle, _radii[0], Vector2.zero);
				GenerateOneRock(MinPolyWidth, SmallPolyWidth, 0.1f, 0.75f, position);

				position = AdvancedMaths.CalculatePointOnCircle(angle, _radii[2], Vector2.zero);
				GenerateOneRock(MediumPolyWidth, LargePolyWidth, 0.7f, 0.5f, position);
			}
		}

		private void CreateFoodAndWater()
		{
			ResourceItem item     = ResourceTemplate.Create("Cooked Meat");
			Vector2      position = AdvancedMaths.RandomDirection() * Random.Range(2f, 5f);
			Loot         loot     = new Loot(position);
			CharacterManager.CurrentRegion().Containers.Add(loot);
			loot.SetResource(item);
			loot.CreateObject();

			item     = ResourceTemplate.Create("Water");
			position = AdvancedMaths.RandomDirection() * Random.Range(2f, 5f);
			loot     = new Loot(position);
			CharacterManager.CurrentRegion().Containers.Add(loot);
			loot.SetResource(item);
			loot.CreateObject();
		}

		private void DisplayEventText()
		{
			EventTextController.SetOverrideText("");
		}

		private IEnumerator WaitForControl(Func<bool> condition, Func<string> text, bool wait = true)
		{
			DisplayEventText();
			float timer   = 5f;
			bool  pressed = false;
			while (!pressed || timer > 0f)
			{
				timer -= Time.unscaledDeltaTime;
				if (!pressed) pressed = condition();
				EventTextController.UpdateOverrideText(text());
				yield return null;
			}

			EventTextController.CloseOverrideText();
			if (wait) yield return new WaitForSecondsRealtime(1);
		}

		private IEnumerator ShowBasicControls()
		{
			yield return StartCoroutine(WaitForControl(() => InputHandler.InputAxisWasPressed(InputAxis.Horizontal) || InputHandler.InputAxisWasPressed(InputAxis.Vertical), () =>
			{
				string text = InputHandler.GetBindingForKey(InputAxis.Horizontal) + InputHandler.GetBindingForKey(InputAxis.Vertical);
				text = text == "A - DW - S" ? "WASD" : "Left Stick";
				return "Move using [" + text + "]";
			}));


			yield return StartCoroutine(WaitForControl(() => InputHandler.InputAxisWasPressed(InputAxis.SwitchTab), () =>
			{
				string text               = InputHandler.GetBindingForKey(InputAxis.SwitchTab);
				if (text != "J - L") text = "Right Stick";
				return "Rotate using [" + text + "]";
			}));

			yield return StartCoroutine(WaitForControl(() => true, () => "You can also aim with the mouse, and rotate the camera by holding [RMB]"));

			yield return StartCoroutine(WaitForControl(() => InputHandler.InputAxisWasPressed(InputAxis.Fire),
			                                           () => "Press [" + InputHandler.GetBindingForKey(InputAxis.Fire) + "] to Fire"));

			yield return StartCoroutine(WaitForControl(() => InputHandler.InputAxisWasPressed(InputAxis.Reload),
			                                           () => "Press [" + InputHandler.GetBindingForKey(InputAxis.Reload) + "] to Reload"));

			yield return StartCoroutine(WaitForControl(() => true, () => "You can view the control scheme at any time through the pause menu"));
		}

		private IEnumerator ShowDashTutorial()
		{
			_player.HealthController.TakeDamage(_player.HealthController.GetMaxHealth() * 0.6f);
			CombatManager.Instance().SetForceShowHud(true);

			yield return StartCoroutine(WaitForControl(() => InputHandler.InputAxisWasPressed(InputAxis.Sprint),
			                                           () => "Dash with [" + InputHandler.GetBindingForKey(InputAxis.Sprint) + "]"));

			EventTextController.CloseOverrideText();
			yield return new WaitForSecondsRealtime(1);
		}

		private IEnumerator ShowHealthTutorial()
		{
			CreateEnemies();

			EventTextController.SetOverrideText("Defeat all enemies");
			yield return new WaitForSecondsRealtime(5);
			EventTextController.CloseOverrideText();
			yield return new WaitForSecondsRealtime(1);
			while (!CombatManager.Instance().ClearOfEnemies())
			{
				if (!_seenAccuracyTutorial && CombatManager.Instance().InactiveEnemyCount() == 0 && PlayerCombat.Instance.GetTarget() != null)
				{
					TutorialOverlay overlay = new TutorialOverlay(EnemyUi.Instance.UiHitController.GetComponent<RectTransform>());
					TutorialManager.Instance.TryOpenTutorial(8, overlay);
					_seenAccuracyTutorial = true;
				}

				yield return null;
			}
		}

		private IEnumerator ShowAttributeTutorial()
		{
			EventTextController.SetOverrideText("Will can be used to restore attributes in and out of combat");
			yield return new WaitForSecondsRealtime(5);
			EventTextController.CloseOverrideText();
			yield return new WaitForSecondsRealtime(1);

			EventTextController.SetOverrideText("Restoring your Life attribute will recover health in combat");
			yield return new WaitForSecondsRealtime(5);
			EventTextController.CloseOverrideText();
			yield return new WaitForSecondsRealtime(1);

			EventTextController.SetOverrideText("Attributes slowly restore when Resting");
			yield return new WaitForSecondsRealtime(5);
			EventTextController.CloseOverrideText();
			yield return new WaitForSecondsRealtime(1);
		}

		private IEnumerator ShowCompassTutorial()
		{
			CreateFoodAndWater();

			yield return StartCoroutine(WaitForControl(() => InputHandler.InputAxisWasPressed(InputAxis.Compass),
			                                           () => "Use your compass with [" + InputHandler.GetBindingForKey(InputAxis.Compass) + "]"));

			TutorialManager.Instance.TryOpenTutorial(3, new List<TutorialOverlay> {new TutorialOverlay(UiCompassPulseController.CompassRect())});
			while (TutorialManager.Instance.IsTutorialVisible()) yield return null;

			yield return StartCoroutine(WaitForControl(() => true,
			                                           () => "Collect the revealed items with [" + InputHandler.GetBindingForKey(InputAxis.TakeItem) + "]"));

			while (CharacterManager.CurrentRegion().Containers.Count > 0) yield return null;

			EventTextController.SetOverrideText("Eat food and drink water to recover Life and Will respectively");
			yield return new WaitForSecondsRealtime(5);
			EventTextController.CloseOverrideText();
			yield return new WaitForSecondsRealtime(1);

			UiGearMenuController.SetOpenAllowed(true);
			UiGearMenuController.SetCloseAllowed(false);

			yield return StartCoroutine(WaitForControl(UiGearMenuController.IsOpen,
			                                           () => "Open your inventory with [" + InputHandler.GetBindingForKey(InputAxis.Inventory) + "] and restore your Life and Will", false));

			EventTextController.CloseOverrideText();
			while (Inventory.GetResourceQuantity("Water") > 0 && Inventory.GetResourceQuantity("Cooked Meat") > 0) yield return null;

			UiGearMenuController.SetCloseAllowed(true);
			while (UiGearMenuController.IsOpen()) yield return null;
			UiGearMenuController.SetOpenAllowed(false);
			yield return new WaitForSecondsRealtime(1);
		}

		private IEnumerator ShowLeaveTutorial()
		{
			CombatManager.Instance().SetForceShowHud(false);
			RiteStarter.GenerateTutorialStarter();
			EventTextController.SetOverrideText("Walk into the portal to begin your journey");
			CharacterManager.SelectedCharacter.Attributes.ResetValues();
			CharacterManager.SelectedCharacter.TravelAction.SetCurrentRegion(MapGenerator.GetInitialNode());
			UiGearMenuController.SetOpenAllowed(true);
			yield return new WaitForSecondsRealtime(2);
			EventTextController.CloseOverrideText();
		}

		private IEnumerator ShowTutorial()
		{
			UiWeaponUpgradeController.Locked = true;
			UiGearMenuController.SetOpenAllowed(false);
			yield return StartCoroutine(ShowBasicControls());
			yield return StartCoroutine(ShowDashTutorial());
			yield return StartCoroutine(ShowHealthTutorial());
			yield return StartCoroutine(ShowAttributeTutorial());
			yield return StartCoroutine(ShowCompassTutorial());
			yield return StartCoroutine(ShowLeaveTutorial());
		}

		private void OnDestroy()
		{
			UiWeaponUpgradeController.Locked = false;
			UiGearMenuController.SetCloseAllowed(true);
			UiGearMenuController.SetOpenAllowed(true);
		}

		private void Update()
		{
			if (_player.HealthController.GetNormalisedHealthValue() >= 0.2f) return;
			_player.HealthController.Heal(20);
		}

		private void CreateEnemies()
		{
			List<EnemyType> inactiveEnemies = new List<EnemyType>();
			inactiveEnemies.Add(EnemyType.Brawler);
			inactiveEnemies.Add(EnemyType.Brawler);
			inactiveEnemies.Add(EnemyType.Sentinel);
			inactiveEnemies.Add(EnemyType.Brawler);
			inactiveEnemies.Add(EnemyType.Sentinel);
			CombatManager.Instance().OverrideMaxSize(2);
			CombatManager.Instance().OverrideInactiveEnemies(inactiveEnemies);
		}
	}
}