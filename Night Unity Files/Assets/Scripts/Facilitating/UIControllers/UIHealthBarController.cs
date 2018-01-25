using SamsHelper;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthBarController : MonoBehaviour
{
    private Slider _slider;
    private ParticleSystem _healthParticles;
    private Image _sicknessLevel;
    private ParticleSystem _bleedEffect, _burnEffect;
    private RectTransform _sliderRect;
    private float _edgeWidthRatio = 3f;

    public void Awake()
    {
        _slider = GetComponent<Slider>();
        _healthParticles = Helper.FindChildWithName<ParticleSystem>(gameObject, "Health Effect");
        _sicknessLevel = Helper.FindChildWithName<Image>(gameObject, "Sickness");
        _burnEffect = Helper.FindChildWithName<ParticleSystem>(gameObject, "Burning");
        _bleedEffect = Helper.FindChildWithName<ParticleSystem>(gameObject, "Bleeding");
        SetValue(1, 1);
        if (_slider.direction != Slider.Direction.LeftToRight) return;
        ParticleSystem.ShapeModule shapeModule = _healthParticles.shape;
        shapeModule.rotation = new Vector3(0, 0, 270);
    }

    public void SetIsPlayerBar()
    {
        _edgeWidthRatio = 7.2f;
    }

    public void SetValue(float normalisedHealth, float alpha)
    {
        int amount = 50;
        if (alpha == 0 || normalisedHealth < 0)
        {
            amount = 0;
        }
        else
        {
            _slider.value = normalisedHealth;
        }
        ParticleSystem.MainModule main = _healthParticles.main;
        main.startColor = new Color(1,1,1,alpha);
        ParticleSystem.EmissionModule emission = _healthParticles.emission;
        emission.rateOverDistance = amount;

        float edgeWidth = _edgeWidthRatio * normalisedHealth;
        
        ParticleSystem.ShapeModule burnShape = _burnEffect.shape;
        burnShape.radius = edgeWidth;

        ParticleSystem.EmissionModule burnEmission = _burnEffect.emission;
        burnEmission.rateOverTime = (int)(edgeWidth / _edgeWidthRatio * 50f);

        ParticleSystem.ShapeModule bleedShapeModule = _bleedEffect.shape;
        bleedShapeModule.radius = edgeWidth;

        ParticleSystem.EmissionModule bleedEmission = _bleedEffect.emission;
        bleedEmission.rateOverTime = (int)(edgeWidth / _edgeWidthRatio * 10f);
    }
    
    public void StartBleeding()
    {
        _bleedEffect.Play();
    }

    public void StopBleeding()
    {
        _bleedEffect.Stop();
    }

    public void StartBurning()
    {
        _burnEffect.Play();
    }

    public void StopBurning()
    {
        _burnEffect.Stop();
    }
    
    public void UpdateSickness(float value)
    {
        _sicknessLevel.fillAmount = value;
    }
}