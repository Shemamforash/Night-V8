using System;
using System.Collections.Generic;
using System.Threading;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Ui;
using Game.Gear.Weapons;
using NUnit.Framework;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies
{
    public class EnemyBehaviour : CharacterCombat
    {
        //        private readonly Cooldown _firingCooldown;
        //        private const float KnockdownDuration = 5f;

        public string ActionText;
        public Action CurrentAction;
        public Enemy Enemy;
        private SpriteRenderer _sprite;
        protected bool FacePlayer;

        protected readonly Queue<Cell> route = new Queue<Cell>();
        //        private const float AlphaCutoff = 0.2f;
//        private const float FadeVisibilityDistance = 5f;
//        private float _currentAlpha;

        public bool OnScreen() => Helper.IsObjectInCameraView(gameObject);

        public override Weapon Weapon() => null;

        public override void Update()
        {
            base.Update();
            if (!CombatManager.InCombat()) return;
            UpdateRotation();
            UpdateDistanceAlpha();
            if (CurrentAction == null)
            {
                ChooseNextAction();
            }
            else
            {
                CurrentAction.Invoke();
            }
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

        public override CharacterCombat GetTarget() => CombatManager.Player();

        public virtual void Initialise(Enemy enemy)
        {
            _sprite = GetComponent<SpriteRenderer>();
            ArmourController = enemy.ArmourController;
            Enemy = enemy;
            SetOwnedByEnemy(Enemy.Template.Speed);
            HealthController.SetInitialHealth(Enemy.Template.Health, this);
//            if (!(this is Medic || this is Martyr)) SetHealBehaviour();
            transform.SetParent(GameObject.Find("World").transform);
//            if (CombatManager.Region().GetRegionType() == RegionType.Nightmare)
//            {
            SetDistance(Vector2.zero, 4f, 8f);
//            }
//            else
//            {
//                SetDistance(CombatManager.Region().Fire.FirePosition, 0.2f, 0.5f);
//            }


            Sprite sprite = Resources.Load<Sprite>("Images/Enemy Symbols/" + GetEnemyName());
            if (sprite == null) return;
            GetComponent<SpriteRenderer>().sprite = sprite;
        }

        private void SetDistance(Vector3 pivot, float rangeMin, float rangeMax)
        {
            Vector3 position = pivot;
            float xOffset = Random.Range(rangeMin, rangeMax);
            float yOffset = Random.Range(rangeMin, rangeMax);
            if (Random.Range(0, 2) == 1) xOffset = -xOffset;
            if (Random.Range(0, 2) == 1) yOffset = -yOffset;
            position.x += xOffset;
            position.y += yOffset;
//            transform.position = position;
            Cell cell = PathingGrid.GetCellNearMe(CurrentCell(), 15);
            transform.position = cell.Position;
        }

        protected void SetActionText(string actionText)
        {
            ActionText = actionText;
            EnemyUi.Instance().UpdateActionText(this, actionText);
        }

        public override void TakeDamage(Shot shot)
        {
            base.TakeDamage(shot);
            EnemyUi.Instance().RegisterHit(this);
            CombatManager.Player().RageController.Increase(shot.DamageDealt());
        }

        public override void Kill()
        {
            base.Kill();
            for (int i = 0; i < Random.Range(2, 10); ++i) SaltBehaviour.Create(transform.position);

            ContainerController controller = ContainerController.CreateEnemyLoot(transform.position, Enemy);
            if (controller != null)
            {
                controller.CreateObject();
                CombatManager.Region().Containers.Add(controller);
            }

            CombatManager.Remove(this);
            Enemy.Kill();
            Destroy(gameObject);
        }

        public virtual void ChooseNextAction()
        {
            Immobilised(false);
        }

        protected void WaitForRoute(Thread routingThread, Action reachTargetAction = null)
        {
            CurrentAction = () =>
            {
                if (routingThread.IsAlive) return;
                if (reachTargetAction == null) reachTargetAction = ChooseNextAction;
                MoveToTargetPosition(reachTargetAction);
            };
        }

        //Firing

        public bool MoveToCover(Action reachCoverAction)
        {
            if (PathingGrid.IsCellHidden(CurrentCell())) return false;
            Cell safeCell = PathingGrid.FindCoverNearMe(CurrentCell());
            if (safeCell == null) return false;
            Immobilised(false);
            SetActionText("Seeking Cover");
            Thread safeRoute = PathingGrid.RouteToCell(CurrentCell(), safeCell, route);
            WaitForRoute(safeRoute, reachCoverAction);
            return true;
        }

        //Wander


        //Movement

        protected void Reposition(Cell c, Action reachTargetAction = null)
        {
            SetActionText("Moving");
            Thread pathThread = PathingGrid.RouteToCell(CurrentCell(), c, route);
            WaitForRoute(pathThread, reachTargetAction);
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
            Thread pathThread = PathingGrid.RouteToCell(CurrentCell(), character.CurrentCell(), route);
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

        public virtual string GetEnemyName() => Enemy.Name;
    }
}