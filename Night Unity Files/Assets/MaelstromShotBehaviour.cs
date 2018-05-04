using UnityEngine;

public class MaelstromShotBehaviour : MonoBehaviour {
    public static void Create(Vector3 direction, Vector3 position)
    {
        GameObject shot = Instantiate(Resources.Load<GameObject>("Prefabs/Combat/Maelstrom Shot"));
        shot.transform.position = position + direction * 0.2f;
        shot.GetComponent<Rigidbody2D>().velocity = direction.normalized * 10f;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Destroy(transform.Find("GameObject").gameObject);
        transform.DetachChildren();
        Destroy(gameObject);
    }
}
