using System.Collections.Generic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Bosses
{
    public class TailFollowBehaviour : MonoBehaviour
    {
        [SerializeField] private float _parentDistance;
        [SerializeField] private bool _isHead;
        private TailFollowBehaviour _parent;
        private TailFollowBehaviour _child;
        private readonly Queue<Vector3> _positionList = new Queue<Vector3>();
        private Vector3 _lastPosition;
        private Rigidbody2D _parentRigidbody;

        public void Awake()
        {
            transform.position = Vector2.zero;
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
            {
                if (i == 0)
                {
                    tailSegments[i].SetParent(this);
                    continue;
                }

                tailSegments[i].SetParent(tailSegments[i - 1]);
            }
        }

        private void SetPosition(Vector2 position)
        {
            _positionList.Enqueue(position);
            if (_positionList.Count < 10) return;
            Vector3 newPosition = _positionList.Dequeue();
            if (!_isHead) newPosition += _parent.transform.up * _parentDistance;
            else newPosition += _parentRigidbody.transform.up * _parentDistance;
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