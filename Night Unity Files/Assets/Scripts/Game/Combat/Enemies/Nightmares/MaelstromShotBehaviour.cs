using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

public class MaelstromShotBehaviour : MonoBehaviour
{
    private Vector3 _direction;
    private Rigidbody2D _rigidBody;
    private float _lifeTime = 1f;
    
    public static void Create(Vector3 direction, Vector3 position)
    {
        GameObject shot = Instantiate(Resources.Load<GameObject>("Prefabs/Combat/Maelstrom Shot"));
        shot.transform.position = position + direction * 0.2f;
        shot.GetComponent<MaelstromShotBehaviour>()._direction = direction;
    }

    public void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _rigidBody.velocity = _direction.normalized * 15f;
    }

    public void FixedUpdate()
    {
        Vector2 dir = new Vector2(-_rigidBody.velocity.y, _rigidBody.velocity.x).normalized;
        float angle = Vector2.Angle(dir, PlayerCombat.Instance.transform.position - transform.position);
        float force = 1000;
        Debug.Log(angle);
        if (angle > 90)
        {
            force = -force;
        }
        _rigidBody.AddForce(force * dir * Time.fixedDeltaTime * _rigidBody.mass);
    }

    public void Update()
    {
        _lifeTime -= Time.deltaTime;
        if (_lifeTime < 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        PlayerCombat player = other.gameObject.GetComponent<PlayerCombat>();
        if (player != null)
        {
            player.AddForce(_direction.normalized * 30f);
        }
        Destroy(transform.Find("GameObject").gameObject);
        transform.DetachChildren();
        Destroy(gameObject);
    }
}
