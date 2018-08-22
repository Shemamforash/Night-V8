using System.Collections;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

public class PushController : MonoBehaviour
{
    private ParticleSystem _pushParticles;
    private static readonly ObjectPool<PushController> _pushPool = new ObjectPool<PushController>("Pushes", "Prefabs/Combat/Visuals/Push Burst");

    public static void Create(Vector2 position, float rotation, float arcSize = 20f)
    {
        PushController pushObject = _pushPool.Create();
        pushObject.transform.position = position;
        pushObject.SetArcSize(rotation, arcSize);
    }

    private void SetArcSize(float rotation, float arcSize)
    {
        transform.rotation = Quaternion.Euler(0, 0, rotation);
        ParticleSystem.ShapeModule shape = _pushParticles.shape;
        shape.arc = arcSize;
        int emitCount = (int) (3 * arcSize);
        _pushParticles.Emit(emitCount);
        StartCoroutine(WaitAndDie());
    }

    private IEnumerator WaitAndDie()
    {
        while (_pushParticles.particleCount > 0) yield return null;
        _pushPool.Return(this);
    }

    public void OnDestroy()
    {
        _pushPool.Dispose(this);
    }

    public void Awake()
    {
        _pushParticles = GetComponent<ParticleSystem>();
    }
}