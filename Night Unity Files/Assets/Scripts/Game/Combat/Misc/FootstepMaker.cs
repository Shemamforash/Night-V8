﻿using System.Collections.Generic;
using SamsHelper;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class FootstepMaker : MonoBehaviour
    {
        private const float DistanceToFootPrint = 2f;
        private readonly List<GameObject> _footstepPool = new List<GameObject>();
        private float _distanceTravelled;
        private GameObject _footprintPrefab;

        private Transform _footstepParent;
        private Vector3 _lastPosition;
        private bool _leftLast;
        private Rigidbody2D _rigidBody;
        public bool UseHoofprint;
        [SerializeField]
        private AudioClip[] _audioClips;
        private AudioSource _audioSource;

        public void Awake()
        {
            if (_footstepParent == null) _footstepParent = GameObject.Find("World").transform.Find("Footsteps");
            _audioSource = GetComponent<AudioSource>();
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
            if(_rigidBody == null) _rigidBody = transform.parent.GetComponent<Rigidbody2D>();
            float zRotation = AdvancedMaths.AngleFromUp(Vector3.zero, _rigidBody.velocity);
            return Quaternion.Euler(new Vector3(0, 0, zRotation));
        }

        private void Update()
        {
            _distanceTravelled += Vector3.Distance(_lastPosition, transform.position);
            if (_distanceTravelled < DistanceToFootPrint) return;
            if (_leftLast)
            {
                GetNewFootprint().transform.Translate(Vector3.left * 0.03f);
                _leftLast = false;
            }
            else
            {
                GetNewFootprint().transform.Translate(Vector3.right * 0.03f);
                _leftLast = true;
            }
            if(_audioClips.Length != 0) _audioSource.PlayOneShot(Helper.RandomInList(_audioClips));
            _distanceTravelled = 0;
            _lastPosition = transform.position;
        }
    }
}