using UnityEngine;
using UnityEngine.UI;

public class ShadowShaderController : MonoBehaviour
{
	private Material _shadowMaterial;
	public RenderTexture LightTexture;

	public void Awake()
	{
		_shadowMaterial = GetComponent<Image>().material;
		_shadowMaterial.mainTexture = LightTexture;
	}
	
	public void OnDestroy()
	{
		LightTexture.Release();
	}
}
