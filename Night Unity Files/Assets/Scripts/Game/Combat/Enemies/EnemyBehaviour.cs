using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Game.Characters;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Combat.Ui;
using Game.Exploration.Regions;
using Game.Gear.Weapons;
using JetBrains.Annotations;
using NUnit.Framework;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies
{
    public class
        EnemyBehaviour : CharacterCombat
    {
        public string ActionText;
        public Action CurrentAction;
        public Enemy Enemy;
        protected bool FacePlayer;

        protected readonly Queue<Cell> route = new Queue<Cell>();

        public bool OnScreen() => Helper.IsObjectInCameraView(gameObject);

        public override Weapon Weapon() => null;

        public override void Update()
        {
            base.Update();
            PushAwayFromNeighbors();
            UpdateAlpha();
            if (!CombatManager.InCombat()) return;
            UpdateRotation();
            if (CurrentAction == null)
            {
                ChooseNextAction();
            }
            else
            {
                CurrentAction.Invoke();
            }
        }

        protected SpriteRenderer Sprite;

        private void UpdateAlpha()
        {
            float distanceToPlayer = Vector2.Distance(transform.position, PlayerCombat.Instance.transform.position);
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
            AddForce(forceDir);
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

        public void OnDrawGizmos()
        {
            if (route.Count == 0) return;
            Cell[] routeArr = route.ToArray();
            for (int i = 1; i < routeArr.Length; ++i) Gizmos.DrawLine(routeArr[i - 1].Position, routeArr[i].Position);
        }

        public override CharacterCombat GetTarget() => PlayerCombat.Instance;

        public virtual void Initialise(Enemy enemy)
        {
            ArmourController = enemy.ArmourController;
            Enemy = enemy;
            SetOwnedByEnemy(Enemy.Template.Speed);
            HealthController.SetInitialHealth(Enemy.Template.Health, this);
            transform.SetParent(GameObject.Find("World").transform);
            transform.position = PathingGrid.GetCellNearMe(Vector2.zero, 8f, 4f).Position;
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

        public override void TakeDamage(Shot shot)
        {
            base.TakeDamage(shot);
            EnemyUi.Instance().RegisterHit(this);
        }

        public override void Kill()
        {
            base.Kill();

            Loot loot = Enemy.DropLoot(transform.position);

            switch (Enemy.Template.DropResource)
            {
                case "Salt":
                    SaltBehaviour.Create(transform.position, Enemy.Template.DropCount);
                    break;
                case "Essence":
                    EssenceCloudBehaviour.Create(transform.position, Enemy.Template.DropCount);
                    break;
                case "Meat":
                    loot.IncrementResource(ResourceTemplate.GetMeat().Name, Random.Range(0, 3));
                    break;
            }

            if (loot.IsValid)
            {
                loot.CreateObject(true);
                CombatManager.Region().Containers.Add(loot);
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

        protected void Reposition(Cell c, Action reachTargetAction = null)
        {
            SetActionText("Moving");
            Debug.DrawLine(CurrentCell().Position, c.Position, Color.red, 5f);
            Thread pathThread = PathingGrid.RouteToCell(CurrentCell(), c, route);
            WaitForRoute(pathThread, reachTargetAction);
        }

        private void MoveToCell(Cell target)
        {
            Vector3 direction = ((Vector3) target.Position - transform.position).normalized;
            Move(direction);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (_targetCell == null) return;
            _reachedTarget = CurrentCell() == _targetCell;
        }

        private Cell _targetCell;
        private bool _reachedTarget;

        private void MoveToTargetPosition(Action ReachTargetAction)
        {
            Queue<Cell> newRoute = new Queue<Cell>(route);
            Assert.IsTrue(newRoute.Count > 0);
            _targetCell = newRoute.Dequeue();
            CurrentAction = () =>
            {
                if (_reachedTarget)
                {
                    if (newRoute.Count == 0)
                    {
                        _targetCell = null;
                        ReachTargetAction();
                        return;
                    }

                    _targetCell = newRoute.Dequeue();
                }

                MoveToCell(_targetCell);
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
                Cell currentCharacterCell = character.CurrentCell();
                if (currentCharacterCell != targetCell)
                {
                    pathThread = PathingGrid.RouteToCell(CurrentCell(), currentCharacterCell, route);
                    targetCell = currentCharacterCell;
                }

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