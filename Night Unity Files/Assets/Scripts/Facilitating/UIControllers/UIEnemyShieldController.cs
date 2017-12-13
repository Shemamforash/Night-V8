using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UIEnemyShieldController : MonoBehaviour
{
	public Image ShieldImage;
	public float ShieldDecayRate = 0.2f;
	private float _shieldAlpha;
	
	private void OnParticleCollision(GameObject other)
	{
		_shieldAlpha = 1f;
	}

	public void Update()
	{
		ShieldImage.color = new Color(1,1,1,_shieldAlpha);
		_shieldAlpha -= ShieldDecayRate * Time.deltaTime;
		if (_shieldAlpha < 0f)
		{
			_shieldAlpha = 0f;
		}
	}
}
