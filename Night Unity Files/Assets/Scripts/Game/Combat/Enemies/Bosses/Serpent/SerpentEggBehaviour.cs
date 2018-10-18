using System.Collections;
using Game.Combat.Enemies.Misc;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

public class SerpentEggBehaviour : MonoBehaviour
{
    private static GameObject _eggPrefab;

    public static void Create(Vector2 position)
    {
        if (_eggPrefab == null) _eggPrefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Serpent/Serpent Egg");
        GameObject egg = Instantiate(_eggPrefab);
        egg.transform.position = position;
    }

    public void Awake()
    {
        StartCoroutine(WaitToPop());
    }

    private IEnumerator WaitToPop()
    {
        float timeToPop = Random.Range(MinPopTime, MaxPopTime);
        while (timeToPop > 0f)
        {
            timeToPop -= Time.deltaTime;
            yield return null;
        }

        Pop();
        Destroy(gameObject);
    }

    private void Pop()
    {
        for (int i = 0; i < Random.Range(2, 5); ++i)
        {
            Grenade g = Grenade.CreateBasic(transform.position, AdvancedMaths.RandomVectorWithinRange(transform.position, 2f), false);
            g.SetExplosionRadius(Random.Range(0.5f, 1f));
        }
    }

    private const float MinPopTime = 1f;
    private const float MaxPopTime = 2f;
}