using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Assets;
using Facilitating.Persistence;
using Game.Characters.CharacterActions;
using Game.Combat;
using Game.Combat.Enemies;
using Game.Combat.Skills;
using Game.Gear.Weapons;
using Game.World;
using Game.World.Region;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Input;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Characters.Player
{
    public class Player : Character, IInputListener
    {
        public readonly StateMachine States = new StateMachine();
        public readonly CharacterTemplate CharacterTemplate;
        public int DistanceFromHome;
        public CharacterView CharacterView;
        public readonly DesolationAttributes Attributes;
        public readonly Number Energy = new Number();
        public event Action<Shot> OnFireAction;
        public event Action OnReloadAction;
        public bool Retaliate;

        private CollectResources _collectResourcesAction;
        private Sleep _sleepAction;
        public Idle IdleAction;
        public Travel TravelAction;
        public Return ReturnAction;
        private CraftAmmo _craftAmmoAction;
        private CharacterActions.Combat _combatAction;
        private LightFire _lightFireAction;
        public readonly RageController RageController;

        private Cooldown _reloadingCooldown;

        private int _storyProgress;

        public Skill CharacterSkillOne, CharacterSkillTwo;
        
        private bool _fired;

        public string GetCurrentStoryProgress()
        {
            if (_storyProgress == CharacterTemplate.StoryLines.Count) return null;
            string currentLine = CharacterTemplate.StoryLines[_storyProgress];
            ++_storyProgress;
            return currentLine;
        }

        public override void Kill()
        {
            if (SceneManager.GetActiveScene().name == "Game") WorldState.HomeInventory().RemoveItem(this);
            if (CombatManager.Player == this) CombatManager.FailCombat();
        }

        //Create Character in code only- no view section, no references to objects in the scene
        public Player(CharacterTemplate characterTemplate) : base("The " + characterTemplate.CharacterClass)
        {
            Debug.Log("Created");
            Attributes = new DesolationAttributes(this);
            MovementController = new MovementController(this, 0);
            CharacterTemplate = characterTemplate;
            CharacterSkills.GetCharacterSkills(this);
            CharacterInventory.MaxWeight = 50;
            Attributes.Endurance.AddOnValueChange(a => { Energy.Max = a.CurrentValue(); });
            RageController = new RageController(this);
            HealthController.AddOnHeal(a => UpdateHealthUi(HealthController.GetNormalisedHealthValue()));
            HealthController.AddOnTakeDamage(a => UpdateHealthUi(HealthController.GetNormalisedHealthValue()));
            Energy.OnMin(Sleep);
            Position.AddOnValueChange(a => { UIEnemyController.Enemies.ForEach(e => e.Position.UpdateValueChange()); });
            SetReloadCooldown();
            RecoilManager.Recoil.AddOnValueChange(a =>
            {
                CombatManager.RecoilManager?.SetValue(a.Normalised());
            });
        }

        ~Player()
        {
            Debug.Log("Destroyed " + Name);
        }

        public void OnHit(Shot shot, int damage)
        {
            OnHit(damage);
            if (shot.Origin() == null) return;
            if (Retaliate) FireWeapon(shot.Origin());
        }

        private void ChangeCover()
        {
            if (InCover) LeaveCover();
            else TakeCover();
        }

        public override void TakeCover()
        {
            base.TakeCover();
            CombatManager.SetCentralViewAlpha(0.4f);
            CombatManager.SetCoverText("In Cover");
        }

        public override void LeaveCover()
        {
            base.LeaveCover();
            CombatManager.SetCentralViewAlpha(1f);
            CombatManager.SetCoverText("Exposed");
        }

        protected override void KnockDown()
        {
            if (IsKnockedDown) return;
            base.KnockDown();
            UIKnockdownController.StartKnockdown(10);
        }

        private void UpdateHealthUi(float normalisedHealth)
        {
            HeartBeatController.SetHealth(normalisedHealth);
            CombatManager.UpdatePlayerHealth();
        }

        public override XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            base.Save(doc, saveType);
            SaveController.CreateNodeAndAppend("Class", doc, Name);
            SaveController.CreateNodeAndAppend("Distance", doc, DistanceFromHome);
            SaveController.CreateNodeAndAppend("Energy", doc, Energy.CurrentValue());
            Attributes.Save(doc, saveType);
            return doc;
        }

        public override ViewParent CreateUi(Transform parent)
        {
            return new InventoryUi(this, parent);
        }

        protected override void SetConditions()
        {
            base.SetConditions();
            Burn.OnConditionNonEmpty = CombatManager.PlayerHealthBar.StartBurning;
            Burn.OnConditionEmpty = CombatManager.PlayerHealthBar.StopBurning;
            Bleeding.OnConditionNonEmpty = CombatManager.PlayerHealthBar.StartBleeding;
            Bleeding.OnConditionEmpty = CombatManager.PlayerHealthBar.StopBleeding;
            Sick.OnConditionNonEmpty = () => CombatManager.PlayerHealthBar.UpdateSickness(Sick.GetNormalisedValue());
            Sick.OnConditionEmpty = () => CombatManager.PlayerHealthBar.UpdateSickness(0);
        }

        //Links character to object in scene
        public override void SetGameObject(GameObject gameObject)
        {
            base.SetGameObject(gameObject);
            SetCharacterUi();
            AddStates();
        }

        private void SetCharacterUi()
        {
            CharacterView = new CharacterView(this);
        }

        public List<BaseCharacterAction> StatesAsList(bool includeInactiveStates)
        {
            return (from BaseCharacterAction s in States.StatesAsList() where s.IsStateVisible() || includeInactiveStates select s).ToList();
        }

        private void AddStates()
        {
            _collectResourcesAction = new CollectResources(this);
            _combatAction = new CharacterActions.Combat(this);
            _sleepAction = new Sleep(this);
            IdleAction = new Idle(this);
            TravelAction = new Travel(this);
            ReturnAction = new Return(this);
            _lightFireAction = new LightFire(this);
            _craftAmmoAction = new CraftAmmo(this);
            States.SetDefaultState(IdleAction);
            CharacterView.FillActionList();
        }

        private bool IsOverburdened()
        {
            return CharacterInventory.Weight >= CharacterInventory.MaxWeight;
        }

        private void Tire()
        {
            int amount = 1;
            if (IsOverburdened()) amount *= 2;
            Energy.Decrement(amount);
        }

        private void Sleep()
        {
            BaseCharacterAction action = (BaseCharacterAction) States.GetCurrentState();
            action.Interrupt();
            Sleep sleepAction = States.GetState("Sleep") as Sleep;
            if (sleepAction != null)
            {
                sleepAction.SetStateTransitionTarget(action);
                sleepAction.AddOnExit(() => { action.Resume(); });
            }

            _sleepAction.Enter();
        }

        public void Rest(int amount)
        {
            Energy.Increment(amount);
            if (!Energy.ReachedMax()) return;
            if (DistanceFromHome == 0)
            {
                IdleAction.Enter();
            }
        }

        public void Travel()
        {
            DistanceFromHome++;
            Tire();
        }

        public void Return()
        {
            --DistanceFromHome;
            Tire();
        }

        public override void Equip(GearItem gearItem)
        {
            base.Equip(gearItem);
            switch (gearItem.GetGearType())
            {
                case GearSubtype.Weapon:
                    Debug.Log(((Weapon) gearItem).WeaponAttributes.Print());
                    CharacterView?.WeaponGearUi.SetGearItem(gearItem);
                    break;
                case GearSubtype.Armour:
                    CharacterView?.ArmourGearUi.SetGearItem(gearItem);
                    break;
                case GearSubtype.Accessory:
                    CharacterView?.AccessoryGearUi.SetGearItem(gearItem);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //MISC
        public override bool Immobilised()
        {
            return _reloadingCooldown.Running() || IsKnockedDown;
        }

        //RELOADING
        private void Reload()
        {
            if (Immobilised()) return;
            if (_reloadingCooldown.Running()) return;
            if (EquipmentController.Weapon().FullyLoaded()) return;
            if (EquipmentController.Weapon().GetRemainingMagazines() == 0) return;
            OnFireAction = null;
            Retaliate = false;
            float reloadSpeed = EquipmentController.Weapon().GetAttributeValue(AttributeType.ReloadSpeed);
            UIMagazineController.EmptyMagazine();
            _reloadingCooldown.Duration = reloadSpeed;
            _reloadingCooldown.Start();
        }

        private void StopReloading()
        {
            if (_reloadingCooldown == null || _reloadingCooldown.Finished()) return;
            _reloadingCooldown.Cancel();
        }

        //COOLDOWNS

        private void SetReloadCooldown()
        {
            _reloadingCooldown = CombatManager.CombatCooldowns.CreateCooldown();
            _reloadingCooldown.SetStartAction(() =>
            {
                EquipmentController.Weapon().Reload(Inventory());
                UpdateMagazineUi();
            });
            _reloadingCooldown.SetDuringAction(t =>
            {
                if (t > _reloadingCooldown.Duration * 0.8f)
                {
                    UIMagazineController.EmptyMagazine();
                }
                else
                {
                    t = (t - _reloadingCooldown.Duration * 0.2f) / (_reloadingCooldown.Duration * 0.8f);
                    t = 1 - t;
                    UIMagazineController.UpdateReloadTime(t);
                }
            });
            _reloadingCooldown.SetEndAction(() => OnReloadAction?.Invoke());
        }

        //FIRING
        protected override Shot FireWeapon(Character target)
        {
            Shot shot = base.FireWeapon(target);
            if (shot != null)
            {
                if (RageController.Active()) shot.GuaranteeCritical();
                shot.SetDamageModifier(Attributes.GetGunDamageModifier());
                OnFireAction?.Invoke(shot);
                UpdateMagazineUi();
                shot.Fire();
                _fired = true;
            }
            return shot;
        }

        //MISC

        protected override void Interrupt()
        {
            StopReloading();
            MovementController.StopSprinting();
            UpdateMagazineUi();
        }

        public void UpdateMagazineUi()
        {
            string magazineMessage = "";
            if (EquipmentController.Weapon().GetRemainingMagazines() == 0) magazineMessage = "NO AMMO";
            else if (EquipmentController.Weapon().Empty())
                magazineMessage = "RELOAD";
            if (magazineMessage == "")
            {
                UIMagazineController.UpdateMagazine();
            }
            else
            {
                UIMagazineController.EmptyMagazine();
                UIMagazineController.SetMessage(magazineMessage);
            }
        }

        private void TryMelee()
        {
            if (CombatManager.CurrentTarget.DistanceToPlayer <= Enemy.MeleeDistance)
            {
                MeleeController.StartMelee(CombatManager.CurrentTarget);
            }
        }
        
        //INPUT

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (Immobilised()) return;
            switch (axis)
            {
                case InputAxis.Cover:
                    if (!isHeld) ChangeCover();
                    break;
                case InputAxis.Enrage:
                    RageController.TryStart();
                    break;
                case InputAxis.Fire:
                    if (!_fired && isHeld) break;
                    if (!_fired || EquipmentController.Weapon().WeaponAttributes.Automatic) FireWeapon(CombatManager.CurrentTarget);
                    break;
                case InputAxis.Reload:
                    Reload();
                    break;
                case InputAxis.Horizontal:
                    MovementController.Move(direction);
                    break;
                case InputAxis.Sprint:
                    MovementController.StartSprinting();
                    break;
                case InputAxis.Melee:
                    TryMelee();
                    break;
                case InputAxis.SkillOne:
                    SkillBar.ActivateSkill(0);
                    break;
                case InputAxis.SkillTwo:
                    SkillBar.ActivateSkill(1);
                    break;
                case InputAxis.SkillThree:
                    SkillBar.ActivateSkill(2);
                    break;
                case InputAxis.SkillFour:
                    SkillBar.ActivateSkill(3);
                    break;
            }
        }

        public void OnInputUp(InputAxis axis)
        {
            switch (axis)
            {
                case InputAxis.Fire:
                    _fired = false;
                    break;
                case InputAxis.Sprint:
                    MovementController.StopSprinting();
                    break;
            }
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
            if (axis == InputAxis.Horizontal)
            {
                MovementController.Dash(direction);
            }
        }

        public void CollectResourcesInRegion(Region region)
        {
            _collectResourcesAction.SetTargetRegion(region);
        }

        public override void EnterCombat()
        {
            base.EnterCombat();
            RageController.EnterCombat();
            SetConditions();
            SkillBar.BindSkills(this);
            CombatManager.UpdatePlayerHealth();
            FacingDirection = Direction.Right;
            InputHandler.RegisterInputListener(this);
            UIMagazineController.SetWeapon(EquipmentController.Weapon());
        }

        public override void ExitCombat()
        {
            UIKnockdownController.Exit();
            MeleeController.Exit();
            IsKnockedDown = false;
            StopReloading();
            InputHandler.UnregisterInputListener(this);
            Position.SetCurrentValue(0);
            _fired = false;
        }
    }
}