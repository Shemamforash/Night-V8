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

        public void Set(object o, bool isCentreItem)
        {
            if (o == null)
            {
                if (isCentreItem) UpdateCentreItemEmpty();
                else SetVisible(false);
                return;
            }

            SetVisible(true);
            Update(o);
        }

        protected abstract void UpdateCentreItemEmpty();

        public abstract void SetColour(Color colour);

        protected abstract void SetVisible(bool visible);

        public void SetElementTransform(Transform elementTransform)
        {
            _elementTransform = elementTransform;
            CacheUiElements(_elementTransform);
        }

        protected abstract void CacheUiElements(Transform transform);
        protected abstract void Update(object o);
    }
}