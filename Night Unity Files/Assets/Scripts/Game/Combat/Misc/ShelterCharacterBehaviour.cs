using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
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
        private MoveBehaviour _moveBehaviour;
        private Cell _targetLastCell;
        private static ShelterCharacterBehaviour _instance;
        private readonly string[] _textStrings = {"Please help me", "Protect me", "I thought this was the end", "Please let me come with you"};
        private Cell _targetCell;
        private bool _seenText, _freed, _leaving;
        private Sequence _sequence;

        public static ShelterCharacterBehaviour Instance()
        {
            return _instance;
        }

        protected override void Awake()
        {
            SpriteFlash = gameObject.FindChildWithName<DamageSpriteFlash>("Icon");
            _bloodSpatter = SpriteFlash.GetComponent<BloodSpatter>();
            Sprite = SpriteFlash.GetComponent<SpriteRenderer>();
            MovementController = GetComponent<MovementController>();
            WeaponAudio = gameObject.FindChildWithName<WeaponAudioController>("Weapon Audio");

            IsPlayer = true;
            _instance = this;
            HealthController.SetInitialHealth(1000, this);
            transform.SetParent(GameObject.Find("World").transform);
            _moveBehaviour = GetComponent<MoveBehaviour>();
            MovementController.SetSpeed(1);
            ArmourController.AutoFillSlots(10);
        }

        public override Weapon Weapon()
        {
            return null;
        }

        public override CanTakeDamage GetTarget()
        {
            return null;
        }

        public static void Generate()
        {
            if (_shelterCharacterPrefab == null) _shelterCharacterPrefab = Resources.Load<GameObject>("Prefabs/Combat/Shelter Character");
            GameObject characterObject = Instantiate(_shelterCharacterPrefab);
            characterObject.transform.position = Vector2.zero;
        }

        public void Update()
        {
            MyUpdate();
        }

        private void UpdateRotation()
        {
            float zRot;
            if (_freed) zRot = AdvancedMaths.AngleFromUp(Vector2.zero, MovementController.GetVelocity());
            else zRot = AdvancedMaths.AngleFromUp(transform.position, PlayerCombat.Position());
            Sprite.transform.rotation = Quaternion.Euler(0, 0, zRot);
        }

        public override void MyUpdate()
        {
            if (!CombatManager.IsCombatActive()) return;
            TryShowText();
            UpdateRotation();
            if (_leaving) return;
            UpdateEnemyTarget();
            CheckToLeave();
        }

        private void CheckToLeave()
        {
            if (_targetCell == null) return;
            if (transform.Distance(_targetCell.Position) > 0.5f) return;
            _leaving = true;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(Sprite.DOColor(UiAppearanceController.InvisibleColour, 2f));
            sequence.AppendCallback(() =>
            {
                CombatManager.ClearInactiveEnemies();
                ResetEnemyTargets();
                Destroy(gameObject);
                Characters.Player character = CombatManager.Region().CharacterHere;
                CharacterManager.AddCharacter(character);
                CombatManager.Region().CharacterHere = null;
            });
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
                CanTakeDamage target = distanceToPlayer < 2f ? PlayerCombat.Instance : (CanTakeDamage) this;
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

        private void TryShowText()
        {
            if (_seenText) return;
            if (transform.Distance(PlayerCombat.Instance.transform) > 5f) return;
            _seenText = true;
            EventTextController.SetOverrideText(_textStrings.RandomElement());
            _sequence = DOTween.Sequence();
            _sequence.AppendInterval(3f);
            _sequence.AppendCallback(EventTextController.CloseOverrideText);
            _seenText = true;
        }

        public void Free()
        {
            _freed = true;
            _targetCell = WorldGrid.GetEdgeCell(transform.position);
            _moveBehaviour.GoToCell(_targetCell);
            List<Enemy> enemies = new List<Enemy>();
            List<EnemyTemplate> allowedTypes = WorldState.GetAllowedHumanEnemyTypes();
            for (int i = 0; i < 100; ++i) enemies.Add(new Enemy(allowedTypes.RandomElement()));
            CombatManager.OverrideInactiveEnemies(enemies);
            _sequence?.Kill();
            EventTextController.SetOverrideText("Protect the survivor");
            _sequence = DOTween.Sequence();
            _sequence.AppendInterval(2f);
            _sequence.AppendCallback(EventTextController.CloseOverrideText);
        }
    }
}