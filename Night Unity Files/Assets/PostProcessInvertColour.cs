using DG.Tweening;
using UnityEngine;

public class PostProcessInvertColour : MonoBehaviour
{
    public Material Material;
    private static readonly int _invertLevel = Shader.PropertyToID("_InvertLevel");

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest, Material);
    }

    public void FadeTo(float value, float duration)
    {
        DOTween.To(() => Material.GetFloat(_invertLevel), f => Material.SetFloat(_invertLevel, f), value, duration);
    }

    public void Set(float val) => Material.SetFloat(_invertLevel, val);

    private void OnDestroy() => Set(0);

    public float CurrentValue() => Material.GetFloat(_invertLevel);
}