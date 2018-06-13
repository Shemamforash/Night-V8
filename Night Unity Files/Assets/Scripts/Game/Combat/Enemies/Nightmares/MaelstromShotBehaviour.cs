using Game.Combat.Player;
using UnityEngine;

public class MaelstromShotBehaviour : MonoBehaviour
{
    private Vector3 _direction;
    
    public static void Create(Vector3 direction, Vector3 position)
    {
        GameObject shot = Instantiate(Resources.Load<GameObject>("Prefabs/Combat/Maelstrom Shot"));
        shot.transform.position = position + direction * 0.2f;
        shot.GetComponent<Rigidbody2D>().velocity = direction.normalized * 10f;
        shot.GetComponent<MaelstromShotBehaviour>()._direction = direction;
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
