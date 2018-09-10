using SamsHelper.Libraries;
using UnityEngine;

public class NoiseMovementController : MonoBehaviour
{
    private Vector2 start = new Vector2(-4, 0), end = new Vector2(4, 0);
    private float speed = 5f;
    private float duration;
    private float _currentTime;
    public ParticleSystem[] _ps;

    public void Awake()
    {
        float distance = start.Distance(end);
        duration = distance / speed;
        transform.rotation = Quaternion.Euler(0, 0, AdvancedMaths.AngleFromUp(start, end));
    }

    public void Restart()
    {
        transform.position = start;
        _currentTime = 0f;
    }

    public void Update()
    {
        _currentTime += Time.deltaTime;
        float normalisedTime = _currentTime / duration;
        if (normalisedTime > 1)
        {
            foreach(ParticleSystem ps in _ps) ps.Emit(150);
            Restart();
            return;
        }
        transform.position = Vector2.Lerp(start, end, normalisedTime);
        float noise = Mathf.PerlinNoise(_currentTime, 0) * 2 - 1;
        Vector3 offset = transform.right * noise;
        transform.position = offset + transform.position;
    }
}