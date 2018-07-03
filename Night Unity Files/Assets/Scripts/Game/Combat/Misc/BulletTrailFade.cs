using System.Collections;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class BulletTrailFade : MonoBehaviour
{
    private static readonly ObjectPool<BulletTrailFade> _pool = new ObjectPool<BulletTrailFade>("Prefabs/Combat/Bullet Trail");
    private TrailRenderer _trailRenderer;

    public void Awake()
    {
        if (_trailRenderer != null) return;
        _trailRenderer = GetComponent<TrailRenderer>();
    }

    public void SetAlpha(float alpha)
    {
        Color c = _trailRenderer.startColor;
        c.a = alpha;
        _trailRenderer.startColor = c;
    }
    
    public void StartFade(float duration)
    {
        transform.SetParent(null);
        StartCoroutine(Fade(duration));
    }

    private IEnumerator Fade(float duration)
    {
        Color startColour = _trailRenderer.startColor;
        float fadeTime = duration;
        while (duration > 0f)
        {
            duration -= Time.deltaTime;
            _trailRenderer.startColor = Color.Lerp(startColour, UiAppearanceController.InvisibleColour, 1f - duration / fadeTime);
            yield return null;
        }

        _pool.Return(this);
    }

    public static BulletTrailFade Create()
    {
        BulletTrailFade bulletTrail = _pool.Create();
        bulletTrail._trailRenderer.Clear();
        return bulletTrail;
    }

    public void OnDestroy()
    {
        _pool.Dispose(this);
    }

    public void SetPosition(Transform parent)
    {
        transform.SetParent(parent, false);
        transform.position = parent.position;
        _trailRenderer.Clear();
    }
}
