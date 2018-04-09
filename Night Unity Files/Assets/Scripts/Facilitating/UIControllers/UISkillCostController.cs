using System.Collections.Generic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UISkillCostController : MonoBehaviour
    {
        private readonly List<GameObject> _skillCostBlips = new List<GameObject>();

        public void SetCost(int cost)
        {
            _skillCostBlips.ForEach(Destroy);
            for (int i = 0; i < cost; ++i)
            {
                GameObject newBlip = Helper.InstantiateUiObject("Prefabs/AttributeMarkerPrefab", transform);
                _skillCostBlips.Add(newBlip);
            }
        }
    }
}