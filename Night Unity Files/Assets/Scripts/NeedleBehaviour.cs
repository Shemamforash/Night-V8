using System;
using System.Collections;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

public class NeedleBehaviour : MonoBehaviour
{
    private static readonly ObjectPool<NeedleBehaviour> _needlePool = new ObjectPool<NeedleBehaviour>("Needles", "Prefabs/Combat/Needle");
    private float _time;
    private ParticleSystem _burstParticles, _trailParticles;
    private AudioSource _moveAudio, _burstAudio;
    private Rigidbody2D _rigidBody2D;
    private bool _isPlayerNeedle;
    private int _damage;
    private bool _firing;
    private Action<Vector2> _onHit;
    private SpriteRenderer _sprite;
    private bool _hit;

    private void Awake()
    {
        _trailParticles = gameObject.FindChildWithName<ParticleSystem>("Image");
        _sprite = _trailParticles.gameObject.GetComponent<SpriteRenderer>();
        _burstParticles = gameObject.FindChildWithName<ParticleSystem>("Burst");
        _rigidBody2D = gameObject.GetComponent<Rigidbody2D>();
        _moveAudio = GetComponent<AudioSource>();
        _burstAudio = _burstParticles.gameObject.GetComponent<AudioSource>();
        _moveAudio.clip = AudioClips.NeedleMove;
        _burstAudio.clip = AudioClips.NeedleHit;
    }

    public static void Create(Vector2 origin, Vector2 target, bool isPlayerNeedle = false)
    {
        _needlePool.Create().Initialise(origin, target, isPlayerNeedle, null);
    }

    public static void Create(Vector2 origin, Vector2 target, Action<Vector2> onHit, bool isPlayerNeedle = false)
    {
        _needlePool.Create().Initialise(origin, target, isPlayerNeedle, onHit);
    }

    private void Initialise(Vector2 origin, Vector2 target, bool isPlayerNeedle, Action<Vector2> onHit)
    {
        _time = 0f;
        _isPlayerNeedle = isPlayerNeedle;
        gameObject.layer = isPlayerNeedle ? 16 : 15;
        int damage = _isPlayerNeedle ? 60 : 30;
        _damage = WorldState.ScaleValue(damage);
        transform.position = origin;
        _sprite.SetAlpha(1f);
        _trailParticles.Clear();
        _moveAudio.time = Random.Range(0f, _moveAudio.clip.length);
        _moveAudio.pitch = Random.Range(0.8f, 1f);
        _moveAudio.Play();
        float rotation = AdvancedMaths.AngleFromUp(origin, target);
        transform.rotation = Quaternion.Euler(0f, 0f, rotation);
        _firing = true;
        _onHit = onHit;
    }

    public void OnDestroy()
    {
        _needlePool.Return(this);
        _hit = false;
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (_hit) return;
        _hit = true;
        CanTakeDamage hitThing = other.gameObject.GetComponent<CanTakeDamage>();
        if (hitThing != null)
        {
            bool validHit = hitThing == PlayerCombat.Instance && !_isPlayerNeedle
                            || hitThing != PlayerCombat.Instance && _isPlayerNeedle;
            if (validHit) hitThing.TakeRawDamage(_damage, _rigidBody2D.velocity.normalized);
        }

        _moveAudio.Stop();
        _burstAudio.pitch = Random.Range(0.8f, 1f);
        _burstAudio.Play();
        _burstParticles.Emit(50);
        _trailParticles.Stop();
        _onHit?.Invoke(transform.position);
        _sprite.SetAlpha(0f);
        _rigidBody2D.velocity = Vector2.zero;
        _firing = false;
        StartCoroutine(WaitAndDie());
    }

    private IEnumerator WaitAndDie()
    {
        yield return new WaitForSeconds(2f);
        Return();
    }

    private void Return()
    {
        _hit = false;
        _needlePool.Return(this);
    }

    private void FixedUpdate()
    {
        if (!_firing) return;
        float speed = Mathf.Lerp(1f, 20f, _time * _time * _time);
        _time += Time.fixedDeltaTime;
        GetComponent<Rigidbody2D>().AddForce(transform.up * speed);
        if (_time > 3f) Return();
    }
}