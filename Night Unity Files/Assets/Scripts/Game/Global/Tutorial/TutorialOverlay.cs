using System;
using Game.Combat.Player;
using QuickEngine.Extensions;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Global.Tutorial
{
    public class TutorialOverlay
    {
        private readonly bool _needsRectOffset;
        private readonly Transform _targetTransform;
        private readonly float _width, _height;
        private readonly bool _centred;
        private Vector3[] _worldCorners = new Vector3[4];

        public TutorialOverlay()
        {
            _centred = true;
        }

        public TutorialOverlay(Transform transform, float width, float height)
        {
            _targetTransform = transform;
            _width = width;
            _height = height;
        }

        public TutorialOverlay(RectTransform rectTransform)
        {
            _targetTransform = rectTransform;
        }

        private void CalculateWorldCornersForTransform()
        {
            Vector2 position = _targetTransform.position;
            float x = position.x;
            float y = position.y;
            float halfWidth = _width / 2f;
            float halfHeight = _height / 2f;
            _worldCorners[0] = new Vector2(x - halfWidth, y - halfHeight);
            _worldCorners[1] = new Vector2(x - halfWidth, y + halfWidth);
            _worldCorners[2] = new Vector2(x + halfWidth, y + halfWidth);
            _worldCorners[3] = new Vector2(x + halfWidth, y - halfHeight);
        }

        private void CalculateNullWorldCorners()
        {
            Vector2 origin = Vector2.zero;
            if (PlayerCombat.Instance != null)
            {
                origin = PlayerCombat.Instance.transform.position;
            }
            _worldCorners[0] = new Vector2(-0.2f, -0.2f) + origin;
            _worldCorners[1] = new Vector2(-0.2f, 0.2f) + origin;
            _worldCorners[2] = new Vector2(0.2f, 0.2f) + origin;
            _worldCorners[3] = new Vector2(0.2f, -0.2f) + origin;
        }

        private void CalculateWorldCornersForRectTransform(RectTransform rectTransform)
        {
            rectTransform.GetWorldCorners(_worldCorners);
        }

        public Tuple<Vector2, Vector2> GetMinMaxOffset(Canvas canvas)
        {
            if (_targetTransform == null)
                CalculateNullWorldCorners();
            else if (_targetTransform is RectTransform rectTransform)
                CalculateWorldCornersForRectTransform(rectTransform);
            else
                CalculateWorldCornersForTransform();

            RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
            float canvasWidth = canvasRectTransform.GetWidth();
            float canvasHeight = canvasRectTransform.GetHeight();

            _worldCorners = AdvancedMaths.WorldCornersToCanvasSpace(_worldCorners, canvasWidth, canvasHeight);

            float minX = _worldCorners[0].x;
            float maxY = _worldCorners[0].y;
            float maxX = canvasWidth - _worldCorners[2].x;
            float minY = canvasHeight - _worldCorners[2].y;

            Vector2 minOffset = new Vector2(minX, maxY);
            Vector2 maxOffset = new Vector2(-maxX, -minY);
            return Tuple.Create(minOffset, maxOffset);
        }

        public bool Centred() => _centred;
    }
}