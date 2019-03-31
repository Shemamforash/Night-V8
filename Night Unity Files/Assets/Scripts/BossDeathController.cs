using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SamsHelper.Libraries;
using UnityEngine;

public class BossDeathController : MonoBehaviour
{
    private static GameObject _prefab;

    private void Awake()
    {
        SpriteRenderer glow = gameObject.FindChildWithName<SpriteRenderer>("Glow");
        SpriteRenderer softGlow = gameObject.FindChildWithName<SpriteRenderer>("Soft Glow");
        glow.SetAlpha(0f);
        softGlow.SetAlpha(0f);
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(0.25f);
        sequence.AppendCallback(() =>
        {
            glow.SetAlpha(1f);
            softGlow.SetAlpha(1f);
        });
        sequence.Insert(0.25f, glow.DOFade(0f, 4f));
        sequence.Insert(0.25f, softGlow.DOFade(0f, 5f));
        sequence.AppendCallback(() => Destroy(gameObject));
    }

    public static void Create(Vector2 position)
    {
        if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Boss Death");
        GameObject bossDeathObject = Instantiate(_prefab);
        bossDeathObject.transform.position = position;
    }
}