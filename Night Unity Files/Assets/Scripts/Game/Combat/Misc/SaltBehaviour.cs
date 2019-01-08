using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Global;
using Game.Global.Tutorial;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;

public class SaltBehaviour : MonoBehaviour
{
    private static ObjectPool<SaltBehaviour> _saltPool = new ObjectPool<SaltBehaviour>("Salt", "Prefabs/Combat/Salt");
    private Rigidbody2D _rigidBody;
    private const float Force = 0.2f;
    private const float PickupRadius = 2f;
    private Transform _tri1Transform, _tri2Transform;
    private List<TutorialOverlay> _overlays;

    public void Awake()
    {
        _tri1Transform = gameObject.FindChildWithName("Triangle 1").transform;
        _tri2Transform = gameObject.FindChildWithName("Triangle 2").transform;
        _tri1Transform.localScale = Vector2.one * 0.5f;
        _rigidBody = GetComponent<Rigidbody2D>();
        _overlays = new List<TutorialOverlay>
        {
            new TutorialOverlay(),
            new TutorialOverlay()
        };
    }

    public static void Create(Vector2 position)
    {
        SaltBehaviour salt = _saltPool.Create();
        salt.transform.position = AdvancedMaths.RandomVectorWithinRange(position, 0.5f);
        salt._rigidBody.AddForce(AdvancedMaths.RandomVectorWithinRange(Vector2.zero, 1).normalized);
    }

    public void Update()
    {
        Pulse();
        if (!CombatManager.IsCombatActive()) return;
        Vector2 directionToPlayer = PlayerCombat.Position() - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        if (distanceToPlayer > PickupRadius) return;
        TryShowTutorial();
        float forceMod = 1f - distanceToPlayer / PickupRadius;
        _rigidBody.AddForce(directionToPlayer.normalized * Force * forceMod);
    }

    private void Pulse()
    {
        float tri1Scale = Mathf.Sin(Time.timeSinceLevelLoad) ;
        tri1Scale = (tri1Scale + 1) / 4f + 0.5f;
        float tri2Scale = Mathf.Sin(Time.timeSinceLevelLoad - 3);
        tri2Scale = (tri2Scale + 1) / 4f + 0.5f;
        _tri1Transform.localScale = tri1Scale * Vector2.one;
        _tri2Transform.localScale = tri2Scale * Vector2.one;
    }

    private void TryShowTutorial()
    {
        TutorialManager.TryOpenTutorial(8, _overlays);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        Inventory.IncrementResource("Salt", 1);
        _saltPool.Return(this);
    }

    private void OnDestroy()
    {
        _saltPool.Return(this);
    }
}