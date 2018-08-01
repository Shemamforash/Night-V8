using System.Collections.Generic;
using System.Net;
using DG.Tweening;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Combat.Player;
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

        public override void Awake()
        {
            base.Awake();
            _text = gameObject.FindChildWithName<TextMeshProUGUI>("Text");
            _text.color = UiAppearanceController.InvisibleColour;
            HealthController.SetInitialHealth(1000, this);
            transform.SetParent(GameObject.Find("World").transform);
            _moveBehaviour = GetComponent<MoveBehaviour>();
            MovementController.SetSpeed(1);
        }

        public override Weapon Weapon()
        {
            return null;
        }

        public override CharacterCombat GetTarget()
        {
            return null;
        }

        public static void Generate(Vector2 position)
        {
            if (_shelterCharacterPrefab == null) _shelterCharacterPrefab = Resources.Load<GameObject>("Prefabs/Combat/Shelter Character");
            GameObject characterObject = Instantiate(_shelterCharacterPrefab);
            characterObject.transform.position = position;
        }

        public void Enter()
        {
            Sequence sequence = DOTween.Sequence();
            _text.text = "Protect me";
            sequence.Append(_text.DOFade(1, 1));
            sequence.AppendInterval(3);
            sequence.Append(_text.DOFade(0, 1));
            GenerateEncounter();
            _moveBehaviour.GoToCell(PathingGrid.GetCellOutOfRange(transform.position));
        }

        public override void Update()
        {
            if (!CurrentCell().EdgeCell) return;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(GetComponent<SpriteRenderer>().DOColor(UiAppearanceController.InvisibleColour, 2f));
            sequence.AppendCallback(() =>
            {
                CombatManager.Enemies().ForEach(e => e.SetTarget(PlayerCombat.Instance));
                Destroy(gameObject);
                Characters.Player character = CombatManager.Region()._characterHere;
                CharacterManager.AddCharacter(character);
            });
        }

        private Cell _targetLastCell;

        public override void Kill()
        {
            base.Kill();
            CombatManager.Enemies().ForEach(e => e.SetTarget(PlayerCombat.Instance));
        }

        private void GenerateEncounter()
        {
            int size = WorldState.Difficulty() + 10;
            List<EnemyTemplate> allowedTypes = WorldState.GetAllowedHumanEnemyTypes();
            while (size > 0)
            {
                EnemyTemplate template = Helper.RandomElement(allowedTypes);
                CombatManager.QueueEnemyToAdd(template, this);
                size -= template.Value;
            }
        }

        public override void TakeDamage(Shot shot)
        {
            HealthController.TakeDamage(shot.DamageDealt());
        }
    }
}