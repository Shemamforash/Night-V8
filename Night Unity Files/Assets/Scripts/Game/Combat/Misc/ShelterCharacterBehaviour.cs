using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Combat.Ui;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class ShelterCharacterBehaviour : CharacterCombat, ICombatEvent
    {
        private static GameObject _shelterCharacterPrefab;
        private MoveBehaviour _moveBehaviour;
        private Cell _targetLastCell;
        private bool _leaving;
        private int _maxEncounterSize;
        private bool _spawnEnemies;
        private static ShelterCharacterBehaviour _instance;
        private bool _shouldShowText = true;
        private readonly string[] _textStrings = {"Please help me", "Protect me", "I thought this was the end", "Please let me come with you"};
        private string _textToShow;

        public static ShelterCharacterBehaviour Instance()
        {
            return _instance;
        }

        protected override void Awake()
        {
            _textToShow = _textStrings.RandomElement();
            IsPlayer = true;
            base.Awake();
            _instance = this;
            HealthController.SetInitialHealth(1000, this);
            transform.SetParent(GameObject.Find("World").transform);
            _moveBehaviour = GetComponent<MoveBehaviour>();
            MovementController.SetSpeed(1);
            ArmourController = new ArmourController(null);
            ArmourController.AutoFillSlots(10);
            _maxEncounterSize = WorldState._currentLevel * 5;
        }

        public override Weapon Weapon()
        {
            return null;
        }

        public override CanTakeDamage GetTarget()
        {
            return null;
        }

        public static void Generate(Vector2 position)
        {
            Debug.Log("generated");
            if (_shelterCharacterPrefab == null) _shelterCharacterPrefab = Resources.Load<GameObject>("Prefabs/Combat/Shelter Character");
            GameObject characterObject = Instantiate(_shelterCharacterPrefab);
            characterObject.transform.position = position;
            Debug.Log(position);
        }

        public bool ShowText()
        {
            return _shouldShowText;
        }

        public void Update()
        {
            if (!CombatManager.IsCombatActive()) return;
            MyUpdate();
        }

        public override void MyUpdate()
        {
            if (_leaving) return;
            if (!_spawnEnemies) return;
            UpdateEnemyTarget();
            UpdateEnemySpawns();
            if (!CurrentCell().IsEdgeCell) return;
            _leaving = true;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(GetComponent<SpriteRenderer>().DOColor(UiAppearanceController.InvisibleColour, 2f));
            sequence.AppendCallback(() =>
            {
                ResetEnemyTargets();
                Destroy(gameObject);
                Characters.Player character = CombatManager.Region().CharacterHere;
                CharacterManager.AddCharacter(character);
                CombatManager.Region().CharacterHere = null;
            });
        }

        private void UpdateEnemySpawns()
        {
            int currentSize = CombatManager.Enemies().Sum(e => ((EnemyBehaviour) e).Enemy.Template.Value);
            int size = _maxEncounterSize - currentSize;
            if (size == 0 || !Helper.RollDie(0, 30)) return;
            List<EnemyTemplate> allowedTypes = WorldState.GetAllowedHumanEnemyTypes();
            while (size > 0)
            {
                EnemyTemplate template = allowedTypes.RandomElement();
                CombatManager.QueueEnemyToAdd(template, this);
                size -= template.Value;
            }
        }

        private void ResetEnemyTargets()
        {
            CombatManager.Enemies().ForEach(e => ((EnemyBehaviour) e).SetTarget(PlayerCombat.Instance));
        }

        private void UpdateEnemyTarget()
        {
            CombatManager.Enemies().ForEach(e =>
            {
                float distanceToPlayer = e.transform.Distance(PlayerCombat.Instance.transform);
                CanTakeDamage target = distanceToPlayer < 3f ? PlayerCombat.Instance : (CanTakeDamage) this;
                ((EnemyBehaviour) e).SetTarget(target);
            });
        }

        public override string GetDisplayName()
        {
            return new[] {"Lonely Wanderer", "Stranger", "Haggard Figure"}.RandomElement();
        }

        public override void Kill()
        {
            base.Kill();
            ResetEnemyTargets();
        }

        public override void TakeShotDamage(Shot shot)
        {
            HealthController.TakeDamage(shot.Attributes().DamageDealt());
        }

        public float InRange()
        {
            return transform.Distance(PlayerCombat.Instance.transform);
        }

        public string GetEventText()
        {
            return _textToShow;
        }

        public void Activate()
        {
            _spawnEnemies = true;
            _shouldShowText = false;
            _moveBehaviour.GoToCell(PathingGrid.GetCellOutOfRange(transform.position));
        }
    }
}