using QuickEngine.Extensions;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Global.Tutorial
{
    public class TutorialOverlay
    {
        private float minY;
        private float minX;
        private float maxX, maxY;
        private readonly bool _centred;
        private readonly bool _needsRectOffset;

        public TutorialOverlay()
        {
            CalculateCoordinatesFromPosition(Vector2.zero, 20, 20);
            _centred = true;
        }

        public TutorialOverlay(Transform transform, float width, float height, Canvas canvas, Camera cam)
        {
            Vector2 position = AdvancedMaths.WorldToCanvasSpace(canvas, cam, transform);
            CalculateCoordinatesFromPosition(position, width, height);
            _needsRectOffset = true;
        }

        private void CalculateCoordinatesFromPosition(Vector2 position, float width, float height)
        {
            minX = position.x - width / 2f;
            minY = position.y - height / 2f;
            maxX = position.x + width / 2f;
            maxY = position.y + height / 2f;
        }

        public TutorialOverlay(RectTransform rectTransform, Canvas canvas, Camera cam)
        {
            Vector2 position = AdvancedMaths.ScreenToCanvasSpace(canvas, cam, rectTransform);
            float width = rectTransform.GetWidth();
            float height = rectTransform.GetHeight();
            CalculateCoordinatesFromPosition(position, width, height);
            _needsRectOffset = true;
        }

        public Vector2 OffsetMin()
        {
            return new Vector2(minX, minY);
        }

        public Vector2 OffsetMax(RectTransform tutorialRect)
        {
            if (_needsRectOffset)
            {
                maxY = tutorialRect.GetHeight() - maxY;
                maxX = tutorialRect.GetWidth() - maxX;
            }

            return new Vector2(-maxX, -maxY);
        }

        public bool Centred() => _centred;
    }
}