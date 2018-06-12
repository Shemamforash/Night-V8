using Game.Combat.Generation;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;

public class SaltBehaviour : MonoBehaviour
{
    private static GameObject _prefab;
    private Rigidbody2D _rigidBody;
    private const float Force = 0.2f;
    private const float PickupRadius = 1f;

    public void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
    }

    public static void Create(Vector2 position)
    {
        if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Salt");
        GameObject salt = Instantiate(_prefab);
        salt.transform.position = AdvancedMaths.RandomVectorWithinRange(position, 0.5f);
        salt.GetComponent<Rigidbody2D>().AddForce(AdvancedMaths.RandomVectorWithinRange(Vector2.zero, 1).normalized);
    }

    public void Update()
    {
        Vector2 directionToPlayer = CombatManager.Player().transform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        if (distanceToPlayer > PickupRadius) return;
        float forceMod = 1f - distanceToPlayer / PickupRadius;
        _rigidBody.AddForce(directionToPlayer.normalized * Force * forceMod);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        CombatManager.Player().Player.Inventory().IncrementResource("Salt", 1);
        Destroy(gameObject);
    }
}