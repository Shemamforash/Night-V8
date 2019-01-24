using System.Collections;
using System.Collections.Generic;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Generation.Shrines;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.InventorySystem;
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
            StartCoroutine(ShowControlsTutorial());
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
            ResourceItem item = ResourceTemplate.Create("Meat");
            Vector2 position = AdvancedMaths.RandomDirection() * Random.Range(2f, 5f);
            Loot loot = new Loot(position);
            CombatManager.GetCurrentRegion().Containers.Add(loot);
            loot.SetResource(item);
            loot.CreateObject();

            item = ResourceTemplate.Create("Water");
            position = AdvancedMaths.RandomDirection() * Random.Range(2f, 5f);
            loot = new Loot(position);
            CombatManager.GetCurrentRegion().Containers.Add(loot);
            loot.SetResource(item);
            loot.CreateObject();
        }

        private IEnumerator ShowControlsTutorial()
        {
            UiGearMenuController.SetOpenAllowed(false);
            yield return new WaitForSeconds(2f);
            EventTextController.SetOverrideText("Move using [WASD]");
            while (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0) yield return null;
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            EventTextController.SetOverrideText("Rotate using [J] and [L]");
            while (Input.GetAxis("SwitchTab") == 0) yield return null;
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            EventTextController.SetOverrideText("You can also aim with the mouse, and rotate the camera by holding [RMB]");
            yield return new WaitForSeconds(3);
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            EventTextController.SetOverrideText("Press [K], or [LMB] to Fire");
            while (Input.GetAxis("Fire") == 0 && Input.GetAxis("Mouse") == 0) yield return null;
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            EventTextController.SetOverrideText("Reload with [R]");
            while (Input.GetAxis("Reload") == 0) yield return null;
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            _player.UpdateAdrenaline(10000);
            EventTextController.SetOverrideText("Dash with [SPACE], this consumes some adrenaline");
            while (Input.GetAxis("Sprint") == 0) yield return null;
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            EventTextController.SetOverrideText("You gain adrenaline by dealing damage to enemies");
            yield return new WaitForSeconds(3);
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            _player.HealthController.TakeDamage(_player.HealthController.GetMaxHealth() * 0.75f);
            CreateEnemies();

            EventTextController.SetOverrideText("Defeat the enemies");
            yield return new WaitForSeconds(5);
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);
            while (!CombatManager.ClearOfEnemies()) yield return null;

            EventTextController.SetOverrideText("Will can be used to restore attributes in and out of combat");
            yield return new WaitForSeconds(3);
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            EventTextController.SetOverrideText("Restoring your Fettle attribute will recover health in combat");
            yield return new WaitForSeconds(3);
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            UiGearMenuController.SetOpenAllowed(true);
            EventTextController.SetOverrideText("Open your inventory with [I] and navigate to the Meditate tab to restore your health");
            while (!UiGearMenuController.IsOpen()) yield return null;
            UiGearMenuController.SetCloseAllowed(false);
            EventTextController.CloseOverrideText();
            while (_player.HealthController.GetNormalisedHealthValue() < 0.5f) yield return null;
            UiGearMenuController.SetCloseAllowed(true);
            while (UiGearMenuController.IsOpen()) yield return null;
            UiGearMenuController.SetOpenAllowed(false);
            yield return new WaitForSeconds(1);

            CreateFoodAndWater();

            EventTextController.SetOverrideText("Use your compass with [E]");
            while (Input.GetAxis("Compass") == 0) yield return null;
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            EventTextController.SetOverrideText("The compass reveals nearby objects");
            yield return new WaitForSeconds(3);
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            EventTextController.SetOverrideText("Collect the revealed items with [T] or [MMB]");
            yield return new WaitForSeconds(2);
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            while (CombatManager.GetCurrentRegion().Containers.Count > 0) yield return null;

            EventTextController.SetOverrideText("Consume food and water to stave off dehydration and thirst");
            yield return new WaitForSeconds(3);
            EventTextController.CloseOverrideText();
            yield return new WaitForSeconds(1);

            UiGearMenuController.SetOpenAllowed(true);
            EventTextController.SetOverrideText("Open your inventory with [I] and navigate to the consume tab");
            while (!UiGearMenuController.IsOpen()) yield return null;
            UiGearMenuController.SetCloseAllowed(false);
            EventTextController.CloseOverrideText();
            while (Inventory.GetResourceQuantity("Water") > 0 && Inventory.GetResourceQuantity("Meat") > 0) yield return null;
            UiGearMenuController.SetCloseAllowed(true);
            while (UiGearMenuController.IsOpen()) yield return null;
            UiGearMenuController.SetOpenAllowed(false);
            yield return new WaitForSeconds(1);


            RiteStarter.GenerateTutorialStarter();
            EventTextController.SetOverrideText("Walk into the portal to begin your journey");
            yield return new WaitForSeconds(2);
            EventTextController.CloseOverrideText();

            CharacterManager.SelectedCharacter.Attributes.ResetValues();
        }

        private void Update()
        {
            if (_player.HealthController.GetNormalisedHealthValue() >= 0.2f) return;
            _player.HealthController.Heal(20);
        }

        private void CreateEnemies()
        {
            List<Enemy> inactiveEnemies = new List<Enemy>();
            inactiveEnemies.Add(new Enemy(EnemyTemplate.GetEnemyTemplate(EnemyType.Brawler)));
            inactiveEnemies.Add(new Enemy(EnemyTemplate.GetEnemyTemplate(EnemyType.Brawler)));
            inactiveEnemies.Add(new Enemy(EnemyTemplate.GetEnemyTemplate(EnemyType.Sentinel)));
            inactiveEnemies.Add(new Enemy(EnemyTemplate.GetEnemyTemplate(EnemyType.Brawler)));
            inactiveEnemies.Add(new Enemy(EnemyTemplate.GetEnemyTemplate(EnemyType.Sentinel)));
            CombatManager.OverrideMaxSize(2, inactiveEnemies);
        }
    }
}