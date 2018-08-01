using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Ui;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Input;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
    public class FountainBehaviour : BasicShrineBehaviour, IInputListener
    {
        public const int Width = 5;
        private static GameObject _fountainPrefab;
        private List<EnemyBehaviour> _enemies;
        private bool _started;

        public static void Generate(Vector2 position)
        {
            if (_fountainPrefab == null) _fountainPrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Fountain");
            GameObject riteShrineObject = Instantiate(_fountainPrefab);
            riteShrineObject.transform.position = position;
            PathingGrid.AddBlockingArea(position, 1.5f);
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
            int daysSpent = WorldState.GetDaysSpentHere() + 5;
            List<EnemyTemplate> allowedEnemies = WorldState.GetAllowedHumanEnemyTypes();
            for (int i = 0; i < Random.Range(daysSpent / 2f, daysSpent); ++i)
            {
                AddEnemy(CombatManager.QueueEnemyToAdd(Helper.RandomElement(allowedEnemies)));
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
        }

        public void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }
    }
}