using System;
using System.Collections.Generic;
using System.Linq;
using Game.Characters;
using Game.Combat.Enemies.Nightmares;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

public class CombatAudioController : MonoBehaviour
{
    private AudioSource _audioSource;
    private static CombatAudioController _instance;
    private float _maxCombatVolume;
    private bool _useAlternateCombatMusic;
    private float _combatTargetVolume;
    private const float ThresholdCombatMusicDistance = 10f;
    private const float MusicFadeDuration = 5f;

    public void Awake()
    {
        _instance = this;
        _audioSource = GetComponent<AudioSource>();
        Region currentRegion = CharacterManager.CurrentRegion();
        _maxCombatVolume = currentRegion.IsDynamic() ? 0.75f : 1f;
        if (currentRegion.GetRegionType() == RegionType.Tutorial) _maxCombatVolume = 0f;
        PlayClip(currentRegion);
        TryPlayEnterRegionAudio(currentRegion);
        SetReverbZone();
    }

    private void PlayClip(Region currentRegion)
    {
        _useAlternateCombatMusic = currentRegion.GetRegionType() == RegionType.Tomb;
        AudioClip audioClip = _useAlternateCombatMusic ? AudioClips.GodsAreDead : AudioClips.AbandonedLands;
        float startTime = Random.Range(0f, audioClip.length);
        _audioSource.clip = audioClip;
        _audioSource.volume = 0;
        _audioSource.time = startTime;
        _audioSource.Play();
    }

    private void TryPlayEnterRegionAudio(Region currentRegion)
    {
        AudioSource enterAudioSource = gameObject.FindChildWithName<AudioSource>("Region Enter");
        if (currentRegion.GetRegionType() == RegionType.Tutorial) return;
        enterAudioSource.Play();
    }

    private void SetReverbZone()
    {
        AudioReverbZone reverbZone = GetComponent<AudioReverbZone>();
        switch (EnvironmentManager.CurrentEnvironmentType())
        {
            case EnvironmentType.Desert:
                reverbZone.reverbPreset = AudioReverbPreset.Plain;
                break;
            case EnvironmentType.Mountains:
                reverbZone.reverbPreset = AudioReverbPreset.Mountains;
                break;
            case EnvironmentType.Sea:
                reverbZone.reverbPreset = AudioReverbPreset.StoneCorridor;
                break;
            case EnvironmentType.Ruins:
                reverbZone.reverbPreset = AudioReverbPreset.Hangar;
                break;
            default:
                reverbZone.reverbPreset = AudioReverbPreset.Generic;
                break;
        }
    }

    private void OnDestroy() => _instance = null;

    private int GetEnemiesInRange()
    {
        if (PlayerCombat.Instance == null) return -1;
        RegionType regionType = CharacterManager.CurrentRegion().GetRegionType();
        switch (regionType)
        {
            case RegionType.Tomb:
                return Tomb.TombActive ? 1 : 0;
            case RegionType.Cache:
                bool canPlay = CacheController.Active() && CacheController.Started();
                return canPlay ? 1 : 0;
        }

        Vector2 playerPosition = PlayerCombat.Position();
        List<CanTakeDamage> enemies = CombatManager.Instance().Enemies();
        if (enemies.Count == 0) return -1;
        int enemiesInRange = enemies.Count(e => e.transform.Distance(playerPosition) <= ThresholdCombatMusicDistance && !(e is AnimalBehaviour));
        return enemiesInRange;
    }

    private void UpdateTargetVolume()
    {
        int enemiesInRange = GetEnemiesInRange();
        float timeChange = Time.deltaTime / 5f;
        switch (enemiesInRange)
        {
            case -1:
                timeChange *= -MusicFadeDuration;
                break;
            case 0:
                timeChange *= -1;
                break;
        }

        _combatTargetVolume = Mathf.Clamp(_combatTargetVolume + timeChange, 0, _maxCombatVolume);
    }

    public void Update()
    {
        UpdateTargetVolume();
        UpdateVolume(_combatTargetVolume, _audioSource);
    }

    private void UpdateVolume(float targetVolume, AudioSource layer)
    {
        float layerDifference = targetVolume - layer.volume;
        float incrementAmount = 1f;
        if (incrementAmount > Mathf.Abs(layerDifference)) incrementAmount = Mathf.Abs(layerDifference);
        if (layerDifference < 0) incrementAmount = -incrementAmount;
        layer.volume += incrementAmount * Time.deltaTime;
    }

    public static float GetTargetVolume() => _instance == null ? 0 : _instance._combatTargetVolume;
}