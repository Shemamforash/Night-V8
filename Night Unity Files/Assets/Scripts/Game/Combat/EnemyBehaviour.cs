using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Characters.Player;
using Game.Combat.CharacterUi;
using Game.Combat.Enemies;
using Game.Combat.Enemies.EnemyTypes;
using Game.Gear.Weapons;
using NUnit.Framework;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Game.Combat
{
    public class EnemyBehaviour : CharacterCombat
    {
        private readonly CharacterAttribute _visionRange = new CharacterAttribute(AttributeType.Vision, 5f);
        private readonly CharacterAttribute _detectionRange = new CharacterAttribute(AttributeType.Detection, 2f);

        private const int EnemyReloadMultiplier = 4;
        private const float MaxAimTime = 2f;

        private bool Alerted;
        private int IdealWeaponDistance;

        private Vector2 _originPosition;
        public Enemy Enemy;
        protected EnemyUi EnemyUi;
        private List<Cell> route = new List<Cell>();
        public Action CurrentAction;


        //        private readonly Cooldown _firingCooldown;
        //        private const float KnockdownDuration = 5f;
//        private bool _waitingForHeal;
        //        private const float AlphaCutoff = 0.2f;
//        private const float FadeVisibilityDistance = 5f;
//        private float _currentAlpha;


        public float DistanceToPlayer;
        
        private void UpdateDistance()
        {
            DistanceToPlayer = DistanceToTarget();
            UpdateDistanceAlpha();
        }

        private void UpdateDistanceAlpha()
        {
//            float distanceToMaxVisibility = CombatManager.VisibilityRange + FadeVisibilityDistance - DistanceToPlayer;
//            float alpha = 0;
//            if (DistanceToPlayer < CombatManager.VisibilityRange)
//            {
//                float normalisedDistance = Helper.Normalise(DistanceToPlayer, CombatManager.VisibilityRange);
//                alpha = 1f - normalisedDistance;
//                alpha = Mathf.Clamp(alpha, AlphaCutoff, 1f);
//            }
//            else if (distanceToMaxVisibility >= 0)
//            {
//                alpha = Helper.Normalise(distanceToMaxVisibility, FadeVisibilityDistance);
//                alpha = Mathf.Clamp(alpha, 0, AlphaCutoff);
//            }

//            SetAlpha(alpha);
        }

//        private void OnDrawGizmos()
//        {
//            Gizmos.color = Color.red;
//            Gizmos.DrawCube(PathingGrid.PositionToCell(transform.position).Position, new Vector3(0.5f, 0.5f, 0.5f));
//            Gizmos.color = Color.green;
//            Gizmos.DrawCube(transform.position, new Vector3(0.5f, 0.5f, 0.5f));
//            Gizmos.color = Color.red;
//            Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);
//            Gizmos.color = Color.green;
//            Gizmos.DrawCube(PathingGrid.PositionToCell(transform.position).Position, Vector3.one * 0.5f);
//            if (route.Count == 0) return;
//            for (int i = 1; i < route.Count; ++i)
//            {
//                Gizmos.DrawLine(route[i - 1].Position, route[i].Position);
//            }
//        }


        public override CharacterCombat GetTarget() => CombatManager.Player;
        public override UIArmourController ArmourController() => EnemyUi.ArmourController;
        public override UIHealthBarController HealthController() => EnemyUi.HealthController;
        public UIHitController HitController() => EnemyUi.UiHitController;

        public void SetSelected()
        {
            CombatManager.Player.SetTarget(this);
            EnemyUi.PrimaryButton.Button().Select();
            EnemyUi.CanvasGroup.alpha = 1;
        }

        public virtual void Initialise(Enemy enemy, EnemyUi enemyUi)
        {
            EnemyUi = enemyUi;
            EnemyUi.NameText.text = enemy.Name;
            Enemy = enemy;
            if (Weapon() != null) IdealWeaponDistance = PathingGrid.WorldToGridDistance(Weapon().CalculateIdealDistance());
            SetOwnedByEnemy(Enemy.Template.Speed);
            HealthController().SetInitialHealth(Enemy.Template.Health, this);
            EnemyUi.ArmourController.SetCharacter(Enemy);
            RecoilManager.EnterCombat();
//            if (!(this is Medic || this is Martyr)) SetHealBehaviour();
            CurrentAction = Wander;
            EnemyUi.UiHitController.SetCharacter(this);
            EnemyUi.PrimaryButton.AddOnDeselectEvent(() => { EnemyUi.CanvasGroup.alpha = UiAppearanceController.FadedColour.a; });
            transform.SetParent(GameObject.Find("World").transform);
            SetDistance(2, 4);
            _originPosition = transform.position;
            UpdateDistance();
            SetConditions();
        }

        public void Reset()
        {
//            Alerted = false;
//            CurrentAction = Wander;
        }

        protected override void KnockDown()
        {
//            base.KnockDown();
//            CurrentAction = KnockedDown();
        }

        private void SetHealBehaviour()
        {
//            HealthController.AddOnTakeDamage(a =>
//            {
//                if (HealthController.GetNormalisedHealthValue() > 0.25f || _waitingForHeal) return;
//                foreach (DetailedEnemyCombat enemy in UIEnemyController.Enemies)
//                {
//                    Medic medic = enemy as Medic;
//                    if (medic == null || medic.HasTarget()) continue;
//                    CurrentAction = WaitForHeal(medic);
//                    break;
//                }
//            });
        }

        private Action WaitForHeal(Medic medic)
        {
            SetActionText("Waiting for Medic");
            medic.RequestHeal(this);
//            _waitingForHeal = true;
            return () =>
            {
//                if (!medic.IsDead) return;
//                _waitingForHeal = false;
                ChooseNextAction();
            };
        }

        public void ClearHealWait()
        {
            ChooseNextAction();
        }

        public void ReceiveHealing(int amount)
        {
//            HealthController.Heal(amount);
            ChooseNextAction();
        }

        private Action KnockedDown()
        {
//            float duration = KnockdownDuration;
            SetActionText("Knocked Down");
            return () =>
            {
//                duration -= Time.deltaTime;
//                if (duration > 0) return;
                ChooseNextAction();
//                IsKnockedDown = false;
            };
        }

        public virtual void ChooseNextAction()
        {
            Immobilised(false);
            CurrentAction = CheckForRepositioning();
            if (CurrentAction != null) return;
            CurrentAction = Aim();
        }

        private const float MeleeWarningTime = 2f;

        private bool TargetVisible()
        {
            _obstructed = PathingGrid.IsLineObstructed(transform.position, GetTarget().transform.position);
            return _obstructed;
        }

        private Action Melee()
        {
            float currentTime = MeleeWarningTime;
            SetActionText("Meleeing");
            return () =>
            {
                currentTime -= Time.deltaTime;
                if (currentTime > 0) return;
//                if (Math.Abs(DistanceToPlayer) > MeleeDistance)
//                {
//                    CurrentAction = Stagger();
//                    return;
//                }

                MeleeController.StartMelee(this);
            };
        }

        private Action WaitForRoute(Thread routingThread)
        {
            return () =>
            {
                if (routingThread.IsAlive) return;
                CurrentAction = MoveToTargetPosition(route);
            };
        }

        private void CheckForPlayer()
        {
            if (DistanceToPlayer > _visionRange) return;
            CurrentAction = Suspicious;
        }

        private void Suspicious()
        {
            SetActionText("Suspicious");
            if (DistanceToPlayer >= _detectionRange.CurrentValue()) return;
            if (DistanceToPlayer >= _visionRange.CurrentValue()) CurrentAction = Wander;
            Alert();
        }

        protected void SetActionText(string action)
        {
            EnemyUi.ActionText.text = action;
        }

        public override Weapon Weapon()
        {
            return Enemy.Weapon;
        }

        public void Alert()
        {
            if (Alerted) return;
            Alerted = true;
            UIEnemyController.AlertAll();
            ChooseNextAction();
        }

        private Action Flee()
        {
            SetActionText("Fleeing");
            return () =>
            {
//                MoveBackward();
                if (DistanceToPlayer > CombatManager.VisibilityRange) Kill();
            };
        }

        public override void Kill()
        {
            base.Kill();
            UIEnemyController.Remove(this);
            Destroy(EnemyUi.gameObject);
            Enemy.Kill();
            Destroy(gameObject);
        }

        public override void Update()
        {
            EnemyUi.UiHitController.UpdateValue();
            if (MeleeController.InMelee) return;
            _obstructed = false;
            base.Update();
            UpdateDistance();
            CurrentAction?.Invoke();
        }

        //Firing

        private Action Reload()
        {
            if (Weapon().GetRemainingMagazines() == 0)
            {
                return Flee();
            }

            SetActionText("Reloading");
            float duration = Weapon().GetAttributeValue(AttributeType.ReloadSpeed) * EnemyReloadMultiplier;
            return () =>
            {
                duration -= Time.deltaTime;
                if (duration > 0) return;
                Weapon().Reload(Enemy.Inventory());
                ChooseNextAction();
            };
        }

        private Action Aim()
        {
            if (Weapon() == null) return CheckForRepositioning();
            Immobilised(true);
            Assert.IsFalse(Weapon().Empty());
            float aimTime = Random.Range(MaxAimTime / 2f, MaxAimTime);
            SetActionText("Aiming");
            return () =>
            {
                if (Immobilised()) return;
                if (!TargetVisible() || OutOfRange()) ChooseNextAction();
                aimTime -= Time.deltaTime;
                if (aimTime >= 0) return;
                CurrentAction = Fire();
                aimTime = 0;
            };
        }

        private Action Fire()
        {
            bool automatic = Weapon().WeaponAttributes.Automatic;
            SetActionText("Firing");
            Immobilised(true);
            return () =>
            {
                if (!TargetVisible() || OutOfRange()) ChooseNextAction();
                List<Shot> shots = Weapon().Fire(this);
                if (shots == null || shots.Count == 0) return;
                shots.ForEach(s => s.Fire());
                if (shots.Any(s => s.DidHit)) CombatManager.Player.TryRetaliate(this);
                int remainingAmmo = Weapon().GetRemainingAmmo();
                if (!automatic)
                {
                    CurrentAction = Aim();
                }
                else if (remainingAmmo == 0)
                {
                    CurrentAction = Reload();
                }
            };
        }

        //Wander

        private void WaitThenWander()
        {
            float waitDuration = Random.Range(1f, 3f);
            CurrentAction = () =>
            {
                waitDuration -= Time.deltaTime;
                if (waitDuration > 0) return;
                Wander();
            };
        }

        private void Wander()
        {
            Cell currentCell = PathingGrid.PositionToCell(transform.position);
            int randomDistance = Random.Range(10, 20);
            Cell targetCell = PathingGrid.GetCellNearMe(currentCell, randomDistance);
            Thread routingThread = PathingGrid.RouteToCell(currentCell, targetCell, route);
            CurrentAction = WaitForRoute(routingThread);
            SetActionText("Wandering");
        }

        //Movement

        protected virtual void ReachTarget()
        {
            if (Alerted)
            {
                ChooseNextAction();
            }
            else
            {
                WaitThenWander();
            }
        }

        private bool _obstructed;

        private bool OutOfRange()
        {
            float cellDistanceToPlayer = PathingGrid.WorldToGridDistance(DistanceToPlayer);
            return cellDistanceToPlayer < IdealWeaponDistance * 0.5f || cellDistanceToPlayer > IdealWeaponDistance * 1.5f || _obstructed;
        }

        protected Action CheckForRepositioning()
        {
//            if (DistanceToPlayer <= MeleeDistance)
//            {
//                return Melee();
//            }

            if (OutOfRange())
            {
                Cell currentCell = PathingGrid.PositionToCell(transform.position);
                Thread pathThread = PathingGrid.RouteToCell(currentCell, PathingGrid.GetCellInRange(currentCell, (int) (IdealWeaponDistance * 1.25f), (int) (IdealWeaponDistance * 0.75f)), route);
                return WaitForRoute(pathThread);
            }

            return null;
        }

        private Action MoveToTargetPosition(List<Cell> newRoute)
        {
            route = newRoute;
            if (newRoute.Count == 0) return ChooseNextAction;

            Cell target = newRoute[0];
            newRoute.RemoveAt(0);
            return () =>
            {
                if (PathingGrid.PositionToCell(transform.position) == target)
                {
                    if (newRoute.Count == 0)
                    {
                        ReachTarget();
                        return;
                    }

                    target = newRoute[0];
                    newRoute.RemoveAt(0);
                }

                Vector3 direction = target.Position;
                direction = direction - transform.position;
                direction.Normalize();
                Move(direction);
            };
        }

        protected void MoveToPlayer()
        {
            if (DistanceToPlayer < 0.25f)
            {
                ReachPlayer();
            }

            Vector3 direction = CombatManager.Player.transform.position;
            direction = direction - transform.position;
            direction.Normalize();
            Move(direction);
        }

        protected virtual void ReachPlayer()
        {
        }
    }
}