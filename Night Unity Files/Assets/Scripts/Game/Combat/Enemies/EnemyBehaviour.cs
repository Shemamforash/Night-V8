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
        private bool _pathingAllowed = false;
        private Thread _routeThread;

        public bool OnScreen() => Helper.IsObjectInCameraView(gameObject);

        public override Weapon Weapon() => null;

        public override void Update()
        {
            base.Update();
            PushAwayFromNeighbors();
            UpdateAlpha();
            if (!CombatManager.InCombat()) return;
            UpdateRotation();
            CurrentAction?.Invoke();
        }

        private IEnumerator WaitForNextPathFind()
        {
            yield return new WaitForSeconds(1);
            _pathingAllowed = true;
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
            //todo remove friendly fire
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

            PlayerCombat.Instance.Player.BrandManager.IncreaseWeaponKills(PlayerCombat.Instance.Player.Weapon.WeaponType());
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

        private List<Cell> _route = new List<Cell>();

//        protected void GetRouteToCell(Cell target, Action reachTargetAction)
//        {
//            if (_waitingForRoute) return;
//            _routeThread.Interrupt();
//            _routeThread = PathingGrid.ThreadRouteToCell(CurrentCell(), target, route);
//            _waitingForRoute = true;
//            CurrentAction = () =>
//            {
//                while (routeThread.IsAlive) return;
//                _waitingForRoute = false;
//                Reposition(reachTargetAction);
//            };
//        }

        private float _lastRouteStartTime;
        
        public void SetRoute(List<Cell> route, float timeStarted)
        {
            if (timeStarted != _lastRouteStartTime) return;
            _route = route;
        }
        
        protected void GoToCell(Cell targetCell, Action reachTargetAction, float distanceFromTarget = 0)
        {
            _lastRouteStartTime = Time.timeSinceLevelLoad;
            _routeThread = PathingGrid.ThreadRouteToCell(CurrentCell(), targetCell, this, _lastRouteStartTime);
            Debug.DrawLine(CurrentCell().Position, targetCell.Position, Color.cyan, 2f);
            CurrentAction = () =>
            {
                while (_routeThread.IsAlive) return;
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
            };
        }

        protected void FindCellToAttackPlayer(Action reachCellAction, float range)
        {
            GoToCell(PlayerCombat.Instance.CurrentCell(), reachCellAction, range);
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

        private void Reposition(Action reachTargetAction)
        {
            SetActionText("Moving");
            Queue<Cell> newRoute = new Queue<Cell>(_route);
            if (newRoute.Count == 0)
            {
                Debug.Log(name + " has no route");
                return;
            }
//            Debug.DrawLine(CurrentCell().Position, _route[_route.Count - 1].Position, Color.red, 5f);
            _targetCell = newRoute.Dequeue();
            CurrentAction = () =>
            {
                if (_reachedTarget)
                {
                    if (newRoute.Count == 0)
                    {
                        _targetCell = null;
                        reachTargetAction();
                        return;
                    }

                    _targetCell = newRoute.Dequeue();
                }

                MoveToCell(_targetCell);
            };
        }

        protected virtual void ReachTarget()
        {
        }

        public virtual string GetEnemyName() => Enemy.Name;
    }
}