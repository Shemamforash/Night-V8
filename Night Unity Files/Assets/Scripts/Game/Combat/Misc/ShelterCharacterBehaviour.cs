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
    public class ShelterCharacterBehaviour : CharacterCombat, ICombatEvent
    {
        private static GameObject _shelterCharacterPrefab;
        private MoveBehaviour _moveBehaviour;
        private Cell _targetLastCell;
        private bool _leaving;
        private static ShelterCharacterBehaviour _instance;
        private bool _shouldShowText = true;
        private readonly string[] _textStrings = {"Please help me", "Protect me", "I thought this was the end", "Please let me come with you"};
        private string _textToShow;
        private Cell _targetCell;

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

            _textToShow = _textStrings.RandomElement();
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

        public bool ShowText()
        {
            return _shouldShowText;
        }

        public void Update()
        {
            MyUpdate();
        }

        private void UpdateRotation()
        {
            float zRot = AdvancedMaths.AngleFromUp(Vector2.zero, MovementController.GetVelocity());
            Sprite.transform.rotation = Quaternion.Euler(0, 0, zRot);
        }

        public override void MyUpdate()
        {
            if (!CombatManager.IsCombatActive()) return;
            if (_leaving) return;
            UpdateEnemyTarget();
            CheckToLeave();
            UpdateRotation();
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
            _shouldShowText = false;
            _targetCell = WorldGrid.GetEdgeCell(transform.position);
            _moveBehaviour.GoToCell(_targetCell);
            List<Enemy> enemies = new List<Enemy>();
            List<EnemyTemplate> allowedTypes = WorldState.GetAllowedHumanEnemyTypes();
            for (int i = 0; i < 100; ++i) enemies.Add(new Enemy(allowedTypes.RandomElement()));
            CombatManager.OverrideInactiveEnemies(enemies);
        }
    }
}