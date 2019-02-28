using System;
using Game.Combat.Player;
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
            Vector2 origin = _targetTransform.position;
            float halfWidth = _width / 2f;
            float halfHeight = _height / 2f;
            if (PlayerCombat.Instance != null) CalculateWorldCornersRelativeToPlayer(origin, halfWidth, halfHeight);
            else CalculateWorldCornersFromPoint(origin, halfWidth, halfHeight);
        }

        private void CalculateWorldCornersRelativeToPlayer(Vector2 origin, float halfWidth, float halfHeight)
        {
            Transform playerTransform = PlayerCombat.Instance.transform;
            Vector2 right = playerTransform.right;
            Vector2 up = playerTransform.forward;
            _worldCorners[0] = origin - right * halfWidth - up * halfHeight;
            _worldCorners[1] = origin - right * halfWidth + up * halfHeight;
            _worldCorners[2] = origin + right * halfWidth + up * halfHeight;
            _worldCorners[3] = origin + right * halfWidth - up * halfHeight;
        }

        private void CalculateWorldCornersFromPoint(Vector2 origin, float halfWidth, float halfHeight)
        {
            float x = origin.x;
            float y = origin.y;
            _worldCorners[0] = new Vector2(x - halfWidth, y - halfHeight);
            _worldCorners[1] = new Vector2(x - halfWidth, y + halfWidth);
            _worldCorners[2] = new Vector2(x + halfWidth, y + halfWidth);
            _worldCorners[3] = new Vector2(x + halfWidth, y - halfHeight);
        }

        private void CalculateNullWorldCorners()
        {
            Vector2 origin = PlayerCombat.Instance != null ? (Vector2) PlayerCombat.Position() : Vector2.zero;
            float halfWidth = 0.2f;
            float halfHeight = 0.2f;
            if (PlayerCombat.Instance != null) CalculateWorldCornersRelativeToPlayer(origin, halfWidth, halfHeight);
            else CalculateWorldCornersFromPoint(origin, halfWidth, halfHeight);
        }

        private void CalculateWorldCornersForRectTransform(RectTransform rectTransform)
        {
            rectTransform.GetWorldCorners(_worldCorners);
            _worldCorners[0].x -= 0.1f;
            _worldCorners[0].y -= 0.1f;

            _worldCorners[1].x -= 0.1f;
            _worldCorners[1].y += 0.1f;

            _worldCorners[2].x += 0.1f;
            _worldCorners[2].y += 0.1f;

            _worldCorners[3].x += 0.1f;
            _worldCorners[3].y -= 0.1f;
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
            Rect rect = canvasRectTransform.rect;
            float canvasWidth = rect.width;
            float canvasHeight = rect.height;

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