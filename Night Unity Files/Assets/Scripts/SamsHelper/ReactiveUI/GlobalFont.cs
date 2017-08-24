using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI
{
    [ExecuteInEditMode]
    public class GlobalFont : MonoBehaviour
    {
        public Font UniversalFont;

        public void Update()
        {
            List<Transform> children = Helper.FindAllChildren(transform);
            foreach (Transform child in children)
            {
                Text textComponent = child.GetComponent<Text>();
                if (textComponent != null)
                {
                    textComponent.font = UniversalFont;
                }
            }
        }
    }
}