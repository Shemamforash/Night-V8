using Game.Combat.Misc;
using UnityEngine;

public class ShelterCharacterColliderBehaviour : MonoBehaviour
{
	private ShelterCharacterBehaviour _shelterCharacter;

	public void Awake()
	{
		_shelterCharacter = transform.parent.GetComponent<ShelterCharacterBehaviour>();
	}
	
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!other.CompareTag("Player")) return;
		_shelterCharacter.Enter();
		Destroy(gameObject);
	}
}
