using UnityEngine;
using UnityEngine.UI;

public class ShadowShaderController : MonoBehaviour
{
	private Material _shadowMaterial;
	public RenderTexture LightTexture;
	public Camera LightCamera;

	public void Awake()
	{
		_shadowMaterial = GetComponent<Image>().material;
		_shadowMaterial.mainTexture = LightTexture;
	}

	public void Update()
	{
		LightTexture.DiscardContents();
		LightCamera.Render();
		_shadowMaterial.mainTexture = LightTexture;
	}

	public void OnDestroy()
	{
		LightTexture.Release();
	}
}
