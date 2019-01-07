using System;
using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Gear.Weapons;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies
{
    public class EnemyBehaviour : CharacterCombat
    {
        public Action CurrentAction;
        public Enemy Enemy;
        public MoveBehaviour MoveBehaviour;

        public override Weapon Weapon() => null;

        public override void MyUpdate()
        {
            base.MyUpdate();
            PushAwayFromNeighbors();
            UpdateRotation();
            CurrentAction?.Invoke();
        }

        public override string GetDisplayName()
        {
            return Enemy.Name;
        }

        private void PushAwayFromNeighbors()
        {
            return;
            List<CanTakeDamage> chars = CombatManager.GetCharactersInRange(transform.position, 2f);
            Vector2 forceDir = Vector2.zero;
            chars.ForEach(c =>
            {
                EnemyBehaviour enemy = c as EnemyBehaviour;
                if (enemy == null || enemy == this) return;
                Vector2 dir = enemy.transform.position - transform.position;
                if (dir == Vector2.zero) dir = AdvancedMaths.RandomVectorWithinRange(transform.position, 1).normalized;
                float force = 20f / dir.magnitude;
                if (force > 50) force = 50;
                forceDir += -dir * force;
            });
            MovementController.AddForce(forceDir);
        }

        protected virtual void UpdateRotation()
        {
            float targetRotation;
            if (GetTarget() != null) targetRotation = AdvancedMaths.AngleFromUp(transform.position, TargetPosition());
            else targetRotation = AdvancedMaths.AngleFromUp(transform.position, transform.position + (Vector3) GetComponent<Rigidbody2D>().velocity);
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, targetRotation));
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
            ArmourController.Reset();
            float speed = Enemy.Template.Speed;
            speed = Random.Range(speed * 0.8f, speed * 1.2f);
            MovementController.SetSpeed(speed);
            HealthController.SetInitialHealth(Enemy.GetHealth(), this);
            MoveBehaviour = gameObject.AddComponent<MoveBehaviour>();
        }

        private void SetPosition()
        {
            transform.SetParent(GameObject.Find("World").transform);
            Cell c = PathingGrid.GetCellNearMe(PlayerCombat.Instance.transform.position, 10f, 7f);
            if (c == null) c = PathingGrid.GetCellNearMe(PlayerCombat.Instance.transform.position, 20f, 7f);
            transform.position = c.Position;
        }

        private void AssignSprite()
        {
            Sprite spriteImage = Resources.Load<Sprite>("Images/Enemy Symbols/" + GetDisplayName());
            if (spriteImage == null) return;
            Sprite.sprite = spriteImage;
        }

        public override void TakeShotDamage(Shot shot)
        {
            float healthBefore = HealthController.GetCurrentHealth();
            base.TakeShotDamage(shot);
            if (HealthController.GetCurrentHealth() != 0 || healthBefore == 0) return;
            PlayerCombat.Instance.Player.IncreaseKills();
        }

        public override void Kill()
        {
            Characters.Player player = PlayerCombat.Instance.Player;
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

            Loot loot = Enemy.DropLoot(transform.position);
            loot?.CreateObject(true);
            PlayerCombat.Instance.TriggerEnemyDeathEffect();
            base.Kill();
        }
    }
}