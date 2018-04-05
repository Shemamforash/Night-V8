using UnityEngine;

public class CombatShaderController : MonoBehaviour
{
	public float Size = 1;

	private Material _material;

	public void Awake()
	{
		_material = new Material(Shader.Find("Hidden/CombatShader"));
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		_material.SetFloat("_Range", Size);
		Graphics.Blit(source, destination, _material);
	}
}
