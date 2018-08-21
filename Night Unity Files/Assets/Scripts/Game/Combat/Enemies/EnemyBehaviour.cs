﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
 using DG.Tweening;
 using Game.Characters;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Combat.Ui;
using Game.Gear.Weapons;
using NUnit.Framework;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies
{
    public class EnemyBehaviour : CharacterCombat
    {
        public string ActionText;
        public Action CurrentAction;
        public Enemy Enemy;
        protected bool FacePlayer;
        public MoveBehaviour MoveBehaviour;

        public override Weapon Weapon() => null;

        public override void Update()
        {
            base.Update();
            PushAwayFromNeighbors();
            if (!CombatManager.InCombat()) return;
            UpdateRotation();
            UpdateMarkTime();
            if (MoveBehaviour.Moving()) return;
            CurrentAction?.Invoke();
        }

        private void UpdateMarkTime()
        {
            if (!_marked) return;
            if (_markTime < 0f) return;
            _markTime -= Time.deltaTime;
            if (_markTime < 0f) PlayerCombat.Instance.EndMark(this);
        }

        protected bool MoveToCover(Action reachCoverAction)
        {
            bool moving = MoveBehaviour.MoveToCover();
            if (moving)
            {
                SetActionText("Seeking Cover");
                CurrentAction = reachCoverAction;
            }

            return moving;
        }

        protected SpriteRenderer Sprite;
        private float _markTime;
        private bool _marked;

        private void PushAwayFromNeighbors()
        {
            List<ITakeDamageInterface> chars = CombatManager.GetCharactersInRange(transform.position, 1f);
            Vector2 forceDir = Vector2.zero;
            chars.ForEach(c =>
            {
                EnemyBehaviour enemy = c as EnemyBehaviour;
                if (enemy == null || enemy == this) return;
                if (Vector2.Distance(transform.position, enemy.transform.position) > 0.25f) return;
                Vector2 dir = enemy.transform.position - transform.position;
                if (dir == Vector2.zero) dir = AdvancedMaths.RandomVectorWithinRange(transform.position, 1).normalized;
                float force = 1 / dir.magnitude;
                forceDir += -dir * force;
            });
            MovementController.AddForce(forceDir);
        }

        private void UpdateRotation()
        {
            float rotation;
            if (FacePlayer && GetTarget() != null) rotation = AdvancedMaths.AngleFromUp(transform.position, GetTarget().transform.position);
            else rotation = AdvancedMaths.AngleFromUp(transform.position, transform.position + (Vector3) GetComponent<Rigidbody2D>().velocity);
//            float currentRotation = transform.eulerAngles.z;
//            float rotateBy = AdvancedMaths.GetSmallestAngleBetweenTwoAngles(currentRotation, rotation);
//            rotateBy *= Time.deltaTime;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));
        }

        public virtual void Initialise(Enemy enemy)
        {
            Enemy = enemy;
            InitialiseAttributes();
            SetTarget(PlayerCombat.Instance);
            SetPosition();
            AssignSprite();
        }

        private void InitialiseAttributes()
        {
            ArmourController = Enemy.ArmourController;
            MovementController.SetSpeed(Enemy.Template.Speed);
            HealthController.SetInitialHealth(Enemy.Template.Health, this);
            MoveBehaviour = gameObject.AddComponent<MoveBehaviour>();
        }

        private void SetPosition()
        {
            transform.SetParent(GameObject.Find("World").transform);
            transform.position = PathingGrid.GetCellNearMe(Vector2.zero, 8f, 4f).Position;
        }

        private void AssignSprite()
        {
            Sprite spriteImage = Resources.Load<Sprite>("Images/Enemy Symbols/" + GetEnemyName());
            Sprite = GetComponent<SpriteRenderer>();
            if (spriteImage == null) return;
            Sprite.sprite = spriteImage;
        }

        protected void SetActionText(string actionText)
        {
            ActionText = actionText;
            EnemyUi.Instance().UpdateActionText(this, actionText);
        }

        public override void TakeShotDamage(Shot shot)
        {
            base.TakeShotDamage(shot);
            CombatManager.IncreaseDamageDealt(shot.DamageDealt());
            PlayerCombat.Instance.DamageDealtSinceMarkStarted += shot.DamageDealt();
            EnemyUi.Instance().RegisterHit(this);
        }

        public override void Kill()
        {
            base.Kill();
            Characters.Player player = ((PlayerCombat) GetTarget()).Player;
            if (player != null)
            {
                if (player.Attributes.SpreadSickness && IsSick())
                {
                    int sicknessStacks = SicknessStacks;
                    if (sicknessStacks > 5) sicknessStacks = 5;
                    CombatManager.GetCharactersInRange(transform.position, 3).ForEach(c =>
                    {
                        EnemyBehaviour b = c as EnemyBehaviour;
                        if (b == null) return;
                        b.Sicken(sicknessStacks);
                    });
                }

                player.IncreaseWeaponKills();
            }

            Loot loot = Enemy.DropLoot(transform.position);

            if (loot.IsValid)
            {
                loot.CreateObject(true);
                CombatManager.Region().Containers.Add(loot);
            }

            CombatManager.Remove(this);
            Enemy.Kill();
            Destroy(gameObject);
        }

        protected virtual void ReachTarget()
        {
        }

        public virtual string GetEnemyName() => Enemy.Name;

        public void Mark()
        {
            _marked = true;
            _markTime = 5f;
        }
    }
}