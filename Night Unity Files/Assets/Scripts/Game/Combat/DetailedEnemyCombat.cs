using System;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Enemies.EnemyTypes;
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
        public bool HasFled;
        protected bool Alerted;
        private readonly CharacterAttribute _visionRange = new CharacterAttribute(AttributeType.Vision, 30f);
        private readonly CharacterAttribute _detectionRange = new CharacterAttribute(AttributeType.Detection, 15f);
        public EnhancedButton PrimaryButton;

        private bool _inSight;


        private const int EnemyReloadMultiplier = 4;

        public Action CurrentAction;
        private readonly Cooldown _firingCooldown;
        private const float MaxAimTime = 2f;
        protected float MinimumFindCoverDistance;
        private int _wanderDirection = -1;

        public TextMeshProUGUI ActionText;
        public UIAimController UiAimController;
        public UIHitController UiHitController;
        private float _currentFadeInTime = 2f;
        private const float MaxFadeInTime = 2f;
        private string _actionString;
        public Enemy Enemy;

        private const float KnockdownDuration = 5f;
        private bool _waitingForHeal;
        
        public float DistanceToPlayer;
        private const float AlphaCutoff = 0.2f;

        private const float FadeVisibilityDistance = 5f;
        public TextMeshProUGUI DistanceText, NameText;
        private float _currentAlpha;

        public override void Awake()
        {
            base.Awake();
            DistanceText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Distance");
            NameText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Name");
            SetAlpha(0f);
            Position.AddOnValueChange(a =>
            {
                if (IsDead) return;
                DistanceToPlayer = Position.CurrentValue() - CombatManager.Player.Position.CurrentValue();
                UpdateDistance();
            });
            
            UiHitController = Helper.FindChildWithName<UIHitController>(gameObject, "Cover");
            UiAimController = Helper.FindChildWithName<UIAimController>(gameObject, "Aim Timer");
            ActionText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Action");
            PrimaryButton = gameObject.GetComponent<EnhancedButton>();
            PrimaryButton.AddOnSelectEvent(() => CombatManager.Player.SetTarget(this));
            HealthController.AddOnTakeDamage(f => { Alert(); });
            Position.AddOnValueChange(a =>
            {
                if (HasFled || IsDead) return;
                _aheadOfPlayer = DistanceToPlayer > 0;
            });
        }

        private void SetAlpha(float alpha)
        {
            gameObject.GetComponent<CanvasGroup>().alpha = alpha;
            _currentAlpha = alpha;
        }

        private void UpdateDistance()
        {
            UpdateDistanceText();
            UpdateDistanceAlpha();
        }

        private void UpdateDistanceAlpha()
        {
            float distanceToMaxVisibility = CombatManager.VisibilityRange + FadeVisibilityDistance - DistanceToPlayer;
            float alpha = 0;
            if (DistanceToPlayer < CombatManager.VisibilityRange)
            {
                float normalisedDistance = Helper.Normalise(DistanceToPlayer, CombatManager.VisibilityRange);
                alpha = 1f - normalisedDistance;
                alpha = Mathf.Clamp(alpha, AlphaCutoff, 1f);
            }
            else if (distanceToMaxVisibility >= 0)
            {
                alpha = Helper.Normalise(distanceToMaxVisibility, FadeVisibilityDistance);
                alpha = Mathf.Clamp(alpha, 0, AlphaCutoff);
            }

//            SetNavigatable(alpha > 0);
            SetAlpha(alpha);
        }
        
        protected Action MoveToTargetPosition(float position)
        {
            if (Position.CurrentValue() > position)
            {
                return () =>
                {
                    MoveForward();
                    if (Position.CurrentValue() > position) return;
                    Position.SetCurrentValue(position);
                    ReachTarget();
                };
            }
            return () =>
            {
                MoveBackward();
                if (Position.CurrentValue() < position) return;
                Position.SetCurrentValue(position);
                ReachTarget();
            };
        }

        public virtual void SetPlayer(Character enemy)
        {
            base.SetPlayer(enemy);
            NameText.text = enemy.Name;
            Enemy = (Enemy)enemy;
            SetOwnedByEnemy(Enemy.Template.Speed);
            HealthController.SetInitialHealth(Enemy.Template.Health, this);
            ArmourController.SetArmourValue(Enemy.Armour.ArmourRating);
            RecoilManager.EnterCombat();
            if (!(enemy is Medic || enemy is Martyr)) SetHealBehaviour();
            CurrentAction = Wander;
            UiHitController.SetCharacter(this);
        }

        public void Reset()
        {
            Alerted = false;
            CurrentAction = Wander;
        }

        protected void UpdateDistanceText()
        {
            if (_inSight)
            {
                float distance = Helper.Round(DistanceToPlayer);
                string distanceText = distance + "m";
                DistanceText.text = distanceText;
                return;
            }
            DistanceText.text = "behind";
        }

        public void Hide()
        {
            _inSight = false;
            UiAimController.Hide();
            UpdateDistanceText();
            SetActionText(_actionString);
        }

        public void Show()
        {
            _inSight = true;
            UiAimController.Show();
            SetActionText(_actionString);
            UpdateDistanceText();
            HealthController.SetValue(_currentAlpha);
        }

        private void SetArmour(int armourLevel, bool inCover)
        {
            //todo get armour
            if (!_inSight) return;
            ArmourController.SetArmourValue(armourLevel);
        }

        public void MarkSelected()
        {
            PrimaryButton.GetComponent<Button>().Select();
            CombatManager.Player.UpdatePlayerDirection();
        }

        protected override void TakeArmourDamage(int damage)
        {
            ArmourController.TakeDamage(damage);
        }

        protected override void KnockDown()
        {
            base.KnockDown();
            CurrentAction = KnockedDown();
        }

        private void SetHealBehaviour()
        {
            HealthController.AddOnTakeDamage(a =>
            {
                if (HealthController.GetNormalisedHealthValue() > 0.25f || _waitingForHeal) return;
                foreach (DetailedEnemyCombat enemy in UIEnemyController.Enemies)
                {
                    Medic medic = enemy as Medic;
                    if (medic == null || medic.HasTarget()) continue;
                    CurrentAction = WaitForHeal(medic);
                    break;
                }
            });
        }

        private Action WaitForHeal(Medic medic)
        {
            SetActionText("Waiting for Medic");
            TakeCover();
            medic.RequestHeal(this);
            _waitingForHeal = true;
            return () =>
            {
                if (!medic.IsDead) return;
                _waitingForHeal = false;
                ChooseNextAction();
            };
        }

        public void ClearHealWait()
        {
            ChooseNextAction();
        }

        public void ReceiveHealing(int amount)
        {
            HealthController.Heal(amount);
            ChooseNextAction();
        }

        private Action KnockedDown()
        {
            float duration = KnockdownDuration;
            SetActionText("Knocked Down");
            return () =>
            {
                duration -= Time.deltaTime;
                if (duration > 0) return;
                ChooseNextAction();
                IsKnockedDown = false;
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
            float absoluteDistance = Math.Abs(DistanceToPlayer);
            if (absoluteDistance <= MeleeDistance)
            {
                return Melee();
            }

            if (absoluteDistance < MinimumFindCoverDistance || absoluteDistance > CombatManager.VisibilityRange || moveAnyway)
            {
                float targetDistance = CalculateIdealRange();
                return MoveToTargetDistance(targetDistance);
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
                if (Math.Abs(DistanceToPlayer) > MeleeDistance)
                {
                    CurrentAction = Stagger();
                    return;
                }

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

        private bool _aheadOfPlayer;

        public Action MoveToTargetDistance(float distance)
        {
            Assert.IsTrue(distance >= 0);
            Action moveForwardAction = () =>
            {
                MoveForward();
                if (DistanceToPlayer > distance) return;
                ReachTarget();
            };
            Action moveBackwardAction = () =>
            {
                MoveBackward();
                if (DistanceToPlayer < distance) return;
                ReachTarget();
            };

            if (_aheadOfPlayer)
            {
                SetActionText(DistanceToPlayer > distance ? "Approaching" : "Retreating");
                return DistanceToPlayer > distance ? moveForwardAction : moveBackwardAction;
            }

            SetActionText(DistanceToPlayer > distance ? "Retreating" : "Approaching");
            return DistanceToPlayer > distance ? moveBackwardAction : moveForwardAction;
        }

        private void Wander()
        {
            float distanceToTravel = Random.Range(0f, 5f);
            distanceToTravel *= _wanderDirection;
            _wanderDirection = -_wanderDirection;
            float targetPosition = Position.CurrentValue() + distanceToTravel;
            CurrentAction = MoveToTargetPosition(targetPosition);
            SetActionText("Wandering");
            CheckForPlayer();
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
            if (DistanceToPlayer > _visionRange) return;
            CurrentAction = Suspicious;
        }

        private float _timeSinceSawPlayer = 0f;
        private const float _timeToForgetPlayer = 2f;

        private void Suspicious()
        {
            SetActionText("Suspicious");
            if (DistanceToPlayer >= _detectionRange.CurrentValue()) return;
            if (DistanceToPlayer >= _visionRange.CurrentValue()) CurrentAction = Wander;
            Alert();
        }

        public bool InCombat()
        {
            return !HasFled && !IsDead && Alerted;
        }

        protected void SetActionText(string action)
        {
            _actionString = action;
            if (!_inSight) action = "";
            ActionText.text = action;
        }

        public override void OnHit(int damage, bool critical)
        {
            base.OnHit(damage, critical);
            if (critical)
            {
                UiHitController.RegisterCritical();
            }
            else
            {
                UiHitController.RegisterShot();
            }

            if (!Alerted) Alert();
        }

        public override void OnMiss()
        {
            if (!Alerted) Alert();
        }


        public virtual void Alert()
        {
            if (Alerted) return;
            Alerted = true;
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
                Wander();
            }
        }

        private Action Reload()
        {
            if (Weapon().GetRemainingMagazines() == 0)
            {
                return Flee();
            }

            TakeCover();
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
                MoveBackward();
                CombatManager.CheckEnemyFled(this);
            };
        }

        private Action Fire()
        {
            int divider = Random.Range(3, 6);
            int noShots = (int) (Weapon().GetAttributeValue(AttributeType.Capacity) / divider);
            bool automatic = Weapon().WeaponAttributes.Automatic;
            SetActionText("Firing");
            if (!automatic)
            {
                noShots = 1;
            }

            return () =>
            {
                Shot s = FireWeapon(CombatManager.Player);
                if (s == null) return;
                s.Fire();
                UiAimController.Fire();
                --noShots;
                int remainingAmmo = Weapon().GetRemainingAmmo();
                if (noShots == 0 || remainingAmmo == 0)
                {
                    UpdateAim(0);
                }

                if (remainingAmmo == 0 || !automatic)
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
            UIEnemyController.Remove(this);
            Enemy.Kill();
            Destroy(this);
        }
        
        public override void UpdateCombat()
        {
            base.UpdateCombat();
            CurrentAction?.Invoke();
        }
        
        private void UpdateAim(float value)
        {
            UiAimController.SetValue(value);
        }

        protected virtual Action Aim()
        {
            if (Weapon().Empty())
            {
                return Reload();
            }

            LeaveCover();
            Assert.IsFalse(Weapon().Empty());
            float aimTime = MaxAimTime;
            SetActionText("Aiming");
            return () =>
            {
                if (Immobilised()) return;
                float normalisedTime = Helper.Normalise(aimTime, MaxAimTime);
                UpdateAim(1 - normalisedTime);
                aimTime -= Time.deltaTime;
                if (aimTime < 0)
                {
                    CurrentAction = Fire();
                }
            };
        }
    }
}