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
        
        public void SetAlpha(float alpha)
        {
            Color c = GetColour();
            c.a = alpha;
            SetColour(c);
        }

        public void StartFade(float duration)
        {
            StartCoroutine(Fade(duration));
        }

        private IEnumerator Fade(float duration)
        {
            Color startColour = GetColour();
            float fadeTime = duration;
            while (duration > 0f)
            {
                if (!CombatManager.IsCombatActive()) yield return null;
                duration -= Time.deltaTime;
                Color lerpedColour = Color.Lerp(startColour, UiAppearanceController.InvisibleColour, 1f - duration / fadeTime);
                SetColour(lerpedColour);
                yield return null;
            }
            ClearTrails();
            GetObjectPool().Return(this);
        }
        
        public void LateUpdate()
        {
            transform.position = _followTransform.transform.position;
        }

        private void OnDestroy()
        {
            GetObjectPool().Dispose(this);
        }

        protected abstract ObjectPool<BulletTrail> GetObjectPool();
        protected abstract void ClearTrails();
        protected abstract Color GetColour();
        protected abstract void SetColour(Color color);
    }
}