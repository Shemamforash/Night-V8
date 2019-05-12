using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

public class BulletImpactBehaviour : MonoBehaviour
{
	private static readonly ObjectPool<BulletImpactBehaviour> _bulletPool = new ObjectPool<BulletImpactBehaviour>("Bullet Impacts", "Prefabs/Combat/Visuals/Bullet Impact");
	private                 ParticleSystem                    _particles;

	public static void Create(Vector3 position, float rotation)
	{
		BulletImpactBehaviour impact = _bulletPool.Create();
		impact.transform.position = position;
		impact.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));
		impact._particles.Emit(Random.Range(2, 6));
	}

	public void Awake()
	{
		if (_particles == null) _particles = GetComponent<ParticleSystem>();
	}

	public void Update()
	{
		if (_particles.particleCount != 0) return;
		_bulletPool.Return(this);
	}

	public void OnDestroy()
	{
		_bulletPool.Dispose(this);
	}
}