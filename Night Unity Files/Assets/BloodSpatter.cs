using System.Collections.Generic;
using SamsHelper.Libraries;
using UnityEngine;

public class BloodSpatter : MonoBehaviour
{
    private static GameObject _spatterPrefab;
    private static List<Sprite> _bloodSprites;

    public void Spray(Vector3 direction, float healthDamage)
    {
        if (_spatterPrefab == null)
        {
            _spatterPrefab = Resources.Load<GameObject>("Images/Blood Spatter");
            _bloodSprites = new List<Sprite>(Resources.LoadAll<Sprite>("Images/Blood"));
        }
        int splatNumber = Mathf.CeilToInt(healthDamage / 2f);
        for (int i = 0; i < splatNumber; ++i)
        {
            GameObject spatterObject = Instantiate(_spatterPrefab);
            float distanceOffset = Mathf.Pow(Random.Range(0f, 1f), 2f) * 0.4f;
            float angleOffset = Mathf.Pow(Random.Range(0f, 1f), 2f) * 20;
            if (Random.Range(0, 2) == 0) angleOffset = -angleOffset;
            spatterObject.GetComponent<SpriteRenderer>().sprite = _bloodSprites.RandomElement();
            direction = Quaternion.AngleAxis(angleOffset, Vector3.forward) * direction;
            Vector2 position = transform.position + direction * distanceOffset;
            spatterObject.transform.position = position;
            spatterObject.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        }

    }
}