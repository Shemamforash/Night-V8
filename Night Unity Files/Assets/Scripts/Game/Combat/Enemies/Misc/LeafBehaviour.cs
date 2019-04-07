using DG.Tweening;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class LeafBehaviour : MonoBehaviour
{
    private static readonly ObjectPool<LeafBehaviour> _leafPool = new ObjectPool<LeafBehaviour>("Dust", "Prefabs/Combat/Dust");
    private SpriteRenderer _sprite;

    private void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
    }

    private void OnDestroy()
    {
        _leafPool.Dispose(this);
    }

    public static void CreateLeaves(Vector2 direction, Vector2 origin)
    {
        for (int j = 0; j < Random.Range(10, 20); ++j)
        {
            Vector2 position = AdvancedMaths.RandomVectorWithinRange(origin, 0.1f);
            LeafBehaviour leafObject = _leafPool.Create();
            leafObject.Initialise(position, direction);
        }
    }

    private void Initialise(Vector2 position, Vector2 direction)
    {
        transform.position = position;
        transform.localScale = Random.Range(0.05f, 0.1f) * Vector2.one;
        gameObject.GetComponent<Rigidbody2D>().velocity = direction * Random.Range(2f, 4f);
        Sequence sequence = DOTween.Sequence();
        _sprite.color = Color.white;
        sequence.Append(_sprite.DOColor(UiAppearanceController.InvisibleColour, 8f));
        sequence.AppendCallback(() => _leafPool.Return(this));
    }

    public static void CreateLeaves(Vector2 origin)
    {
        for (int j = 0; j < Random.Range(10, 20); ++j)
        {
            Vector2 direction = AdvancedMaths.RandomVectorWithinRange(Vector2.zero, 1).normalized;
            Vector2 position = origin + direction * 0.1f;
            LeafBehaviour leafObject = _leafPool.Create();
            leafObject.Initialise(position, direction);
        }
    }
}