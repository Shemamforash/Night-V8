using QuickEngine.Extensions;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class WorldToCanvasSpace : MonoBehaviour
{
    private Canvas _canvas;
    private CanvasScaler _scaler;
    public TutorialOverlayController _tutorialOverlayController;

    private void Awake()
    {
        _canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        _scaler = _canvas.GetComponent<CanvasScaler>();
    }

    private void Update()
    {
        float leftOffset = 10;
        float rightOffset = 1020;
        float topOffset = 950;
        float bottomOffset = 35;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        float canvasActualWidth = _canvas.GetComponent<RectTransform>().GetWidth();
        float canvasActualHeight = _canvas.GetComponent<RectTransform>().GetHeight();

        float widthRatio = canvasActualWidth / 1920;
        float heightRatio = canvasActualHeight / 1080;
        Debug.Log(canvasActualHeight + " " + heightRatio + " " + widthRatio);
        leftOffset *= widthRatio;
        rightOffset *= widthRatio;
        topOffset *= heightRatio;
        bottomOffset *= heightRatio;

        Vector2 minOffset = new Vector2(leftOffset, topOffset);
        Vector2 maxOffset = new Vector2(rightOffset, bottomOffset);
        _tutorialOverlayController.SetTutorialArea(minOffset, maxOffset);
    }


//    private void Update()
//    {
//        GameObject currentObject = EventSystem.current.currentSelectedGameObject;if (currentObject == null) return;
//        RectTransform rect = currentObject.GetComponent<RectTransform>();
//        Vector2 size = Vector2.Scale(rect.rect.size, rect.lossyScale);
//        Rect screenRect = new Rect((Vector2) rect.position - size * 0.5f, size);
//        Vector2 topLeft = new Vector2(screenRect.xMin, screenRect.yMin);
//        Vector2 bottomRight = new Vector2(screenRect.xMax, screenRect.yMax);
//        Vector2 position = (topLeft + bottomRight) / 2f;
//        position = Camera.main.WorldToScreenPoint(position);
//        Debug.Log(position);
//
////        Vector2 position = Input.mousePosition;
//        float targetWidth = _scaler.referenceResolution.x;
//        float targetHeight = _scaler.referenceResolution.y;
//        float screenWidth = Screen.width;
//        float screenHeight = Screen.height;
//
//        float canvasActualWidth = _canvas.GetComponent<RectTransform>().GetWidth();
//        float canvasActualHeight = _canvas.GetComponent<RectTransform>().GetHeight();
//
//        position.x *= canvasActualWidth / screenWidth;
//        position.y *= canvasActualHeight / screenHeight;
//
//        Debug.Log(targetWidth + " " + screenWidth + " " + canvasActualWidth + " " + position.x + " " + position.y);
//        
//        
//    }
}