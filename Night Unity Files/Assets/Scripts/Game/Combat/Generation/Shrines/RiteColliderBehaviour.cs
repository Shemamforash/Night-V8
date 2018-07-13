using Game.Combat.Generation.Shrines;
using UnityEngine;

public class RiteColliderBehaviour : MonoBehaviour
{
    private RiteShrineBehaviour _riteShrine;
    
    public void Awake()
    {
        _riteShrine = transform.parent.parent.GetComponent<RiteShrineBehaviour>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _riteShrine.EnterShrineCollider(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _riteShrine.ExitShrineCollider();
    }
}
