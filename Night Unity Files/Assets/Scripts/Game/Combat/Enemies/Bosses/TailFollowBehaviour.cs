using System.Collections.Generic;
using DG.Tweening;
using SamsHelper.Libraries;
using Sirenix.Utilities;
using UnityEngine;

namespace Game.Combat.Enemies.Bosses
{
    public class TailFollowBehaviour : MonoBehaviour
    {
        [SerializeField] private bool _isHead;
        private TailFollowBehaviour _parent;
        private TailFollowBehaviour _child;
        private readonly Queue<Vector3> _positionList = new Queue<Vector3>();
        private Vector3 _lastPosition;
        private Rigidbody2D _parentRigidbody;
        private int ignoreCount;
        private readonly List<SpriteRenderer> _sprites = new List<SpriteRenderer>();

        public void Awake()
        {
            transform.position = Vector2.zero;
            CacheSprites();
            if (!_isHead) return;
            _parentRigidbody = transform.parent.GetComponent<Rigidbody2D>();
            List<TailFollowBehaviour> tailSegments = new List<TailFollowBehaviour>();
            int noChildren = transform.parent.childCount;
            for (int i = 0; i < noChildren; ++i)
            {
                Transform child = transform.parent.GetChild(i);
                TailFollowBehaviour tailFollow = child.GetComponent<TailFollowBehaviour>();
                if (tailFollow == null) continue;
                tailSegments.Add(tailFollow);
            }

            for (int i = 0; i < tailSegments.Count; ++i)
                tailSegments[i].SetParent(i == 0 ? this : tailSegments[i - 1]);
        }

        private void CacheSprites()
        {
            SpriteRenderer spriteOnTail = GetComponent<SpriteRenderer>();
            if (spriteOnTail != null) _sprites.Add(spriteOnTail);
            transform.GetComponentsInChildren<SpriteRenderer>().ForEach(s => _sprites.Add(s));
            _sprites.ForEach(s => s.SetAlpha(0f));
        }

        public TailFollowBehaviour GetChild()
        {
            return _child;
        }

        private void SetInactive()
        {
            gameObject.transform.position = Vector2.zero;
            if (_child == null) return;
            _child.SetInactive();
        }

        private void SetPosition(Vector2 position)
        {
            if (_positionList.Count == 6)
            {
                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(0.25f);
                _sprites.ForEach(s => sequence.Insert(0.5f, s.DOFade(1f, 1f)));
            }

            _positionList.Enqueue(position);
            if (_positionList.Count < 8)
            {
                SetInactive();
                return;
            }

            Vector3 newPosition = _positionList.Dequeue();
            transform.position = newPosition;
            if (_child == null) return;
            _child.SetPosition(_lastPosition);
            _lastPosition = newPosition;
        }

        private void FixedUpdate()
        {
            if (!_isHead) return;
            Vector3 position = _parentRigidbody.transform.position;
            SetPosition(position);
        }

        private void SetParent(TailFollowBehaviour parent)
        {
            _parent = parent;
            _parent._child = this;
        }
    }
}