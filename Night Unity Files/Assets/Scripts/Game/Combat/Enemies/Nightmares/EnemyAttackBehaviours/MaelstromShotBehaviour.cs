using System.Collections;
using Game.Combat.Player;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;

public class MaelstromShotBehaviour : MonoBehaviour
{
    private static readonly ObjectPool<MaelstromShotBehaviour> _shotPool = new ObjectPool<MaelstromShotBehaviour>("Maelstrom Shots", "Prefabs/Combat/Visuals/Maelstrom Shot");
    private Vector3 _direction;
    private Rigidbody2D _rigidBody;
    private float _lifeTime = 5f;
    private float _speed = 3f;
    private SpriteRenderer[] _sprites;
    private bool _dying, _follow;
    private MaelstromShotTrail _trail;
    private float _angleModifier;
    private const float AngleDecay = 0.97f;
    private const int ShotDamage = 10;
    private AudioSource _audioSource;
    private float _damageModifier = 1;

    public static MaelstromShotBehaviour Create(Vector3 direction, Vector3 position, float speed, bool suppressAudio, bool follow = true)
    {
        MaelstromShotBehaviour shot = _shotPool.Create();
        shot.transform.position = position;
        shot.ResetShot(direction, speed, suppressAudio, follow);
        return shot;
    }

    public static void CreateBurst(int angleInterval, Vector3 position, float speed, bool suppressAudio, int startRotation = 0)
    {
        for (int i = 0; i < 360; i += angleInterval)
        {
            Vector2 direction = AdvancedMaths.CalculatePointOnCircle(i + startRotation, 1, Vector2.zero);
            Create(direction, (Vector2) position + direction / 2f, speed, suppressAudio, false);
        }
    }

    private void ResetShot(Vector2 direction, float speed, bool suppressAudio, bool follow)
    {
        foreach (SpriteRenderer spriteRenderer in _sprites) spriteRenderer.enabled = true;
        if (!suppressAudio) _audioSource.Play();
        _direction = direction;
        _speed = speed;
        _lifeTime = 5f;
        _rigidBody.velocity = _direction * _speed * 4;
        _dying = false;
        _follow = follow;
        _angleModifier = 1;
        _trail = MaelstromShotTrail.Create();
        _trail.SetTarget(transform);
    }

    public void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _sprites = GetComponentsInChildren<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();
    }

    public void FixedUpdate()
    {
        if (_dying || !_follow) return;
        Vector2 dir = new Vector2(-_rigidBody.velocity.y, _rigidBody.velocity.x).normalized;
        float angle = Vector2.Angle(dir, PlayerCombat.Position() - transform.position);
        _angleModifier *= AngleDecay;
        float force = 1000;
        if (angle > 90) force = -force;
        force *= _angleModifier;
        _rigidBody.AddForce(force * dir * Time.fixedDeltaTime * _rigidBody.mass);
        _rigidBody.velocity = Vector3.ClampMagnitude(_rigidBody.velocity, _speed);
    }

    public void Update()
    {
        if (_dying) return;
        _lifeTime -= Time.deltaTime;
        if (_lifeTime > 0) return;
        StartCoroutine(WaitToDie());
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        PlayerCombat player = other.gameObject.GetComponent<PlayerCombat>();
        int damage = (int) (WorldState.ScaleValue(ShotDamage) * _damageModifier);
        player.TakeRawDamage(damage, _rigidBody.velocity.normalized);
        player.MovementController.AddForce(_rigidBody.velocity.normalized * 20f);
        Explode();
    }

    private IEnumerator WaitToDie()
    {
        if (!_trail.gameObject.activeInHierarchy) Destroy(_trail);
        else if (_trail != null) _trail.StartFade();
        foreach (SpriteRenderer spriteRenderer in _sprites) spriteRenderer.enabled = false;
        _dying = true;
        _rigidBody.velocity = Vector2.zero;
        yield return new WaitForSeconds(0.5f);
        _shotPool.Return(this);
    }

    private void Explode()
    {
        MaelstromImpactBehaviour.Create(transform.position);
        StartCoroutine(WaitToDie());
    }

    public void OnDestroy()
    {
        _shotPool.Dispose(this);
    }

    public void SetDamageModifier(float damageModifier)
    {
        _damageModifier = damageModifier;
    }
}