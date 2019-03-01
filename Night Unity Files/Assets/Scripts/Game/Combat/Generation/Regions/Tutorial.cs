using System.Collections;
using System.Collections.Generic;
using Facilitating;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Generation.Shrines;
using Game.Combat.Player;
using Game.Combat.Ui;
using Game.Global.Tutorial;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class Tutorial : RegionGenerator
    {
        private PlayerCombat _player;

        protected override void Generate()
        {
            _player = PlayerCombat.Instance;
            GameObject tutorialFloor = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Tutorial Floor");
            Instantiate(tutorialFloor, Vector3.zero, Quaternion.identity);
            CreateRocks();
            CharacterManager.SelectedCharacter.Attributes.SetTutorialValues();
            StartCoroutine(ShowTutorial());
        }

        private void CreateRocks()
        {
            float[] _radii = {6, 9, 11};
            float angleInterval = 360f / 25f;
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
            ResourceItem item = ResourceTemplate.Create("Cooked Meat");
            Vector2 position = AdvancedMaths.RandomDirection() * Random.Range(2f, 5f);
            Loot loot = new Loot(position);
            CharacterManager.CurrentRegion().Containers.Add(loot);
            loot.SetResource(item);
            loot.CreateObject();

            item = ResourceTemplate.Create("Water");
            position = AdvancedMaths.RandomDirection() * Random.Range(2f, 5f);
            loot = new Loot(position);
            CharacterManager.CurrentRegion().Containers.Add(loot);
            loot.SetResource(item);
            loot.CreateObject();
        }

        private IEnumerator ShowBasicControls()
        {
            yield return new WaitForSeconds(2f);
            EventTextController.SetOverrideText("Move using [WASD]");
            while (!InputHandler.InputAxisWasPressed(InputAxis.Horizontal) && !InputHandler.InputAxisWasPressed(InputAxis.Vertical)) yield return null;
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            EventTextController.SetOverrideText("Rotate using [J] and [L]");
            while (!InputHandler.InputAxisWasPressed(InputAxis.SwitchTab)) yield return null;
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            EventTextController.SetOverrideText("You can also aim with the mouse, and rotate the camera by holding [RMB]");
            yield return new WaitForSeconds(5);
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            EventTextController.SetOverrideText("Press [K], or [LMB] to Fire");
            while (!InputHandler.InputAxisWasPressed(InputAxis.Fire)) yield return null;
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            EventTextController.SetOverrideText("Reload with [R]");
            while (!InputHandler.InputAxisWasPressed(InputAxis.Reload)) yield return null;
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);
        }

        private IEnumerator ShowAdrenalineTutorial()
        {
            _player.UpdateAdrenaline(10000);
            _player.HealthController.TakeDamage(_player.HealthController.GetMaxHealth() * 0.6f);
            CombatManager.Instance().SetForceShowHud(true);
            yield return new WaitForSeconds(0.5f);
            TutorialManager.TryOpenTutorial(7, new List<TutorialOverlay> {new TutorialOverlay(RageBarController.AdrenalineRect())});
            while (TutorialManager.IsTutorialVisible()) yield return null;
            EventTextController.SetOverrideText("Dash with [SPACE], this consumes some adrenaline");
            while (!InputHandler.InputAxisWasPressed(InputAxis.Sprint)) yield return null;
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            EventTextController.SetOverrideText("You gain adrenaline by dealing damage to enemies");
            yield return new WaitForSeconds(4);
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);
        }

        private bool _seenAccuracyTutorial;

        private IEnumerator ShowHealthTutorial()
        {
            CreateEnemies();

            EventTextController.SetOverrideText("Defeat all enemies");
            yield return new WaitForSeconds(5);
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);
            while (!CombatManager.Instance().ClearOfEnemies())
            {
                if (!_seenAccuracyTutorial && CombatManager.Instance().InactiveEnemyCount() == 0 && PlayerCombat.Instance.GetTarget() != null)
                {
                    TutorialOverlay overlay = new TutorialOverlay(EnemyUi.Instance.UiHitController.GetComponent<RectTransform>());
                    TutorialManager.TryOpenTutorial(8, overlay);
                    _seenAccuracyTutorial = true;
                }

                yield return null;
            }
        }

        private IEnumerator ShowAttributeTutorial()
        {
            EventTextController.SetOverrideText("Will can be used to restore attributes in and out of combat");
            yield return new WaitForSeconds(5);
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            EventTextController.SetOverrideText("Restoring your Fettle attribute will recover health in combat");
            yield return new WaitForSeconds(5);
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            UiGearMenuController.SetOpenAllowed(true);
            EventTextController.SetOverrideText("Open your inventory with [I] and navigate to the Meditate tab to restore your Fettle");
            while (!UiGearMenuController.IsOpen()) yield return null;
            UiGearMenuController.SetCloseAllowed(false);
            EventTextController.CloseOverrideText();
            while (_player.HealthController.GetNormalisedHealthValue() < 0.5f) yield return null;
            UiGearMenuController.SetCloseAllowed(true);
            while (UiGearMenuController.IsOpen()) yield return null;
            UiGearMenuController.SetOpenAllowed(false);
            yield return new WaitForSeconds(1);

            EventTextController.SetOverrideText("Attributes can also be restored by sleeping");
            yield return new WaitForSeconds(5);
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);
        }

        private IEnumerator ShowCompassTutorial()
        {
            CreateFoodAndWater();
            EventTextController.SetOverrideText("Use your compass with [E]");
            while (!InputHandler.InputAxisWasPressed(InputAxis.Compass)) yield return null;
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            TutorialManager.TryOpenTutorial(3, new List<TutorialOverlay> {new TutorialOverlay(UiCompassPulseController.CompassRect())});
            while (TutorialManager.IsTutorialVisible()) yield return null;

            EventTextController.SetOverrideText("Collect the revealed items with [T] or [MMB]");
            yield return new WaitForSeconds(5);
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            while (CharacterManager.CurrentRegion().Containers.Count > 0) yield return null;

            EventTextController.SetOverrideText("Consume food and water to stave off dehydration and thirst");
            yield return new WaitForSeconds(5);
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            UiGearMenuController.SetOpenAllowed(true);
            EventTextController.SetOverrideText("Open your inventory with [I] and navigate to the consume tab");
            while (!UiGearMenuController.IsOpen()) yield return null;
            UiGearMenuController.SetCloseAllowed(false);
            EventTextController.CloseOverrideText();
            while (Inventory.GetResourceQuantity("Water") > 0 && Inventory.GetResourceQuantity("Cooked Meat") > 0) yield return null;
            UiGearMenuController.SetCloseAllowed(true);
            while (UiGearMenuController.IsOpen()) yield return null;
            UiGearMenuController.SetOpenAllowed(false);
            yield return new WaitForSeconds(1);
        }

        private IEnumerator ShowLeaveTutorial()
        {
            CombatManager.Instance().SetForceShowHud(false);
            RiteStarter.GenerateTutorialStarter();
            EventTextController.SetOverrideText("Walk into the portal to begin your journey");
            yield return new WaitForSeconds(2);
            EventTextController.CloseOverrideText();
            CharacterManager.SelectedCharacter.Attributes.ResetValues();
            UiGearMenuController.SetOpenAllowed(true);
        }

        private IEnumerator ShowTutorial()
        {
            UiGearMenuController.SetOpenAllowed(false);
            yield return StartCoroutine(ShowBasicControls());
            yield return StartCoroutine(ShowAdrenalineTutorial());
            yield return StartCoroutine(ShowHealthTutorial());
            yield return StartCoroutine(ShowAttributeTutorial());
            yield return StartCoroutine(ShowCompassTutorial());
            yield return StartCoroutine(ShowLeaveTutorial());
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