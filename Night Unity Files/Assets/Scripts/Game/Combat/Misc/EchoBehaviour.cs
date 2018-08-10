using DG.Tweening;
using Game.Characters;
using Game.Combat.Ui;
using UnityEngine;

public class EchoBehaviour : MonoBehaviour
{
    private static GameObject _echoPrefab;
    private Player _player;
    private static GameObject _burstPrefab;

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
        string storyLines = _player.GetStoryLine();
        PlayerUi.SetEventText(storyLines, StoryController.GetTimeToRead(storyLines));
        if (_burstPrefab == null) _burstPrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Disappear");
        ParticleSystem p = Instantiate(_burstPrefab).GetComponent<ParticleSystem>();
        p.Emit(150);
        p.transform.position = transform.position;
        Sequence s = DOTween.Sequence();
        s.AppendInterval(p.main.duration);
        s.AppendCallback(() => Destroy(p.gameObject));
        Destroy(gameObject);
    }
}