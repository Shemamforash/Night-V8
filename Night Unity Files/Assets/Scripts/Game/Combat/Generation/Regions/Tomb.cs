using DG.Tweening;
using Game.Combat.Enemies.Bosses;
using Game.Exploration.Environment;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class Tomb : RegionGenerator //not a mine
    {
        protected override void Generate()
        {
            switch (EnvironmentManager.CurrentEnvironment.LevelNo)
            {
                case 0:
//                    OvaBehaviour.Create();
                    GameObject prefab = Resources.Load<GameObject>("Prefabs/Combat/Tomb Portal");
                    Instantiate(prefab).transform.position = Vector2.zero;
                    Sequence sequence = DOTween.Sequence();
                    sequence.AppendInterval(5f);
                    sequence.Append(prefab.transform.DOScale(0f, 1f).SetEase(Ease.InCubic));
                    SerpentBehaviour.Create();
                    break;
                case 1:
                    StarfishBehaviour.Create();
                    break;
                case 2:
                    SwarmBehaviour.Create();
                    break;
                case 3:
                    OvaBehaviour.Create();
                    break;
                case 4:
                    break;
            }
        }
    }
}