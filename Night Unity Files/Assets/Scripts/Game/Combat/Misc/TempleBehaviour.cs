using SamsHelper.Libraries;
using UnityEngine;

public class TempleBehaviour : MonoBehaviour
{
    private SpriteRenderer _ring1, _ring2;
    private float _ring1Speed, _ring2Speed;
    private const float Ring1MaxAlpha = 0.1f, Ring2MaxAlpha = 0.07f;

    public void Awake()
    {
        _ring1 = Helper.FindChildWithName<SpriteRenderer>(gameObject, "Ring 1");
        _ring2 = Helper.FindChildWithName<SpriteRenderer>(gameObject, "Ring 2");
        _ring1Speed = Random.Range(1, 5);
        _ring2Speed = Random.Range(1, 3);
        _ring1.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360)));
        _ring2.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360)));
        if (Random.Range(0, 2) == 1)
        {
            _ring1Speed = -_ring1Speed;
        }
        else
        {
            _ring2Speed = -_ring2Speed;
        }
    }

    public void Update()
    {
        _ring1.transform.Rotate(new Vector3(0, 0, _ring1Speed * Time.deltaTime));
        _ring2.transform.Rotate(new Vector3(0, 0, _ring2Speed * Time.deltaTime));
        Color color = _ring1.color;
        color.a = Mathf.PerlinNoise(_ring1.transform.rotation.z, 0) * Ring1MaxAlpha;
        _ring1.color = color;

        color = _ring2.color;
        color.a = Mathf.PerlinNoise(_ring2.transform.rotation.z, 0) * Ring2MaxAlpha;
        _ring2.color = color;
    }
}