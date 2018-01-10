using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIArmourController : MonoBehaviour
{
    private const int ArmourDivisions = 10;
    private readonly List<GameObject> _armourSegments = new List<GameObject>();
    private const int SegmentSpacing = 5;
    private HorizontalLayoutGroup _layoutGroup;

    // Use this for initialization
    public void Awake()
    {
        _layoutGroup = GetComponent<HorizontalLayoutGroup>();
        _layoutGroup.spacing = SegmentSpacing;
        for (int i = 0; i < ArmourDivisions; ++i)
        {
            GameObject newSegment = new GameObject();
            RectTransform rectTransform = newSegment.AddComponent<RectTransform>();
            rectTransform.SetParent(transform);
            rectTransform.localScale = new Vector3(1, 1, 1);
            newSegment.AddComponent<Image>();
            LayoutElement layout = newSegment.AddComponent<LayoutElement>();
            layout.preferredHeight = GetComponent<RectTransform>().rect.height;
            newSegment.name = "Armour Piece " + (i + 1);
            newSegment.SetActive(false);
            _armourSegments.Add(newSegment);
        }
    }

    public void SetArmourValue(int armourLevel)
    {
        GetComponent<RectTransform>().anchorMin = new Vector2(1 - armourLevel / 10f, 0.5f);
        for (int i = 0; i < _armourSegments.Count; ++i)
        {
            _armourSegments[i].SetActive(i < armourLevel);
        }
    }

    public void SetColor(Color c)
    {
        _armourSegments.ForEach(a => a.GetComponent<Image>().color = c);
    }
}