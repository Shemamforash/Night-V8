﻿using System;
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
        private Queue<Cell> route = new Queue<Cell>();
        public Action CurrentAction;


        //        private readonly Cooldown _firingCooldown;
        //        private const float KnockdownDuration = 5f;
//        private bool _waitingForHeal;
        //        private const float AlphaCutoff = 0.2f;
//        private const float FadeVisibilityDistance = 5f;
//        private float _currentAlpha;

        public override void Update()
        {
            base.Update();
            _couldHitTarget = TargetVisible() && !OutOfRange();
            EnemyUi.UiHitController.UpdateValue();
            CurrentAction?.Invoke();
        }

        private void UpdateDistanceAlpha()
        {
//            float distanceToMaxVisibility = CombatManager.VisibilityRange + FadeVisibilityDistance - DistanceToTarget();
//            float alpha = 0;
//            if (DistanceToTarget() < CombatManager.VisibilityRange)
//            {
//                float normalisedDistance = Helper.Normalise(DistanceToTarget(), CombatManager.VisibilityRange);
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

        public void OnDrawGizmos()
        {
            if (route.Count == 0) return;
            Cell[] routeArr = route.ToArray();
            for (int i = 1; i < routeArr.Length; ++i)
            {
                Gizmos.DrawLine(routeArr[i - 1].Position, routeArr[i].Position);
            }

//            Gizmos.color = new Color(0, 1, 0, 0.2f);
//            Gizmos.DrawSphere(transform.position, (int)(IdealWeaponDistance * 1.5f / PathingGrid.CellResolution));
//            Gizmos.color = new Color(0, 1, 0, 0.3f);
//            Gizmos.DrawSphere(transform.position, (int)(IdealWeaponDistance * 0.5f / PathingGrid.CellResolution));
        }


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
            SetConditions();
        }

        public void SetDistance(int rangeMin, int rangeMax)
        {
            Vector3 position = new Vector3();
            position.x = Random.Range(rangeMin, rangeMax);
            if (Random.Range(0, 2) == 1) position.x = -position.x;
            position.y = Random.Range(rangeMin, rangeMax);
            if (Random.Range(0, 2) == 1) position.y = -position.y;
            transform.position = position;
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

        private void WaitForHeal(Medic medic)
        {
            SetActionText("Waiting for Medic");
            medic.RequestHeal(this);
//            _waitingForHeal = true;
            ChooseNextAction();
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

        private void KnockedDown()
        {
//            float duration = KnockdownDuration;
            SetActionText("Knocked Down");
            ChooseNextAction();
        }

        private const float MeleeWarningTime = 2f;

        private Action Melee()
        {
            float currentTime = MeleeWarningTime;
            SetActionText("Meleeing");
            return () =>
            {
                currentTime -= Time.deltaTime;
                if (currentTime > 0) return;
//                if (Math.Abs(DistanceToTarget()) > MeleeDistance)
//                {
//                    CurrentAction = Stagger();
//                    return;
//                }
            };
        }

        private void CheckForPlayer()
        {
            if (DistanceToTarget() > _visionRange) return;
            CurrentAction = Suspicious;
        }

        private void Suspicious()
        {
            SetActionText("Suspicious");
            if (DistanceToTarget() >= _detectionRange.CurrentValue()) return;
            if (DistanceToTarget() >= _visionRange.CurrentValue()) CurrentAction = Wander;
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

        private void Flee()
        {
            SetActionText("Fleeing");
            CurrentAction = () =>
            {
//                MoveBackward();
                if (DistanceToTarget() > CombatManager.VisibilityRange) Kill();
            };
        }

        public override void TakeDamage(Shot shot)
        {
            base.TakeDamage(shot);
            if (shot.IsCritical) HitController().RegisterCritical();
            Alert();
            CombatManager.Player.RageController.Increase(shot.DamageDealt());
        }
        
        public override void Kill()
        {
            base.Kill();
            UIEnemyController.Remove(this);
            Destroy(EnemyUi.gameObject);
            Enemy.Kill();
            Destroy(gameObject);
        }

        private bool TargetVisible()
        {
            bool _obstructed = PathingGrid.IsLineObstructed(transform.position, GetTarget().transform.position);
//            if (_obstructed) Debug.DrawLine(transform.position, GetTarget().transform.position, Color.yellow, 2f);
            return !_obstructed;
        }

        public virtual void ChooseNextAction()
        {
            Immobilised(false);
            if (NeedsRepositioning()) return;
            Aim();
        }

        private void WaitForRoute(Thread routingThread, Action reachTargetAction = null)
        {
            CurrentAction = () =>
            {
                if (routingThread.IsAlive) return;
                if (reachTargetAction == null) reachTargetAction = ChooseNextAction;
                MoveToTargetPosition(reachTargetAction);
            };
        }

        protected virtual void OnAlert()
        {
            if (NeedsRepositioning()) return;
            if (Weapon() == null)
            {
                MoveToPlayer();
                return;
            }

            Aim();
        }

        public void Alert()
        {
            if (Alerted) return;
            Alerted = true;
            UIEnemyController.AlertAll();
            OnAlert();
        }

        //Firing

        private bool NeedToMoveToCover(Action reachCoverAction)
        {
            if (PathingGrid.IsCellHidden(CurrentCell())) return false;
            Cell safeCell = PathingGrid.FindCoverNearMe(CurrentCell());
            if (safeCell == null) return false;
            Immobilised(false);
            SetActionText("Seeking Cover");
            StartSprinting();
            Thread safeRoute = PathingGrid.RouteToCell(CurrentCell(), safeCell, route);
            WaitForRoute(safeRoute, reachCoverAction);
            return true;
        }

        private void Reload()
        {
            if (NeedToMoveToCover(Reload)) return;
            if (Weapon().GetRemainingMagazines() == 0)
            {
                Flee();
                return;
            }

            SetActionText("Reloading");
            float duration = Weapon().GetAttributeValue(AttributeType.ReloadSpeed) * EnemyReloadMultiplier;
            CurrentAction = () =>
            {
                duration -= Time.deltaTime;
                if (duration > 0) return;
                Weapon().Reload(Enemy.Inventory());
                ChooseNextAction();
            };
        }

        private void Aim()
        {
            Immobilised(true);
            Assert.IsNotNull(Weapon());
            Assert.IsFalse(Weapon().Empty());
            SetActionText("Aiming");
            CurrentAction = () =>
            {
                if (!_couldHitTarget) ChooseNextAction();
                if (RecoilManager.GetAccuracyModifier() < 0.75f) return;
                Fire();
            };
        }

        private bool _couldHitTarget;

        private void Fire()
        {
            bool automatic = Weapon().WeaponAttributes.Automatic;
            SetActionText("Firing");
            Immobilised(true);
            CurrentAction = () =>
            {
                if (!_couldHitTarget)
                {
                    ChooseNextAction();
                    return;
                }

                List<Shot> shots = Weapon().Fire(this, true);
                if (shots.Any(s => s.DidHit)) CombatManager.Player.TryRetaliate(this);
                if (Weapon().Empty()) Reload();
                else if (!automatic)
                    Aim();
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
            int randomDistance = Random.Range(10, 20);
            Cell targetCell = PathingGrid.GetCellNearMe(CurrentCell(), randomDistance);
            Thread routingThread = PathingGrid.RouteToCell(CurrentCell(), targetCell, route);
            WaitForRoute(routingThread, WaitThenWander);
            SetActionText("Wandering");
        }

        //Movement

        protected virtual void ReachTarget()
        {
            if (Alerted)
            {
                ChooseNextAction();
            }
        }

        private bool OutOfRange()
        {
            return DistanceToTarget() < IdealWeaponDistance * 0.5f || DistanceToTarget() > IdealWeaponDistance * 1.5f;
        }

        private bool NeedsRepositioning()
        {
//            if (DistanceToTarget() <= MeleeDistance)
//            {
//                return Melee();
//            }

            if (_couldHitTarget || Weapon() == null) return false;
            SetActionText("Moving");
            Cell targetCell = PathingGrid.FindCellToAttackPlayer(CurrentCell(), (int) (IdealWeaponDistance * 1.25f), (int) (IdealWeaponDistance * 0.75f));
            Thread pathThread = PathingGrid.RouteToCell(CurrentCell(), targetCell, route);
            WaitForRoute(pathThread);
            return true;
        }

        private void MoveToTargetPosition(Action ReachTargetAction)
        {
            Queue<Cell> newRoute = new Queue<Cell>(route);
            Assert.IsTrue(newRoute.Count > 0);
            Cell target = newRoute.Dequeue();
            CurrentAction = () =>
            {
                if (CurrentCell() == target)
                {
                    StopSprinting();
                    if (newRoute.Count == 0)
                    {
                        ReachTargetAction();
                        return;
                    }

                    target = newRoute.Dequeue();
                }

                Vector3 direction = ((Vector3) target.Position - transform.position).normalized;
                Move(direction);
            };
        }

        protected void MoveToPlayer()
        {
            CurrentAction = () =>
            {
                if (DistanceToTarget() < 0.25f)
                {
                    ReachPlayer();
                    return;
                }

                Vector3 direction = CombatManager.Player.transform.position;
                direction = direction - transform.position;
                direction.Normalize();
                Move(direction);
            };
        }

        protected virtual void ReachPlayer()
        {
        }
    }
}