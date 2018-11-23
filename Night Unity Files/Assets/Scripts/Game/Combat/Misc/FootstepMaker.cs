using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Global;
using SamsHelper;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class FootstepMaker : MonoBehaviour
    {
        private const float TimeToFootPrint = 0.75f;
        private readonly List<GameObject> _footstepPool = new List<GameObject>();
        private float _timePassed;
        private GameObject _footprintPrefab;

        private Transform _footstepParent;
        private Vector3 _lastPosition;
        private bool _leftLast;
        private Rigidbody2D _rigidBody;
        public bool UseHoofprint;
        private int _nextClip;
        private AudioPoolController _audioPool;

        public void Awake()
        {
            if (_footstepParent == null) _footstepParent = GameObject.Find("World").transform.Find("Footsteps");
            _audioPool = GetComponent<AudioPoolController>();
            _audioPool.SetMixerGroup("Modified", 1);
            _nextClip = AudioClips.FootstepClips.Length + 1;
            _lastPosition = transform.position;
        }

        private void SetTransformAndRotation(GameObject footprintObject)
        {
            footprintObject.SetActive(true);
            footprintObject.transform.position = transform.position;
            footprintObject.transform.rotation = GetRotation();
        }

        private GameObject GetNewFootprint()
        {
            GameObject footprintObject;
            if (_footstepPool.Count == 0)
            {
                if (_footprintPrefab == null)
                {
                    _footprintPrefab = Resources.Load<GameObject>(UseHoofprint ? "Prefabs/Map/Hoofprint" : "Prefabs/Map/Footprint");
                }

                footprintObject = Instantiate(_footprintPrefab, transform.position, GetRotation());
                footprintObject.transform.SetParent(_footstepParent);
                FadeAndDie footprint = footprintObject.GetComponent<FadeAndDie>();
                footprint.SetLifeTime(2f, () =>
                {
                    _footstepPool.Add(footprint.gameObject);
                    footprint.gameObject.SetActive(false);
                });
            }
            else
            {
                footprintObject = _footstepPool[0];
                _footstepPool.RemoveAt(0);
                SetTransformAndRotation(footprintObject);
            }

            footprintObject.GetComponent<FadeAndDie>().StartFade();
            return footprintObject;
        }

        private Quaternion GetRotation()
        {
            if (_rigidBody == null) _rigidBody = transform.parent.GetComponent<Rigidbody2D>();
            float zRotation = AdvancedMaths.AngleFromUp(Vector3.zero, _rigidBody.velocity);
            return Quaternion.Euler(new Vector3(0, 0, zRotation));
        }

        private void PlayClip()
        {
            ++_nextClip;
            if (_nextClip >= AudioClips.FootstepClips.Length)
            {
                AudioClips.FootstepClips.Shuffle();
                _nextClip = 0;
            }

            AudioClip clip = AudioClips.FootstepClips[_nextClip];
            float volume = Random.Range(0.3f, 0.4f);
            float pitch = Random.Range(0.9f, 1f);
            _audioPool.Create().Play(clip, volume, pitch, 2000);
        }

        private void Update()
        {
            if (!CombatManager.IsCombatActive()) return;
            if (Vector2.Distance(_lastPosition, transform.position) < 0.25f) return;
            _timePassed += Time.deltaTime;
            if (_timePassed < TimeToFootPrint) return;
            _timePassed = 0f;
            _lastPosition = transform.position;
            PlaceFootprint();
            PlayClip();
        }

        private void PlaceFootprint()
        {
            Vector3 dir = _leftLast ? Vector3.left : Vector3.right;
            dir *= 0.03f;
            _leftLast = !_leftLast;
            GetNewFootprint().transform.Translate(dir);
        }
    }
}