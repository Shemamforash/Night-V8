using System.Collections.Generic;
using System.Linq;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

public class CombatMusicController : MonoBehaviour
{
    private AudioSource _layer1, _layer2, _layer3;
    private float _layer2TargetVolume, _layer3TargetVolume;
    private float _layer2VolumeGain = 1f, _layer3VolumeGain = 0.5f;

    public void Awake()
    {
        return;
        _layer1 = gameObject.FindChildWithName<AudioSource>("Layer 1");
        _layer2 = gameObject.FindChildWithName<AudioSource>("Layer 2");
        _layer3 = gameObject.FindChildWithName<AudioSource>("Layer 3");
        _layer1.clip = Helper.LoadFileFromAssetBundle<AudioClip>("music/combat/simmav", "simmav a");
        _layer2.clip = Helper.LoadFileFromAssetBundle<AudioClip>("music/combat/simmav", "simmav b");
        _layer3.clip = Helper.LoadFileFromAssetBundle<AudioClip>("music/combat/simmav", "simmav c");

        SetVolume(_layer1, 1);
        SetVolume(_layer2, 0);
        SetVolume(_layer3, 0);
    }

    private void UpdateLayerVolume(float targetVolume, AudioSource layer, float incrementAmount)
    {
        float layerDifference = targetVolume - layer.volume;
        if (incrementAmount > Mathf.Abs(layerDifference)) incrementAmount = Mathf.Abs(layerDifference);
        if (layerDifference < 0) incrementAmount = -incrementAmount;
        layer.volume += incrementAmount * Time.deltaTime;
    }

    public void Update()
    {
        return;
        Vector2 playerPosition = PlayerCombat.Instance.transform.position;
        List<CanTakeDamage> enemies = CombatManager.Enemies();
        float nearestEnemy = 1000f;
        if (enemies.Count != 0) nearestEnemy = enemies.Min(e => e.transform.Distance(playerPosition));
        _layer2TargetVolume = -(nearestEnemy / 2f) + 4f;
        _layer3TargetVolume = _layer2TargetVolume - 1f;
        _layer2TargetVolume = Mathf.Clamp(_layer2TargetVolume, 0, 1);
        _layer3TargetVolume = Mathf.Clamp(_layer3TargetVolume, 0, 1);
        UpdateLayerVolume(_layer2TargetVolume, _layer2, _layer2VolumeGain);
        UpdateLayerVolume(_layer3TargetVolume, _layer3, _layer3VolumeGain);
    }

    private void SetVolume(AudioSource source, float volume)
    {
        source.volume = volume;
        source.Play();
    }
}