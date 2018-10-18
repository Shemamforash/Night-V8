using DG.Tweening;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

public class WormBodyBehaviour : MonoBehaviour
{
    private ParticleSystem _dustParticles, _cloudParticles, _explodeParticles, _implodeParticles;

    private GameObject _teethObject;

    private SpriteRenderer _warningGlow;
    private float _dealDamageTimer;
    private bool _dealDamage;

    public void Awake()
    {
        _dustParticles = gameObject.FindChildWithName<ParticleSystem>("Dust");
        _cloudParticles = gameObject.FindChildWithName<ParticleSystem>("Clouds");
        _explodeParticles = gameObject.FindChildWithName<ParticleSystem>("Explode");
        _implodeParticles = gameObject.FindChildWithName<ParticleSystem>("Implode");
        _teethObject = gameObject.FindChildWithName("Teeth");
        _warningGlow = gameObject.FindChildWithName<SpriteRenderer>("Warning");
    }

    public void OnTriggerStay2D(Collider2D other)
    {
        if (!_dealDamage) return;
        if (!other.gameObject.CompareTag("Player")) return;
        _dealDamageTimer -= Time.deltaTime;
        if (_dealDamageTimer > 0f) return;
        _dealDamageTimer = 0.2f;
        PlayerCombat.Instance.TakeRawDamage(5, AdvancedMaths.RandomVectorWithinRange(Vector2.zero, 1).normalized);
        PlayerCombat.Instance.MovementController.KnockBack(AdvancedMaths.RandomVectorWithinRange(Vector2.zero, 1).normalized * Random.Range(20, 40));
    }

    public void Update()
    {
        if (!_dealDamage) return;
        PlayerCombat.Instance.Shake(500);
    }
    
    public void Initialise(Vector2 position)
    {
        _dealDamageTimer = 0f;
        _dealDamage = false;
        transform.position = position;
        transform.localScale = Vector2.one;
        _teethObject.SetActive(false);
        _warningGlow.color = new Color(1,1,1,0f);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_warningGlow.DOColor(Color.white, 2f));
        sequence.InsertCallback(1f, () => _cloudParticles.Play());
        sequence.AppendCallback(() =>
        {
            _dealDamage = true; 
            _teethObject.SetActive(true);
            _explodeParticles.Emit(200);
            _dustParticles.Play();
        });
        sequence.Append(_warningGlow.DOColor(new Color(1, 1, 1, 0f), 0.5f));
        sequence.AppendInterval(Random.Range(3f, 6f));
        
        Sequence endSequence = DOTween.Sequence();
        endSequence.AppendCallback(() =>
        {
            _dealDamage = false;
            _implodeParticles.Emit(200);
        });
        endSequence.Append(transform.DOScale(Vector2.zero, 0.5f));
        endSequence.AppendCallback(() =>
        {
            _dustParticles.Stop();
            _cloudParticles.Stop();
        });
        endSequence.InsertCallback(0.5f, () => _teethObject.SetActive(false));
        endSequence.InsertCallback(2f, () =>
        {
            Destroy(gameObject);
        });
        sequence.Append(endSequence);
    }
}