using UnityEngine;

namespace DefaultNamespace
{
    public abstract class ListElement
    {
        //either elementindex < 0 then hide
        //elementIndex >= length  then hide
        //element index >= 0 && < length then details

        private int _elementIndex;
        private Transform _elementTransform;
        private CanvasGroup _canvasGroup;

        public void Set(object o, bool isCentreItem)
        {
            if (o == null)
            {
                if (isCentreItem) UpdateCentreItemEmpty();
                else SetVisible(false);
                return;
            }

            _canvasGroup.alpha = isCentreItem ? 1 : 0.4f;
            SetVisible(true);
            Update(o);
        }

        protected abstract void UpdateCentreItemEmpty();

        public abstract void SetColour(Color colour);

        protected abstract void SetVisible(bool visible);

        public void SetElementTransform(Transform elementTransform)
        {
            _elementTransform = elementTransform;
            _canvasGroup = _elementTransform.GetComponent<CanvasGroup>();
            CacheUiElements(_elementTransform);
        }

        protected abstract void CacheUiElements(Transform transform);
        protected abstract void Update(object o);
    }
}