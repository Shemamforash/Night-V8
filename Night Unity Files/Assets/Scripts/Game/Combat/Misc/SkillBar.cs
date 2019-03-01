using System;
using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Exploration.Regions;
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
        private List<CooldownController> _skillControllers;
        private float _cooldownModifier;
        private bool SkillsAreFree;
        private bool _skillsReady;
        private float _cooldownRemaining;
        private float _duration;
        private Characters.Player _player;
        private static bool _needsUpdate;
        private static SkillBar _instance;
        private bool _hasSkill;
        private bool _seenTutorial;

        public void Awake()
        {
            _instance = this;
            _skillControllers = new List<CooldownController>();

            for (int i = 0; i < NoSlots; ++i)
                _skillControllers.Add(gameObject.FindChildWithName<CooldownController>("Skill " + (i + 1)));

            _skillsReady = false;
        }

        public void Start()
        {
            if (CharacterManager.CurrentRegion().GetRegionType() != RegionType.Tutorial) return;
            gameObject.FindChildWithName<CanvasGroup>("Skills Left").alpha = 0;
            gameObject.FindChildWithName<CanvasGroup>("Skills Right").alpha = 0;
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        private void UpdateCooldownControllers(float normalisedDuration)
        {
            _skillControllers.ForEach(c => c.UpdateCooldownFill(normalisedDuration));
        }

        public void Update()
        {
            ShowSkillTutorial();
            TryUpdateSkills();
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

        private void ShowSkillTutorial()
        {
            if (_seenTutorial || !TutorialManager.Active()) return;
            if (!_hasSkill) return;
            RectTransform skillBarRect = GetComponent<RectTransform>();
            List<TutorialOverlay> overlays = new List<TutorialOverlay>
            {
                new TutorialOverlay(skillBarRect),
                new TutorialOverlay(skillBarRect),
                new TutorialOverlay(skillBarRect),
                new TutorialOverlay(skillBarRect)
            };
            TutorialManager.TryOpenTutorial(15, overlays);
            _seenTutorial = true;
        }

        private bool IsCharacterSkillOneUnlocked() => _player.Attributes.SkillOneUnlocked;
        private bool IsCharacterSkillTwoUnlocked() => _player.Attributes.SkillTwoUnlocked;
        private bool IsWeaponSkillOneUnlocked() => _player.Attributes.WeaponSkillOneUnlocks.Contains(_player.EquippedWeapon.WeaponType());
        private bool IsWeaponSkillTwoUnlocked() => _player.Attributes.WeaponSkillTwoUnlocks.Contains(_player.EquippedWeapon.WeaponType());

        private void TryUpdateSkills()
        {
            if (!_needsUpdate) return;
            _needsUpdate = false;
            _player = CharacterManager.SelectedCharacter;
            _cooldownModifier = _player.Attributes.CalculateSkillCooldownModifier();
            Skill characterSkillOne = _player.CharacterSkillOne;
            Skill characterSkillTwo = _player.CharacterSkillTwo;
            Skill weaponSkillOne = _player.EquippedWeapon.WeaponSkillOne;
            Skill weaponSkillTwo = _player.EquippedWeapon.WeaponSkillTwo;
            _hasSkill = IsCharacterSkillOneUnlocked() || IsCharacterSkillTwoUnlocked() || IsWeaponSkillOneUnlocked() || IsWeaponSkillTwoUnlocked();

#if UNITY_EDITOR
            SkillsAreFree = true;
#endif

            BindSkill(0, characterSkillOne, IsCharacterSkillOneUnlocked, _player.GetCharacterSkillOneProgress);
            BindSkill(1, characterSkillTwo, IsCharacterSkillTwoUnlocked, _player.GetCharacterSkillTwoProgress);
            BindSkill(2, weaponSkillOne, IsWeaponSkillOneUnlocked, _player.GetWeaponSkillOneProgress);
            BindSkill(3, weaponSkillTwo, IsWeaponSkillTwoUnlocked, _player.GetWeaponSkillTwoProgress);
        }

        public static void UpdateSkills()
        {
            _needsUpdate = true;
        }

        private void BindSkill(int slot, Skill skill, Func<bool> isSkillUnlocked, Func<Tuple<string, float>> getProgress)
        {
            _skillControllers[slot].SetSkill(skill, isSkillUnlocked, getProgress);
        }

        private bool IsSkillFree()
        {
            float freeSkillChance = PlayerCombat.Instance.Player.Attributes.FreeSkillChance;
            return Random.Range(0f, 1f) <= freeSkillChance;
        }

        public static SkillBar Instance() => _instance;

        public void ActivateSkill(int skillNo)
        {
            if (!_skillsReady) return;
            Skill skill = _skillControllers[skillNo].Skill();
            if (skill == null) return;
            bool freeSkill = IsSkillFree();
            if (!skill.Activate(freeSkill || SkillsAreFree)) return;
            if (freeSkill) return;
            StartCooldown(skill.AdrenalineCost());
        }

        private void StartCooldown(int duration)
        {
            _skillsReady = false;
            _duration = duration * _cooldownModifier;
            _cooldownRemaining = _duration;
        }
    }
}