using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
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

        public bool OnScreen() => Helper.IsObjectInCameraView(gameObject);

        public override Weapon Weapon() => null;

        public override void Update()
        {
            base.Update();
            PushAwayFromNeighbors();
            UpdateAlpha();
            if (!CombatManager.InCombat()) return;
            UpdateRotation();
            if (MoveBehaviour.Moving()) return;
            CurrentAction?.Invoke();
        }

        protected bool MoveToCover(Action reachCoverAction)
        {
            bool moving = MoveBehaviour.MoveToCover(reachCoverAction);
            if(moving) SetActionText("Seeking Cover");
            return moving;
        }

        protected SpriteRenderer Sprite;

        private void UpdateAlpha()
        {
            float distanceToPlayer = Vector2.Distance(transform.position, GetTarget().transform.position);
            float alpha;
            float visibility = CombatManager.VisibilityRange();
            if (distanceToPlayer > visibility) alpha = 0;
            else alpha = 1 - distanceToPlayer / visibility;
            Color c = Sprite.color;
            c.a = alpha;
            Sprite.color = c;
        }

        private void PushAwayFromNeighbors()
        {
            List<CharacterCombat> chars = CombatManager.GetCharactersInRange(transform.position, 1f);
            Vector2 forceDir = Vector2.zero;
            chars.ForEach(c =>
            {
                if (c == this) return;
                Vector2 dir = c.transform.position - transform.position;
                if (dir == Vector2.zero) dir = AdvancedMaths.RandomVectorWithinRange(transform.position, 1).normalized;
                float force = 1 / dir.magnitude;
                forceDir += -dir * force;
            });
            MovementController.AddForce(forceDir);
        }

        private void UpdateRotation()
        {
            float rotation;
            if (FacePlayer && GetTarget() != null)
            {
                rotation = AdvancedMaths.AngleFromUp(transform.position, GetTarget().transform.position);
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));
                return;
            }

            rotation = AdvancedMaths.AngleFromUp(transform.position, transform.position + (Vector3) GetComponent<Rigidbody2D>().velocity);
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));
        }

        public virtual void Initialise(Enemy enemy)
        {
            ArmourController = enemy.ArmourController;
            Enemy = enemy;
            MovementController.SetSpeed(Enemy.Template.Speed);
            HealthController.SetInitialHealth(Enemy.Template.Health, this);
            transform.SetParent(GameObject.Find("World").transform);
            transform.position = PathingGrid.GetCellNearMe(Vector2.zero, 8f, 4f).Position;
            Sprite spriteImage = Resources.Load<Sprite>("Images/Enemy Symbols/" + GetEnemyName());
            Sprite = GetComponent<SpriteRenderer>();
            if (spriteImage == null) return;
            Sprite.sprite = spriteImage;
            SetTarget(PlayerCombat.Instance);
            MoveBehaviour = gameObject.AddComponent<MoveBehaviour>();
        }

        protected void SetActionText(string actionText)
        {
            ActionText = actionText;
            EnemyUi.Instance().UpdateActionText(this, actionText);
        }

        public override void TakeDamage(Shot shot)
        {
            base.TakeDamage(shot);
            CombatManager.IncreaseDamageDealt(shot.DamageDealt());
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
    }
}