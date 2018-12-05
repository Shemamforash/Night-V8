using System.Collections.Generic;
using Facilitating.UIControllers;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class SkillBar : MonoBehaviour
    {
        private const int NoSlots = 4;
        private static Skill[] _skills;
        private static List<int> _skillsLocked;
        private static List<CooldownController> CooldownControllers;
        private static List<UISkillCostController> CostControllers;
        private static float _cooldownModifier;
        private static bool SkillsAreFree;
        private static bool _skillsReady;
        private static float _cooldownRemaining;
        private static float _duration;

        public void Awake()
        {
            CooldownControllers = new List<CooldownController>();
            CostControllers = new List<UISkillCostController>();

            for (int i = 0; i < NoSlots; ++i)
            {
                CooldownControllers.Add(gameObject.FindChildWithName<CooldownController>("Skill " + (i + 1)));
                CostControllers.Add(CooldownControllers[i].gameObject.FindChildWithName<UISkillCostController>("Cost"));
            }

            _skills = new Skill[NoSlots];
            _skillsLocked = new List<int>();
            _skillsReady = false;
        }

        private void UpdateCooldownControllers(float normalisedDuration)
        {
            CooldownControllers.ForEach(c => c.UpdateCooldownFill(normalisedDuration));
        }

        public void Update()
        {
            if (_skillsReady) return;
            _cooldownRemaining -= Time.deltaTime;
            if (_cooldownRemaining < 0)
            {
                _cooldownRemaining = 0;
                UpdateCooldownControllers(1);
                _skillsReady = true;
                return;
            }

            float normalisedTime = 1 - _cooldownRemaining / _duration;
            UpdateCooldownControllers(normalisedTime);
        }

        public static void BindSkills(Characters.Player player, float skillCooldownModifier)
        {
            _cooldownModifier = skillCooldownModifier;
            Skill characterSkillOne = null;
            Skill characterSkillTwo = null;
            Skill weaponSkillOne = null;
            Skill weaponSkillTwo = null;

            if (player.Attributes.SkillOneUnlocked)
                characterSkillOne = player.CharacterSkillOne;

            if (player.Attributes.SkillTwoUnlocked)
                characterSkillTwo = player.CharacterSkillTwo;

            if (player.EquippedWeapon != null)
            {
                if (player.Attributes.WeaponSkillOneUnlocks.Contains(player.EquippedWeapon.WeaponType()))
                    weaponSkillOne = player.EquippedWeapon.WeaponSkillOne;

                if (player.Attributes.WeaponSkillTwoUnlocks.Contains(player.EquippedWeapon.WeaponType()))
                    weaponSkillTwo = player.EquippedWeapon.WeaponSkillTwo;
            }

#if UNITY_EDITOR
            SkillsAreFree = true;
            characterSkillOne = player.CharacterSkillOne;
            characterSkillTwo = player.CharacterSkillTwo;

            if (player.EquippedWeapon != null)
            {
                weaponSkillOne = player.EquippedWeapon.WeaponSkillOne;
                weaponSkillTwo = player.EquippedWeapon.WeaponSkillTwo;
            }
#endif

            BindSkill(0, characterSkillOne);
            BindSkill(1, characterSkillTwo);
            BindSkill(2, weaponSkillOne);
            BindSkill(3, weaponSkillTwo);
        }

        private static void BindSkill(int slot, Skill skill)
        {
            _skills[slot] = skill;
            CooldownControllers[slot].SetSkill(skill);
            if (skill == null) return;
            CostControllers[slot].SetCost(skill.AdrenalineCost());
        }

        private static bool TryLockSkill(int skillNo)
        {
            float noSkillChance = PlayerCombat.Instance.Player.Attributes.SkillDisableChance;
            if (Random.Range(0f, 1f) > noSkillChance) return false;
            _skillsLocked.Add(skillNo);
            return true;
        }

        private static bool IsSkillFree()
        {
            float freeSkillChance = PlayerCombat.Instance.Player.Attributes.FreeSkillChance;
            return Random.Range(0f, 1f) <= freeSkillChance;
        }

        public static void ActivateSkill(int skillNo)
        {
            if (_skillsLocked.Contains(skillNo)) return;
            if (!_skillsReady) return;
            if (TryLockSkill(skillNo)) return;
            bool freeSkill = IsSkillFree();
            if (!_skills[skillNo].Activate(freeSkill || SkillsAreFree)) return;
            TutorialManager.TryOpenTutorial(14);
            if (freeSkill) return;
            StartCooldown(_skills[skillNo].AdrenalineCost());
        }

        private static void StartCooldown(int duration)
        {
            _skillsReady = false;
            _duration = duration * _cooldownModifier;
            _cooldownRemaining = _duration;
        }
    }
}