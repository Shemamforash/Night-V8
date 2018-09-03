using DG.Tweening;
using SamsHelper.Libraries;
using UnityEngine;

public class WormBodyBehaviour : MonoBehaviour
{
    private ParticleSystem _dustParticles, _cloudParticles, _explodeParticles, _implodeParticles;

    private GameObject _teethObject;

    private SpriteRenderer _warningGlow;

    public void Awake()
    {
        _dustParticles = gameObject.FindChildWithName<ParticleSystem>("Dust");
        _cloudParticles = gameObject.FindChildWithName<ParticleSystem>("Clouds");
        _explodeParticles = gameObject.FindChildWithName<ParticleSystem>("Explode");
        _implodeParticles = gameObject.FindChildWithName<ParticleSystem>("Implode");
        _teethObject = gameObject.FindChildWithName("Teeth");
        _warningGlow = gameObject.FindChildWithName<SpriteRenderer>("Warning");
    }
    
    public void Initialise(Vector2 position)
    {
        transform.position = position;
        transform.localScale = Vector2.one;
        _teethObject.SetActive(false);
        _warningGlow.color = new Color(1,1,1,0f);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_warningGlow.DOColor(Color.white, 2f));
        sequence.InsertCallback(1f, () => _cloudParticles.Play());
        sequence.AppendCallback(() =>
        {
            _teethObject.SetActive(true);
            _explodeParticles.Emit(200);
            _dustParticles.Play();
            Debug.Log("fart");
        });
        sequence.Append(_warningGlow.DOColor(new Color(1, 1, 1, 0f), 0.5f));
        sequence.AppendInterval(Random.Range(3f, 6f));
        
        Sequence endSequence = DOTween.Sequence();
        endSequence.AppendCallback(() => { _implodeParticles.Emit(200); });
        endSequence.Append(transform.DOScale(Vector2.zero, 0.5f));
        endSequence.AppendCallback(() =>
        {
            _dustParticles.Stop();
            _cloudParticles.Stop();
        });
        endSequence.InsertCallback(0.5f, () => _teethObject.SetActive(false));
        endSequence.InsertCallback(2f, () =>
        {
            WormBehaviour.Unregister(this);
            Destroy(gameObject);
        });
        sequence.Append(endSequence);
    }
}