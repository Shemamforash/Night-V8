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
    public class ShelterCharacterBehaviour : CharacterCombat
    {
        private static GameObject _shelterCharacterPrefab;
        private TextMeshProUGUI _text;
        private MoveBehaviour _moveBehaviour;
        private Cell _targetLastCell;
        private bool _leaving;
        private int _maxEncounterSize;

        protected override void Awake()
        {
            base.Awake();
            _text = gameObject.FindChildWithName<TextMeshProUGUI>("Text");
            _text.color = UiAppearanceController.InvisibleColour;
            HealthController.SetInitialHealth(1000, this);
            transform.SetParent(GameObject.Find("World").transform);
            _moveBehaviour = GetComponent<MoveBehaviour>();
            MovementController.SetSpeed(1);
            ArmourController = new ArmourController(null);
            ArmourController.AutoFillSlots(10);
            _maxEncounterSize = (WorldState._currentLevel + 1) * 5;
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

        public void Enter()
        {
            Sequence sequence = DOTween.Sequence();
            _text.text = "Protect me";
            sequence.Append(_text.DOFade(1, 1));
            sequence.AppendInterval(3);
            sequence.Append(_text.DOFade(0, 1));
            _moveBehaviour.GoToCell(PathingGrid.GetCellOutOfRange(transform.position));
        }

        public void Update()
        {
            if (!CombatManager.IsCombatActive()) return;
            MyUpdate();
        }

        public override void MyUpdate()
        {
            if (_leaving) return;
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
                Characters.Player character = CombatManager.Region()._characterHere;
                CharacterManager.AddCharacter(character);
            });
        }

        private void UpdateEnemySpawns()
        {
            int currentSize = CombatManager.Enemies().Sum(e => ((EnemyBehaviour) e).Enemy.Template.Value);
            int size = _maxEncounterSize - currentSize;
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
            CombatManager.Enemies().ForEach(e => ((EnemyBehaviour)e).SetTarget(PlayerCombat.Instance));
        }

        private void UpdateEnemyTarget()
        {
            CombatManager.Enemies().ForEach(e =>
            {
                float distanceToPlayer = e.transform.Distance(PlayerCombat.Instance.transform);
                CanTakeDamage target = distanceToPlayer < 3f ? PlayerCombat.Instance : (CanTakeDamage)this;
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
    }
}