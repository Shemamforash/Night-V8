using UnityEngine;
using UnityEngine.UI;

public class ShadowShaderController : MonoBehaviour
{
    private Material _shadowMaterial;
    public Camera TargetCamera;

    public void Awake()
    {
        _shadowMaterial = GetComponent<Image>().material;
        _shadowMaterial.mainTexture = TargetCamera.targetTexture;
    }

    public void Update()
    {
        ReleaseTexture();
        RenderTexture shadowTexture = new RenderTexture(Screen.width, Screen.height, 0);
        shadowTexture.format = RenderTextureFormat.ARGB32;
        shadowTexture.useDynamicScale = true;
        shadowTexture.Create();
        TargetCamera.targetTexture = shadowTexture;
        TargetCamera.Render();
        _shadowMaterial.mainTexture = TargetCamera.targetTexture;
        _shadowMaterial.SetTexture("_ShadowTex", TargetCamera.targetTexture);
    }

    private void ReleaseTexture()
    {
        if (TargetCamera == null) return;
        if (TargetCamera.targetTexture == null) return;
        TargetCamera.targetTexture.Release();
    }

    public void OnDestroy()
    {
        ReleaseTexture();
    }
}