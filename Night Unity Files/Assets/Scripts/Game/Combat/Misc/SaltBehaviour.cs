using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;

public class SaltBehaviour : MonoBehaviour
{
    private static ObjectPool<SaltBehaviour> _saltPool = new ObjectPool<SaltBehaviour>("Salt", "Prefabs/Combat/Salt");
    private Rigidbody2D _rigidBody;
    private const float Force = 0.4f;
    private const float PickupRadius = 2f;

    public void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
    }

    public static void Create(Vector2 position)
    {
        SaltBehaviour salt = _saltPool.Create();
        salt.transform.position = AdvancedMaths.RandomVectorWithinRange(position, 0.5f);
        salt._rigidBody.AddForce(AdvancedMaths.RandomVectorWithinRange(Vector2.zero, 1).normalized);
    }

    public void Update()
    {
        if (!CombatManager.IsCombatActive()) return;
        Vector2 directionToPlayer = PlayerCombat.Instance.transform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        if (distanceToPlayer > PickupRadius) return;
        TryShowTutorial();
        
        float forceMod = 1f - distanceToPlayer / PickupRadius;
        _rigidBody.AddForce(directionToPlayer.normalized * Force * forceMod);
    }

    private void TryShowTutorial()
    {
        TutorialManager.TryOpenTutorial(5);
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