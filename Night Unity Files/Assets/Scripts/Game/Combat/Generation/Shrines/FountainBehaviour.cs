using System.Collections;
using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Player;
using Game.Combat.Ui;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Input;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
    public class FountainBehaviour : BasicShrineBehaviour, IInputListener
    {
        private static GameObject _fountainPrefab;
        private List<EnemyBehaviour> _enemies;
        private bool _started;
        private Region _region;

        public static void Generate(Region region)
        {
            if (_fountainPrefab == null) _fountainPrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Fountain");
            GameObject riteShrineObject = Instantiate(_fountainPrefab);
            riteShrineObject.GetComponent<FountainBehaviour>().Initialise(region);
        }

        private void Initialise(Region region)
        {
            _region = region;
            transform.position = region.ShrinePosition;
            PathingGrid.AddBlockingArea(region.ShrinePosition, 1.5f);
            if (!_region.FountainVisited) return;
            Destroy(this);
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            InputHandler.UnregisterInputListener(this);
            PlayerUi.FadeTextOut();
        }

        protected override void StartShrine()
        {
            InputHandler.RegisterInputListener(this);
            PlayerUi.SetEventText("Drink from the fountain [T}");
        }

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (axis != InputAxis.TakeItem) return;
            InputHandler.UnregisterInputListener(this);
            Destroy(gameObject.GetComponent<Collider2D>());
            Triggered = true;
            PlayerUi.SetEventText("Earn your blessing", 2f);
            StartCoroutine(SpawnEnemies());
        }

        private IEnumerator SpawnEnemies()
        {
            int daysSpent = WorldState.GetDaysSpentHere() + 5;
            List<EnemyTemplate> allowedEnemies = WorldState.GetAllowedHumanEnemyTypes();
            float timeToSpawn = 0f;
            for (int i = 0; i < Random.Range(daysSpent / 2f, daysSpent); ++i)
            {
                while (timeToSpawn > 0f)
                {
                    timeToSpawn -= Time.deltaTime;
                    yield return null;
                }

                Vector2 spawnPosition = PathingGrid.GetCellNearMe(transform.position, 5).Position;
                SpawnTrailController.Create(transform.position, spawnPosition, allowedEnemies.RandomElement().EnemyType);
                timeToSpawn = Random.Range(0.5f, 1f);
                yield return null;
            }
        }

        protected override void OnEnemiesDead()
        {
            Succeed();
        }

        protected override void Succeed()
        {
            CharacterManager.SelectedCharacter.Attributes.Get(AttributeType.Thirst).Decrement(10);
            CharacterManager.SelectedCharacter.Attributes.Get(AttributeType.Hunger).Decrement(10);
            CharacterManager.SelectedCharacter.Attributes.Get(AttributeType.Endurance).SetToMax();
            CharacterManager.SelectedCharacter.Attributes.Get(AttributeType.Perception).SetToMax();
            CharacterManager.SelectedCharacter.Attributes.Get(AttributeType.Strength).SetToMax();
            CharacterManager.SelectedCharacter.Attributes.Get(AttributeType.Willpower).SetToMax();
            PlayerCombat.Instance.HealthController.Heal(1000000);
            PlayerCombat.Instance.ResetCompass();
        }

        public void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }
    }
}