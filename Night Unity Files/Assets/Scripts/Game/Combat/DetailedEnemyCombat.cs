using System;
using System.Collections.Generic;
using System.Linq;
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
    public class DetailedEnemyCombat : CharacterCombat
    {
        protected bool Alerted;
        private readonly CharacterAttribute _visionRange = new CharacterAttribute(AttributeType.Vision, 30f);
        private readonly CharacterAttribute _detectionRange = new CharacterAttribute(AttributeType.Detection, 15f);

        private const int EnemyReloadMultiplier = 4;

        public Action CurrentAction;

        private Vector2 _originPosition;

//        private readonly Cooldown _firingCooldown;
        private const float MaxAimTime = 2f;
        protected int MinRepositionDistance = 4;
//        private int _wanderDirection = -1;


//        private float _currentFadeInTime = 2f;
//        private const float MaxFadeInTime = 2f;
//        private string _actionString;
        public Enemy Enemy;

//        private const float KnockdownDuration = 5f;
//        private bool _waitingForHeal;

        public float DistanceToPlayer;
//        private const float AlphaCutoff = 0.2f;

//        private const float FadeVisibilityDistance = 5f;
//        private float _currentAlpha;
        private EnemyUi _enemyUi;

        public override CharacterCombat GetTarget()
        {
            return CombatManager.Player;
        }

        public override UIArmourController ArmourController()
        {
            return _enemyUi.ArmourController;
        }

        private void UpdateDistance()
        {
            DistanceToPlayer = Vector2.Distance(CharacterController.Position(), CombatManager.Player.CharacterController.Position());
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

        private void OnDrawGizmos()
        {
//            Gizmos.color = Color.red;
//            Gizmos.DrawCube(PathingGrid.PositionToCell(transform.position).Position, new Vector3(0.5f, 0.5f, 0.5f));
//            Gizmos.color = Color.green;
//            Gizmos.DrawCube(transform.position, new Vector3(0.5f, 0.5f, 0.5f));
            if (route.Count == 0) return;
            for (int i = 1; i < route.Count; ++i)
            {
                Gizmos.DrawLine(route[i - 1].Position, route[i].Position);
            }
        }

        private List<PathingGrid.Cell> route = new List<PathingGrid.Cell>();

        private Action MoveToTargetPosition(List<PathingGrid.Cell> newRoute)
        {
            route = newRoute;
            PathingGrid.Cell target = newRoute[0];
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
                CharacterController.GetComponent<Rigidbody2D>().AddForce(direction * 2);
            };
        }

        public override UIHealthBarController HealthController()
        {
            return _enemyUi.HealthController;
        }

        public UIHitController HitController()
        {
            return _enemyUi.UiHitController;
        }

        public void SetSelected()
        {
            _enemyUi.PrimaryButton.Button().Select();
        }

        public virtual void Initialise(Enemy enemy, EnemyUi enemyUi)
        {
            _enemyUi = enemyUi;
            _enemyUi.NameText.text = enemy.Name;
            Enemy = enemy;
            SetOwnedByEnemy(Enemy.Template.Speed);
            HealthController().SetInitialHealth(Enemy.Template.Health, this);
            _enemyUi.ArmourController.SetCharacter(Enemy);
            RecoilManager.EnterCombat();
//            if (!(this is Medic || this is Martyr)) SetHealBehaviour();
            CurrentAction = Wander;
            _enemyUi.UiHitController.SetCharacter(this);
            _enemyUi.PrimaryButton.AddOnSelectEvent(() =>
            {
                CombatManager.Player.SetTarget(this);
                _enemyUi.CanvasGroup.alpha = 1;
            });
            _enemyUi.PrimaryButton.AddOnDeselectEvent(() => { _enemyUi.CanvasGroup.alpha = UiAppearanceController.FadedColour.a; });
            _enemyUi.HealthController.AddOnTakeDamage(f => { Alert(); });
            CharacterController = GetComponent<CombatCharacterController>();
            CharacterController.transform.SetParent(GameObject.Find("World").transform);
            CharacterController.SetOwner(this);
            CharacterController.SetDistance(2, 4);
            _originPosition = CharacterController.Position();
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
            CurrentAction = CheckForRepositioning();
            if (CurrentAction != null) return;
            CurrentAction = Aim();
        }

        private const float MeleeWarningTime = 2f;
        private const float StaggerTime = 2f;

        protected Action CheckForRepositioning(bool moveAnyway = false)
        {
            if (!InCombat()) return null;
//            if (DistanceToPlayer <= MeleeDistance)
//            {
//                return Melee();
//            }

            
            if (DistanceToPlayer < MinRepositionDistance || DistanceToPlayer > 3 || moveAnyway)
            {
                PathingGrid.Cell currentCell = PathingGrid.PositionToCell(transform.position);
                List<PathingGrid.Cell> path = PathingGrid.RouteToCell(currentCell, PathingGrid.GetCellInRange(currentCell, 10, MinRepositionDistance));
                return MoveToTargetPosition(path);
            }

            return null;
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

        private Action Stagger()
        {
            float currentTime = StaggerTime;
            SetActionText("Staggered");
            return () =>
            {
                currentTime -= Time.deltaTime;
                if (currentTime > 0) return;
                ChooseNextAction();
            };
        }

        public bool _aheadOfPlayer;

        public void MoveToPlayer()
        {
//            float playerPosition = CombatManager.Player.Position.CurrentValue();
//            if (playerPosition < Position.CurrentValue())
//            {
//                MoveForward();
//                if (playerPosition >= Position.CurrentValue())
//                {
//                    Position.SetCurrentValue(playerPosition);
//                    ReachTarget();
//                }
//            }
//            else
//            {
//                MoveBackward();
//                if (playerPosition <= Position.CurrentValue())
//                {
//                    Position.SetCurrentValue(playerPosition);
//                    ReachTarget();
//                }
//            }
        }
//
//        public Action MoveToTargetDistance(float distance)
//        {
//            Assert.IsTrue(distance >= 0);
//            Action moveForwardAction = () =>
//            {
//                MoveForward();
//                if (DistanceToPlayer > distance) return;
//                ReachTarget();
//            };
//            Action moveBackwardAction = () =>
//            {
//                MoveBackward();
//                if (DistanceToPlayer < distance) return;
//                ReachTarget();
//            };
//
//            if (_aheadOfPlayer)
//            {
//                SetActionText(DistanceToPlayer > distance ? "Approaching" : "Retreating");
//                return DistanceToPlayer > distance ? moveForwardAction : moveBackwardAction;
//            }
//
//            SetActionText(DistanceToPlayer > distance ? "Retreating" : "Approaching");
//            return DistanceToPlayer > distance ? moveBackwardAction : moveForwardAction;
//        }

        private void Wander()
        {
            if (transform.position == Vector3.zero) return;
            PathingGrid.Cell currentCell = PathingGrid.PositionToCell(transform.position);
            int randomDistance = Random.Range(20, 30);
            PathingGrid.Cell targetCell = PathingGrid.GetCellNearMe(currentCell, randomDistance);
            List<PathingGrid.Cell> route = PathingGrid.RouteToCell(currentCell, targetCell);
            CurrentAction = MoveToTargetPosition(route);
            SetActionText("Wandering");
            CheckForPlayer();
        }

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

        private float CalculateIdealRange()
        {
            if (Weapon() == null) return 0;
            float idealRange = Weapon().GetAttributeValue(AttributeType.Range);
            idealRange = Random.Range(0.8f * idealRange, idealRange);
            if (idealRange >= CombatManager.VisibilityRange)
            {
                idealRange = Random.Range(0.8f * CombatManager.VisibilityRange, CombatManager.VisibilityRange);
            }

            return idealRange;
        }

        private void CheckForPlayer()
        {
//            if (DistanceToPlayer > _visionRange) return;
//            CurrentAction = Suspicious;
        }

        private void Suspicious()
        {
            SetActionText("Suspicious");
//            if (DistanceToPlayer >= _detectionRange.CurrentValue()) return;
//            if (DistanceToPlayer >= _visionRange.CurrentValue()) CurrentAction = Wander;
            Alert();
        }

        public bool InCombat()
        {
//            return !IsDead && Alerted;
            return true;
        }

        protected void SetActionText(string action)
        {
//            _actionString = action;
//            if (!DistanceController.InSight) action = "";
//            ActionText.text = action;
        }

        public override void OnMiss()
        {
//            if (!Alerted) Alert();
        }

        public override Weapon Weapon()
        {
            return Enemy.Weapon;
        }

        public virtual void Alert()
        {
            if (Alerted) return;
            Alerted = true;
            Debug.Log("alerted");
            UIEnemyController.AlertAll();
            ChooseNextAction();
        }

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

        private Action Flee()
        {
            SetActionText("Fleeing");
            return () =>
            {
//                MoveBackward();
                if (DistanceToPlayer > CombatManager.VisibilityRange) Kill();
            };
        }

        private Action Fire()
        {
            int divider = Random.Range(3, 6);
            int noShots = (int) (Weapon().GetAttributeValue(AttributeType.Capacity) / divider);
            bool automatic = Weapon().WeaponAttributes.Automatic;
            if (automatic) noShots = 1;
            SetActionText("Firing");
            if (!automatic)
            {
                noShots = 1;
            }

            return () =>
            {
                List<Shot> shots = Weapon().Fire(this);
                if (shots == null || shots.Count == 0) return;
                shots.ForEach(s => s.Fire());
                if (shots.Any(s => s.DidHit)) CombatManager.Player.TryRetaliate(this);
                --noShots;
                int remainingAmmo = Weapon().GetRemainingAmmo();
                if (noShots == 0 || remainingAmmo == 0)
                {
                    UpdateAim(0);
                }

                if (remainingAmmo == 0)
                {
                    CurrentAction = Reload();
                }
                else if (noShots == 0)
                {
                    ChooseNextAction();
                }
            };
        }

        public override void Kill()
        {
            base.Kill();
            UIEnemyController.Remove(this);
            Destroy(_enemyUi.gameObject);
            Enemy.Kill();
            Destroy(gameObject);
        }

        public override void Update()
        {
            if (MeleeController.InMelee) return;
            base.Update();
            UpdateDistance();
            CurrentAction?.Invoke();
        }

        private void UpdateAim(float value)
        {
//            UiAimController.SetValue(value);
        }

        protected virtual Action Aim()
        {
            if(Weapon() == null) return CheckForRepositioning();
            if (Weapon().Empty())
            {
                return Reload();
            }

            Assert.IsFalse(Weapon().Empty());
            float aimTime = Random.Range(MaxAimTime / 2f, MaxAimTime);
            SetActionText("Aiming");
            return () =>
            {
                if (Immobilised()) return;
                float normalisedTime = Helper.Normalise(aimTime, MaxAimTime);
                aimTime -= Time.deltaTime;
                if (aimTime < 0)
                {
                    CurrentAction = Fire();
                    aimTime = 0;
                }

                UpdateAim(1 - normalisedTime);
            };
        }
    }
}