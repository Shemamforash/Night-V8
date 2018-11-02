using System.Collections;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

public class NeedleBehaviour : MonoBehaviour
{
    private static readonly ObjectPool<NeedleBehaviour> _needlePool = new ObjectPool<NeedleBehaviour>("Needles", "Prefabs/Combat/Needle");
    private float _time;
    private ParticleSystem _burstParticles, _trailParticles;
    private Rigidbody2D _rigidBody2D;
    private bool _isPlayerNeedle;
    private int _damage;
    private bool _firing;

    public static void Create(Vector2 origin, Vector2 target, int damage, bool isPlayerNeedle = false)
    {
        _needlePool.Create().Initialise(origin, target, damage, isPlayerNeedle);
    }

    private void Initialise(Vector2 origin, Vector2 target, int damage, bool isPlayerNeedle)
    {
        _time = 0f;
        _isPlayerNeedle = isPlayerNeedle;
        _damage = damage;
        transform.position = origin;
        _trailParticles.Clear();
        float rotation = AdvancedMaths.AngleFromUp(origin, target);
        transform.rotation = Quaternion.Euler(0f, 0f, rotation);
        _firing = true;
    }

    public void OnDestroy()
    {
        _needlePool.Return(this);
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        CanTakeDamage hitThing = other.GetComponent<CanTakeDamage>();
        if (hitThing != null)
        {
            bool validHit = hitThing == PlayerCombat.Instance && !_isPlayerNeedle
                            || hitThing != PlayerCombat.Instance && _isPlayerNeedle;
            if (validHit) hitThing.TakeRawDamage(_damage, _rigidBody2D.velocity.normalized);
        }

        _burstParticles.Emit(50);
        _trailParticles.Stop();
        transform.position = new Vector2(100, 100);
        _rigidBody2D.velocity = Vector2.zero;
        StartCoroutine(WaitAndDie());
    }

    private IEnumerator WaitAndDie()
    {
        yield return new WaitForSeconds(2f);
        Return();
    }

    private void Return()
    {
        _firing = false;
        _needlePool.Return(this);
    }

    private void Awake()
    {
        _trailParticles = gameObject.GetComponent<ParticleSystem>();
        _burstParticles = gameObject.FindChildWithName<ParticleSystem>("Burst");
        _rigidBody2D = gameObject.GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (!_firing) return;
        float speed = Mathf.Lerp(0f, 20f, _time * _time * _time);
        _time += Time.fixedDeltaTime;
        GetComponent<Rigidbody2D>().AddForce(transform.up * speed);
        if (_time > 3f) Return();
    }
}