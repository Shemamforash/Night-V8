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

        public bool OnScreen() => Helper.IsObjectInCameraView(gameObject);

        public override Weapon Weapon() => null;

        public override void Update()
        {
            base.Update();
            PushAwayFromNeighbors();
            UpdateAlpha();
            CheckForRequiredPathfind();
            if (!CombatManager.InCombat()) return;
            UpdateRotation();
            CurrentAction?.Invoke();
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
            CombatManager.IncreaseDamageDealt(shot.DamageDealt());
            EnemyUi.Instance().RegisterHit(this);
        }

        public override void Kill()
        {
            base.Kill();
            if (PlayerCombat.Instance.Player.Attributes.SpreadSickness && IsSick())
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

            PlayerCombat.Instance.Player.IncreaseWeaponKills();
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

        public bool MoveToCover(Action reachCoverAction)
        {
            if (PathingGrid.IsCellHidden(CurrentCell())) return false;
            Cell safeCell = PathingGrid.FindCoverNearMe(CurrentCell());
            if (safeCell == null) return false;
            SetActionText("Seeking Cover");
            GoToCell(safeCell, reachCoverAction);
            return true;
        }

        private void MoveToNextCell()
        {
            Vector3 direction = ((Vector3) _nextCell.Position - transform.position).normalized;
            Move(direction);
        }

        private Cell _nextCell;
        
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (_nextCell == null) return;
            _reachedTarget = CurrentCell() == _nextCell;
        }

        private Cell _destinationCell;
        private float _targetDistance;
        private bool _reachedTarget;
        private float _currentTime;
        private Action _reachTargetAction;

        private void Reposition(Action reachTargetAction)
        {
            SetActionText("Moving");
            Queue<Cell> newRoute = new Queue<Cell>(_route);
            if (newRoute.Count == 0)
            {
                Debug.Log(name + " has no route");
                return;
            }

            Debug.DrawLine(CurrentCell().Position, _route[_route.Count - 1].Position, Color.red, 5f);
            _nextCell = newRoute.Dequeue();
            CurrentAction = () =>
            {
                if (_reachedTarget)
                {
                    if (newRoute.Count == 0)
                    {
                        _destinationCell = null;
                        reachTargetAction();
                        return;
                    }

                    _nextCell = newRoute.Dequeue();
                }

                MoveToNextCell();
            };
        }

        protected virtual void ReachTarget()
        {
        }

        public virtual string GetEnemyName() => Enemy.Name;

        //pathfind

        private List<Cell> _route = new List<Cell>();
        private float _lastRouteStartTime;
        private Thread _routeThread;

        public void SetRoute(List<Cell> route, float timeStarted)
        {
            if (timeStarted != _lastRouteStartTime) return;
            _route = route;
        }

        private Cell _lastTargetCell;

        private void CheckForRequiredPathfind()
        {
            if (_currentTime > 0f)
            {
                _currentTime -= Time.deltaTime;
                return;
            }

            _currentTime = 1f;
            if (_destinationCell == null) return;
            if (_destinationCell == _lastTargetCell) return;
            _lastTargetCell = _destinationCell;
            _lastRouteStartTime = Time.timeSinceLevelLoad;
            _routeThread = PathingGrid.ThreadRouteToCell(CurrentCell(), _destinationCell, this, _lastRouteStartTime);
            if (RouteWaitCoroutine != null) StopCoroutine(RouteWaitCoroutine);
            RouteWaitCoroutine = StartCoroutine(WaitForRoute(_reachTargetAction, _targetDistance));
        }

        private Coroutine RouteWaitCoroutine;

        private IEnumerator WaitForRoute(Action reachTargetAction, float distanceFromTarget)
        {
            while (_routeThread.IsAlive) yield return null;
            if (distanceFromTarget != 0)
            {
                for (int i = _route.Count - 1; i >= 0; --i)
                {
                    Cell c = _route[i];
                    float distance = Vector2.Distance(c.Position, GetTarget().CurrentCell().Position);
                    if (distance < distanceFromTarget)
                    {
                        _route.RemoveAt(i);
                    }
                    else break;
                }
            }

            PathingGrid.SmoothRoute(_route);
            Reposition(reachTargetAction);
        }

        protected void GoToCell(Cell targetCell, Action reachTargetAction, float distanceFromTarget = 0)
        {
            _destinationCell = targetCell;
            _targetDistance = distanceFromTarget;
            _reachTargetAction = reachTargetAction;
        }

        protected void FindCellToAttackPlayer(Action reachCellAction, float range)
        {
            _reachTargetAction = reachCellAction;
            _destinationCell = PlayerCombat.Instance.CurrentCell();
            _targetDistance = range;
        }
    }
}