using System.Collections;
using Game.Combat.Generation;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Game.Combat.Misc
{
    public abstract class BulletTrail : MonoBehaviour
    {
        private Transform _followTransform;

        public void SetTarget(Transform followTransform)
        {
            _followTransform = followTransform;
            transform.position = followTransform.position;
            ClearTrails();
        }

        public void StartFade()
        {
            StartCoroutine(Fade());
        }

        private IEnumerator Fade()
        {
            _followTransform = null;
            yield return null;
            while (!Done()) yield return null;
            ClearTrails();
            GetObjectPool().Return(this);
        }

        protected abstract bool Done();

        private void LateUpdate()
        {
            if (_followTransform == null) return;
            transform.position = _followTransform.transform.position;
        }

        private void OnDestroy()
        {
            GetObjectPool().Dispose(this);
        }

        protected abstract ObjectPool<BulletTrail> GetObjectPool();
        protected abstract void ClearTrails();

        public virtual void SetFinalPosition(Vector2 finalPosition)
        {
            transform.position = finalPosition;
            _followTransform = null;
        }
    }
}