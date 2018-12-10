using System.Collections;
using System.Collections.Generic;
using QuickEngine.Extensions;
using UnityEngine;
using UnityEngine.UI;

public class WorldToCanvasSpace : MonoBehaviour
{
    private Canvas _canvas;
    private CanvasScaler _scaler;

    private void Awake()
    {
        _canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        _scaler = _canvas.GetComponent<CanvasScaler>();
    }

    private void Update()
    {
        Vector2 mousePosition = Input.mousePosition;
        float targetWidth = _scaler.referenceResolution.x;
        float targetHeight = _scaler.referenceResolution.y;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        float canvasActualWidth = _canvas.GetComponent<RectTransform>().GetWidth();
        float canvasActualHeight = _canvas.GetComponent<RectTransform>().GetHeight();

        float ratio = canvasActualWidth / screenWidth;
        mousePosition.x *= ratio;

        Debug.Log(targetWidth + " " + screenWidth + " " + canvasActualWidth + " " + mousePosition.x);
    }
}