using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Game.Combat.Enemies.Humans;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Ui;
using Game.Gear.Weapons;
using NUnit.Framework;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies
{
    public class EnemyBehaviour : CharacterCombat
    {
        private const int EnemyReloadMultiplier = 4;
        private const float MaxAimTime = 2f;

        private const float MeleeWarningTime = 2f;
        private readonly CharacterAttribute _detectionRange = new CharacterAttribute(AttributeType.Detection, 2f);
        private readonly CharacterAttribute _visionRange = new CharacterAttribute(AttributeType.Vision, 5f);

        private Vector2 _originPosition;


        //        private readonly Cooldown _firingCooldown;
        //        private const float KnockdownDuration = 5f;
        private bool _waitingForHeal;

        public string ActionText;

        protected bool Alerted;

        protected bool CouldHitTarget;
        public Action CurrentAction;
        public Enemy Enemy;
        private int IdealWeaponDistance;
        private SpriteRenderer _sprite;

        private readonly Queue<Cell> route = new Queue<Cell>();
        //        private const float AlphaCutoff = 0.2f;
//        private const float FadeVisibilityDistance = 5f;
//        private float _currentAlpha;

        public bool OnScreen()
        {
            return Helper.IsObjectInCameraView(gameObject);
        }

        public override void Update()
        {
            if (!CombatManager.InCombat()) return;
            base.Update();
            CouldHitTarget = TargetVisible() && !OutOfRange();
            CurrentAction?.Invoke();
            UpdateDistanceAlpha();
        }

        private void UpdateDistanceAlpha()
        {
            float distanceToPlayer = DistanceToTarget();
            float alpha = 0;
            if (distanceToPlayer <= CombatManager.VisibilityRange())
            {
                alpha = distanceToPlayer / CombatManager.VisibilityRange();
                alpha = 1 - alpha;
            } 
            Color spriteColour = _sprite.color;
            spriteColour.a = alpha;
            _sprite.color = spriteColour;
        }

        public void OnDrawGizmos()
        {
            if (route.Count == 0) return;
            Cell[] routeArr = route.ToArray();
            for (int i = 1; i < routeArr.Length; ++i) Gizmos.DrawLine(routeArr[i - 1].Position, routeArr[i].Position);

//            Gizmos.color = new Color(0, 1, 0, 0.2f);
//            Gizmos.DrawSphere(transform.position, (int)(IdealWeaponDistance * 1.5f / PathingGrid.CellResolution));
//            Gizmos.color = new Color(0, 1, 0, 0.3f);
//            Gizmos.DrawSphere(transform.position, (int)(IdealWeaponDistance * 0.5f / PathingGrid.CellResolution));
        }


        public override CharacterCombat GetTarget()
        {
            return CombatManager.Player();
        }

        private PathingGrid _grid;
        
        public virtual void Initialise(Enemy enemy)
        {
            _sprite = GetComponent<SpriteRenderer>();
            _grid = PathingGrid.Instance();
            ArmourController = enemy.ArmourController;
            Enemy = enemy;
            if (Weapon() != null) IdealWeaponDistance = _grid.WorldToGridDistance(Weapon().CalculateIdealDistance());
            SetOwnedByEnemy(Enemy.Template.Speed);
            HealthController.SetInitialHealth(Enemy.Template.Health, this);
//            if (!(this is Medic || this is Martyr)) SetHealBehaviour();
            if (Random.Range(0, 3) == 1) SetActionText("Resting");
            else CurrentAction = Wander;
            transform.SetParent(GameObject.Find("World").transform);
            SetDistance(0.2f, 0.5f);
            _originPosition = transform.position;
            SetHealBehaviour();
        }

        private void SetDistance(float rangeMin, float rangeMax)
        {
            Vector3 position = CombatManager.Region().Fire.FirePosition;
            float xOffset = Random.Range(rangeMin, rangeMax);
            float yOffset = Random.Range(rangeMin, rangeMax);
            if (Random.Range(0, 2) == 1) xOffset = -xOffset;
            if (Random.Range(0, 2) == 1) yOffset = -yOffset;
            position.x += xOffset;
            position.y += yOffset;
            transform.position = position;
        }

        private static Medic FindMedic()
        {
            foreach (EnemyBehaviour enemy in CombatManager.EnemiesOnScreen())
            {
                Medic medic = enemy as Medic;
                if (medic == null || medic.HasTarget()) continue;
                return medic;
            }

            return null;
        }

        private void SetHealBehaviour()
        {
            HealthController.AddOnTakeDamage(a =>
            {
                if (HealthController.GetNormalisedHealthValue() > 0.25f || _waitingForHeal) return;
                Medic m = FindMedic();
                if (m == null) return;
                MoveToCover(() => WaitForHeal(m));
            });
        }

        private void WaitForHeal(Medic medic)
        {
            CurrentAction = () =>
            {
                if (!_waitingForHeal)
                {
                    SetActionText("Waiting for Medic");
                    medic.RequestHeal(this);
                    _waitingForHeal = true;
                }

                if (medic != null) return;
                medic = FindMedic();
                if (medic == null) ChooseNextAction();
                else WaitForHeal(medic);
            };
        }

        private void KnockedDown()
        {
//            float duration = KnockdownDuration;
            SetActionText("Knocked Down");
            ChooseNextAction();
        }

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

        protected void SetActionText(string actionText)
        {
            ActionText = actionText;
            EnemyUi.Instance().UpdateActionText(this, actionText);
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
                if (DistanceToTarget() > CombatManager.VisibilityRange()) Kill();
            };
        }

        public override void TakeDamage(Shot shot)
        {
            base.TakeDamage(shot);
            EnemyUi.Instance().RegisterHit(this);
            Alert();
            CombatManager.Player().RageController.Increase(shot.DamageDealt());
        }

        public override void Kill()
        {
            base.Kill();
            ContainerController controller = new ContainerController(transform.position, Enemy.Inventory());
            controller.CreateObject();
            CombatManager.Remove(this);
            Enemy.Kill();
            Destroy(gameObject);
        }

        private bool TargetVisible()
        {
            bool _obstructed = _grid.IsLineObstructed(transform.position, GetTarget().transform.position);
//            if (_obstructed) Debug.DrawLine(transform.position, GetTarget().transform.position, Color.yellow, 2f);
            return !_obstructed;
        }

        public virtual void ChooseNextAction()
        {
            Immobilised(false);
            if (NeedsRepositioning()) return;
            if (Weapon().Empty()) Reload();
            else Aim();
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
            CombatManager.AlertAll();
            OnAlert();
        }

        //Firing

        private bool MoveToCover(Action reachCoverAction)
        {
            if (_grid.IsCellHidden(CurrentCell())) return false;
            Cell safeCell = _grid.FindCoverNearMe(CurrentCell());
            if (safeCell == null) return false;
            Immobilised(false);
            SetActionText("Seeking Cover");
            StartSprinting();
            Thread safeRoute = _grid.RouteToCell(CurrentCell(), safeCell, route);
            WaitForRoute(safeRoute, reachCoverAction);
            return true;
        }

        private void Reload()
        {
            if (MoveToCover(Reload)) return;
//                Flee();
            SetActionText("Reloading");
            float duration = Weapon().GetAttributeValue(AttributeType.ReloadSpeed) * EnemyReloadMultiplier;
            CurrentAction = () =>
            {
                duration -= Time.deltaTime;
                if (duration > 0) return;
                Weapon().Reload();
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
                if (!CouldHitTarget) ChooseNextAction();
                if (GetAccuracyModifier() < 0.75f) return;
                Fire();
            };
        }

        private void Fire()
        {
            bool automatic = Weapon().WeaponAttributes.Automatic;
            SetActionText("Firing");
            Immobilised(true);
            CurrentAction = () =>
            {
                if (!CouldHitTarget)
                {
                    ChooseNextAction();
                    return;
                }

                List<Shot> shots = Weapon().Fire(this, true);
                if (shots.Any(s => s.DidHit)) CombatManager.Player().TryRetaliate(this);
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
            float randomDistance = Random.Range(0.5f, 1.5f);
            float currentAngle = AdvancedMaths.AngleFromUp(_originPosition, transform.position);
            float randomAngle = currentAngle + Random.Range(20f, 60f);
            randomAngle *= Mathf.Deg2Rad;
            Vector2 randomPoint = new Vector2();
            randomPoint.x = randomDistance * Mathf.Cos(randomAngle) + _originPosition.x;
            randomPoint.y = randomDistance * Mathf.Sin(randomAngle) + _originPosition.y;
            Cell targetCell = _grid.PositionToCell(randomPoint);
//            Cell targetCell = PathingGrid.GetCellNearMe(CurrentCell(), randomDistance);
            Thread routingThread = _grid.RouteToCell(CurrentCell(), targetCell, route);
            WaitForRoute(routingThread, WaitThenWander);
            SetActionText("Wandering");
        }

        //Movement

        protected virtual void ReachTarget()
        {
            if (Alerted) ChooseNextAction();
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

            if (CouldHitTarget || Weapon() == null) return false;
            SetActionText("Moving");
            Cell targetCell = _grid.FindCellToAttackPlayer(CurrentCell(), (int) (IdealWeaponDistance * 1.25f), (int) (IdealWeaponDistance * 0.75f));
            Thread pathThread = _grid.RouteToCell(CurrentCell(), targetCell, route);
            WaitForRoute(pathThread);
            return true;
        }

        private void MoveToCell(Cell target)
        {
            Vector3 direction = ((Vector3) target.Position - transform.position).normalized;
            Move(direction);
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

                MoveToCell(target);
            };
        }

        protected void MoveToPlayer()
        {
            MoveToCharacter(GetTarget(), ReachPlayer);
        }

        protected void MoveToCharacter(CharacterCombat character, Action reachCharacterAction)
        {
            Cell targetCell = null;
            SetActionText("Moving to " + character.name);
            Thread pathThread = _grid.RouteToCell(CurrentCell(), character.CurrentCell(), route);
            CurrentAction = () =>
            {
                if (pathThread.IsAlive) return;
                if (character.IsDead)
                {
                    ChooseNextAction();
                    return;
                }

                if (CurrentCell() == targetCell || targetCell == null) targetCell = route.Count == 0 ? null : route.Dequeue();

                if (targetCell != null) MoveToCell(targetCell);

                if (Vector2.Distance(character.transform.position, transform.position) > 0.25f) return;
                reachCharacterAction();
            };
        }

        protected virtual void ReachPlayer()
        {
        }
    }
}