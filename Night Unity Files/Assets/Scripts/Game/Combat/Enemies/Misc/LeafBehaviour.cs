using DG.Tweening;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class LeafBehaviour : MonoBehaviour
{
    private Rigidbody2D _rb2d;
    private static GameObject prefab;

    private void Awake()
    {
        _rb2d = GetComponent<Rigidbody2D>();
        Sequence sequence = DOTween.Sequence();
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sequence.Append(sprite.DOColor(UiAppearanceController.InvisibleColour, 8f));
        sequence.AppendCallback(() => Destroy(gameObject));
    }

    public static void CreateLeaves(Vector2 direction, Vector2 origin)
    {
        if (prefab == null) prefab = Resources.Load<GameObject>("Prefabs/Combat/Dust");
        for (int j = 0; j < Random.Range(10, 20); ++j)
        {
            Vector2 position = AdvancedMaths.RandomVectorWithinRange(origin, 0.1f);
            GameObject leafObject = Instantiate(prefab);
            leafObject.transform.position = position;
            leafObject.transform.localScale = Random.Range(0.05f, 0.1f) * Vector2.one;
            leafObject.GetComponent<Rigidbody2D>().velocity = direction * Random.Range(2f, 4f);
        }
    }

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 8) return;
        Vector3 velocity = other.transform.parent.GetComponent<Rigidbody2D>().velocity;
        float distance = other.transform.Distance(transform);
        distance /= 0.5f;
        _rb2d.AddForce(velocity * 2 * distance * Random.Range(0.8f, 1.1f));
    }
}