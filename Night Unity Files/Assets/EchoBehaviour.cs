using Game.Characters;
using Game.Combat.Generation;
using UnityEngine;

public class EchoBehaviour : MonoBehaviour
{
	private static GameObject _echoPrefab;
	private Player _player;

	public static void Create(Vector2 position, Player player)
	{
		if (_echoPrefab == null) _echoPrefab = Resources.Load<GameObject>("Prefabs/Combat/Echo");
		EchoBehaviour echoBehaviour = Instantiate(_echoPrefab).GetComponent<EchoBehaviour>();
		echoBehaviour.transform.position = position;
		echoBehaviour.SetPlayer(player);
	}

	private void SetPlayer(Player player)
	{
		_player = player;
	}

	public void OnTriggerEnter2D(Collider2D other)
	{
		if (!other.CompareTag("Player")) return;
		CombatManager.SetCharacterEcho(_player);
		//todo fade me out
		Destroy(gameObject);
	}
}
