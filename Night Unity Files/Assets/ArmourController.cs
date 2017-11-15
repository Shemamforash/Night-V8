using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmourController : MonoBehaviour
{
    private const int ArmourDivisions = 15;
    private const int MaxDivisionSize = 20;
    private readonly List<GameObject> _armourSegments = new List<GameObject>();
    private const int SegmentSpacing = 5;
    private HorizontalLayoutGroup _layoutGroup;

    // Use this for initialization
    public void Awake()
    {
        float segmentWidth = GetComponent<RectTransform>().rect.width - SegmentSpacing * (ArmourDivisions - 1);
        segmentWidth /= ArmourDivisions;
        if (segmentWidth > MaxDivisionSize) segmentWidth = MaxDivisionSize;
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
            layout.preferredWidth = segmentWidth;
            layout.preferredHeight = GetComponent<RectTransform>().rect.height;
            newSegment.name = "Armour Piece " + (i + 1);
            newSegment.SetActive(false);
            _armourSegments.Add(newSegment);
        }
    }
    
    public void SetArmourValue(int armourLevel)
    {
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