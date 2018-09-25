using Game.Gear;
using Game.Gear.Weapons;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI;
using UnityEngine;

public class DurabilityBarController : MonoBehaviour
{
    private RectTransform _durabilityTransform;
    private ParticleSystem _durabilityParticles;
    private Weapon _weapon;
    private bool _forceUpdate;
    private float _lastPixelWidth;
    private const float AbsoluteMaxDurability = ((int) ItemQuality.Radiant + 1) * 10;
    private const float WorldWidthCoefficient = 0.00502f;

    public void Awake()
    {
        _durabilityTransform = gameObject.FindChildWithName<RectTransform>("Max");
        _durabilityParticles = gameObject.FindChildWithName<ParticleSystem>("Current");
    }

    public void Reset()
    {
        _durabilityTransform.anchorMin = Vector2.zero;
        _durabilityTransform.anchorMax = Vector2.zero;
    }

    public void Update()
    {
        if (_weapon == null)
        {
            _durabilityParticles.Stop();
            return;
        }

        float pixelWidth = _durabilityTransform.rect.width;
        if (!_durabilityParticles.isPlaying) _durabilityParticles.Play();
        if (!_forceUpdate && pixelWidth == _lastPixelWidth) return;

        Number durability = _weapon.WeaponAttributes.GetDurability();
        float maxDurability = durability.Max;
        float normalisedDurability = durability.Normalised();
        float rectAnchorOffset = maxDurability / AbsoluteMaxDurability / 2;

        _durabilityParticles.Clear();
        _forceUpdate = false;
        _lastPixelWidth = pixelWidth;

        _durabilityTransform.anchorMin = new Vector2(0.5f - rectAnchorOffset, 0.5f);
        _durabilityTransform.anchorMax = new Vector2(0.5f + rectAnchorOffset, 0.5f);

        float worldWidth = pixelWidth * WorldWidthCoefficient * normalisedDurability;
        ParticleSystem.ShapeModule shape = _durabilityParticles.shape;
        ParticleSystem.EmissionModule emission = _durabilityParticles.emission;
        shape.radius = worldWidth;
        emission.rateOverTime = 300 * worldWidth / 3;
    }

    public void SetWeapon(Weapon weapon)
    {
        _weapon = weapon;
        _forceUpdate = true;
        _durabilityParticles.Clear();
    }
}