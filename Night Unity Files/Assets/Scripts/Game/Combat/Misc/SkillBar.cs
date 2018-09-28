using System.Collections;
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
        private const float BaseSkillCooldown = 5f;
        private const int NoSlots = 4;
        private static Skill[] _skills;
        private static List<int> _skillsLocked;
        private static List<CooldownController> CooldownControllers;
        private static List<UISkillCostController> CostControllers;
        private static float _cooldownModifier;
        private static Coroutine _cooldownCoroutine;
        private static SkillBar _instance;
        private static bool _finishEarly;

        public void Awake()
        {
            _instance = this;
            CooldownControllers = new List<CooldownController>();
            CostControllers = new List<UISkillCostController>();

            for (int i = 0; i < NoSlots; ++i)
            {
                CooldownControllers.Add(gameObject.FindChildWithName<CooldownController>("Skill " + (i + 1)));
                CostControllers.Add(CooldownControllers[i].gameObject.FindChildWithName<UISkillCostController>("Cost"));
            }

            _skills = new Skill[NoSlots];
            _skillsLocked = new List<int>();
        }

        private void UpdateCooldownControllers(float normalisedDuration)
        {
            for (int i = 0; i < CooldownControllers.Count; i++)
            {
                bool skillReady = false;
                if (i < _skills.Length)
                {
                    skillReady = _skills[i].CanAfford();
                }

                CooldownControllers[i].UpdateCooldownFill(normalisedDuration, skillReady);
            }
        }

        private IEnumerator SkillsCooldown()
        {
            float duration = BaseSkillCooldown * _cooldownModifier;
            Debug.Log(BaseSkillCooldown + " " + _cooldownModifier);
            float currentTime = 0f;
            UpdateCooldownControllers(0f);
            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                if (_finishEarly) currentTime = duration;
                if (currentTime > duration) currentTime = duration;
                UpdateCooldownControllers(currentTime / duration);
                yield return null;
            }

            _finishEarly = false;
            _cooldownCoroutine = null;
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
            CooldownControllers[slot].SetVisible(skill != null);
            if (skill == null) return;
            CooldownControllers[slot].Text(skill.Name);
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
            if (_cooldownCoroutine != null) return;
            if (TryLockSkill(skillNo)) return;
            bool freeSkill = IsSkillFree();
            if (!_skills[skillNo].Activate(freeSkill)) return;
            _cooldownCoroutine = _instance.StartCoroutine(_instance.SkillsCooldown());
        }

        public static void ResetCooldowns()
        {
            //todo fix me
            _finishEarly = true;
        }
    }
}