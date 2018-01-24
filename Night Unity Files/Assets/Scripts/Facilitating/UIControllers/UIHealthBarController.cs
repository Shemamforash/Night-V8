using SamsHelper;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthBarController : MonoBehaviour
{
    private Slider _slider;
    private ParticleSystem _healthParticles;
    private RectTransform _sliderRect;
    private Transform _healthParticleTransform;

    public void Awake()
    {
        _slider = GetComponent<Slider>();
        _healthParticles = Helper.FindChildWithName<ParticleSystem>(gameObject, "Health Effect");
        _healthParticleTransform = _healthParticles.transform;
        _sliderRect = _slider.GetComponent<RectTransform>();
        SetValue(1, 1);
        if (_slider.direction != Slider.Direction.LeftToRight) return;
        ParticleSystem.ShapeModule shapeModule = _healthParticles.shape;
        shapeModule.rotation = new Vector3(0, 0, 270);
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
    }
}