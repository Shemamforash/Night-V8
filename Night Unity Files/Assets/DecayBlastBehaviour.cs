using DG.Tweening;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

public class DecayBlastBehaviour : MonoBehaviour
{
    private static readonly ObjectPool<DecayBlastBehaviour> _decayBlasts = new ObjectPool<DecayBlastBehaviour>("Decay Blast", "Prefabs/Combat/Effects/Decay Blast");
    private Rigidbody2D _rigidBody2D;
    private const float MaxLifeTime = 4f;
    private float _lifeTime;

    private void Awake()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
    }

    public static void Create(CanTakeDamage origin, Vector2 target)
    {
        DecayBlastBehaviour blastBehaviour = _decayBlasts.Create();
        blastBehaviour.gameObject.layer = origin is PlayerCombat ? 16 : 15;
        Vector3 direction = target.Direction(origin.transform);
        blastBehaviour.Initialise(origin.transform.position, direction);
    }

    private void Initialise(Vector3 originPosition, Vector3 direction)
    {
        transform.position = originPosition + direction * 0.5f;
        _rigidBody2D.velocity = direction * 5;
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(MaxLifeTime);
        sequence.AppendCallback(() => _rigidBody2D.velocity = Vector2.zero);
        sequence.AppendInterval(2);
        sequence.AppendCallback(() => _decayBlasts.Return(this));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.name);
        _rigidBody2D.velocity = Vector2.zero;
    }

    public void OnDestroy()
    {
        _decayBlasts.Dispose(this);
    }
}