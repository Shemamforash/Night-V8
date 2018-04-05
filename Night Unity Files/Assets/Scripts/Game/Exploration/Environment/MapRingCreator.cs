using Game.World.Region;
using SamsHelper;
using UnityEngine;

public class MapRingCreator : MonoBehaviour
{
    public void Awake()
    {
        GameObject ringPrefab = Resources.Load<GameObject>("Prefabs/Map/Map Ring");
        for (int i = 1; i <= 10; ++i)
        {
            int ringRadius = i * MapGenerator.MinRadius * 2;
            GameObject ring = Instantiate(ringPrefab, transform.position, ringPrefab.transform.rotation);
            ring.transform.SetParent(transform);
            ring.name = "Ring: distance " + i + " hours";
            RingDrawer ringDrawer = ring.GetComponent<RingDrawer>();
            ringDrawer.DrawCircle(ringRadius);
            float alpha = 1f / 9f * i + 1f / 9f;
            alpha = 1 - alpha;
            ringDrawer.SetColor(new Color(1, 1, 1, alpha));
        }
    }
}