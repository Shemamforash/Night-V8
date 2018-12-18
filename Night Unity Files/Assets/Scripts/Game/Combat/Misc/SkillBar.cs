using System;
using System.Collections.Generic;
using Game.Combat.Player;
using Game.Global.Tutorial;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Misc
{
    public class SkillBar : MonoBehaviour
    {
        private const int NoSlots = 4;
        private static List<CooldownController> _skillControllers;
        private static float _cooldownModifier;
        private static bool SkillsAreFree;
        private static bool _skillsReady;
        private static float _cooldownRemaining;
        private static float _duration;
        private static RectTransform _skillBarRect;
        private static List<TutorialOverlay> _overlays;
        private static Characters.Player _player;

        public void Awake()
        {
            _skillControllers = new List<CooldownController>();

            for (int i = 0; i < NoSlots; ++i)
                _skillControllers.Add(gameObject.FindChildWithName<CooldownController>("Skill " + (i + 1)));

            _skillsReady = false;
            _skillBarRect = GetComponent<RectTransform>();
            _overlays = new List<TutorialOverlay>
            {
                new TutorialOverlay(_skillBarRect),
                new TutorialOverlay(_skillBarRect),
                new TutorialOverlay(_skillBarRect),
                new TutorialOverlay(_skillBarRect)
            };
        }

        private void UpdateCooldownControllers(float normalisedDuration)
        {
            _skillControllers.ForEach(c => c.UpdateCooldownFill(normalisedDuration));
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

        private static bool IsCharacterSkillOneUnlocked() => _player.Attributes.SkillOneUnlocked;
        private static bool IsCharacterSkillTwoUnlocked() => _player.Attributes.SkillTwoUnlocked;
        private static bool IsWeaponSkillOneUnlocked() => _player.Attributes.WeaponSkillOneUnlocks.Contains(_player.EquippedWeapon.WeaponType());
        private static bool IsWeaponSkillTwoUnlocked() => _player.Attributes.WeaponSkillTwoUnlocks.Contains(_player.EquippedWeapon.WeaponType());

        public static void BindSkills(Characters.Player player, float skillCooldownModifier)
        {
            _player = player;
            _cooldownModifier = skillCooldownModifier;
            Skill characterSkillOne = player.CharacterSkillOne;
            Skill characterSkillTwo = player.CharacterSkillTwo;
            Skill weaponSkillOne = player.EquippedWeapon.WeaponSkillOne;
            Skill weaponSkillTwo = player.EquippedWeapon.WeaponSkillTwo;

#if UNITY_EDITOR
            SkillsAreFree = true;
#endif

            BindSkill(0, characterSkillOne, IsCharacterSkillOneUnlocked, player.GetCharacterSkillOneProgress);
            BindSkill(1, characterSkillTwo, IsCharacterSkillTwoUnlocked, player.GetCharacterSkillTwoProgress);
            BindSkill(2, weaponSkillOne, IsWeaponSkillOneUnlocked, player.GetWeaponSkillOneProgress);
            BindSkill(3, weaponSkillTwo, IsWeaponSkillTwoUnlocked, player.GetWeaponSkillTwoProgress);
        }

        private static void BindSkill(int slot, Skill skill, Func<bool> isSkillUnlocked, Func<Tuple<string, float>> getProgress)
        {
            _skillControllers[slot].SetSkill(skill, isSkillUnlocked, getProgress);
        }

        private static bool FailToCastSkill()
        {
            float failChance = PlayerCombat.Instance.Player.Attributes.SkillDisableChance;
            return !(Random.Range(0f, 1f) > failChance);
        }

        private static bool IsSkillFree()
        {
            float freeSkillChance = PlayerCombat.Instance.Player.Attributes.FreeSkillChance;
            return Random.Range(0f, 1f) <= freeSkillChance;
        }

        public static void ActivateSkill(int skillNo)
        {
            if (!_skillsReady) return;
            Skill skill = _skillControllers[skillNo].Skill();
            if (!FailToCastSkill())
            {
                bool freeSkill = IsSkillFree();
                if (!skill.Activate(freeSkill || SkillsAreFree)) return;
                TutorialManager.TryOpenTutorial(14, _overlays);
                if (freeSkill) return;
            }

            StartCooldown(skill.AdrenalineCost());
        }

        private static void StartCooldown(int duration)
        {
            _skillsReady = false;
            _duration = duration * _cooldownModifier;
            _cooldownRemaining = _duration;
        }
    }
}