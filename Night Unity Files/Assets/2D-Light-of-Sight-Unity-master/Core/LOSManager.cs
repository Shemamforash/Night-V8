﻿using UnityEngine;
using System.Collections.Generic;

namespace LOS
{
    /// <summary>
    /// LOS manager is a singleton.
    /// It coordinates the system & provide common functions for others to use.
    /// </summary>
    [ExecuteInEditMode]
    public class LOSManager : MonoBehaviour
    {
        public PhysicsOpt physicsOpt;
        public float viewboxExtension = 1.01f;
        public LOSManager debugMode;

//		public float collidersExtension = 1.001f;

        public Vector2 halfViewboxSize => losCamera.halfViewboxSize;
        public bool is2D => physicsOpt == PhysicsOpt.Physics_2D;

        private static LOSManager _instance;

        private List<LOSLightBase> _lights = new List<LOSLightBase>();
        private Transform _losCameraTrans;
        private LOSCamera _losCamera;
        private bool _isDirty;


        public void Awake()
        {
            _instance = this;
            _losCamera = Camera.main.GetComponent<LOSCamera>();
        }
        
        /// <summary>
        /// Gets the instance of the singleton.
        /// </summary>
        /// <value>The instance.</value>
        public static LOSManager instance
        {
            get
            {
                if (_instance == null) _instance = FindObjectsOfType<LOSManager>()[0];
                return _instance;
            }
        }

        /// <summary>
        /// Tries the get instance.
        /// It is useful when you need to get the singleton near the end of an object's life cycle,
        /// 	as there are chances that it is the end of play mode, it's ok for the singleton to return null.
        /// </summary>
        /// <returns>The get instance.</returns>
        public static LOSManager TryGetInstance()
        {
            return _instance;
        }

        public List<LOSObstacle> obstacles { get; } = new List<LOSObstacle>();

        public List<LOSLightBase> lights => _lights;

        /// <summary>
        /// Gets the viewbox.
        /// Viewbox is the screen rect in world space.
        /// </summary>
        /// <value>The viewbox.</value>
        private List<LOSCamera.ViewBoxLine> viewbox => losCamera.viewbox;

        /// <summary>
        /// Gets the four viewbox corners.
        /// </summary>
        /// <value>The viewbox corners.</value>
        public List<Vector3> viewboxCorners => losCamera.viewboxCorners;

        public LOSCamera losCamera
        {
            get
            {
                if (_losCamera == null) _losCamera = Camera.main.GetComponent<LOSCamera>();
                return _losCamera;
            }
        }

        public Transform losCameraTrans
        {
            get
            {
                if (_losCameraTrans == null)
                {
                    _losCameraTrans = losCamera.transform;
                }

                return _losCameraTrans;
            }
        }


        void Start()
        {
            Init();
        }

        void LateUpdate()
        {
            UpdateLights();
        }

        void OnEnable()
        {
            foreach (var light in lights)
            {
                light.ToggleOnOff(true);
            }
        }

        void OnDisable()
        {
            foreach (var light in lights)
            {
                if (light != null)
                {
                    light.ToggleOnOff(false);
                }
            }
        }

        /// <summary>
        /// Updates the lights. 
        /// It is the place tells the lights to draw.
        /// </summary>
        public void UpdateLights()
        {
            if (losCamera.CheckDirty())
            {
                UpdateViewingBox();
            }

            foreach (var light in lights)
            {
                light.TryDraw();
            }

            UpdatePreviousInfo();
        }

        private void UpdatePreviousInfo()
        {
            _isDirty = false;

            losCamera.UpdatePreviousInfo();

            foreach (LOSObstacle obstacle in obstacles)
            {
                obstacle.UpdatePreviousInfo();
            }

//			UpdateViewingBox();
        }

        private void Init()
        {
            _instance = this;

            UpdateViewingBox();
        }

        private void UpdateViewingBox()
        {
            losCamera.UpdateViewingBox();
        }

        public void AddObstacle(LOSObstacle obstacle)
        {
            if (!obstacles.Contains(obstacle))
            {
                _isDirty = true;
                obstacles.Add(obstacle);
            }
        }

        public void RemoveObstacle(LOSObstacle obstacle)
        {
            obstacles.Remove(obstacle);
            _isDirty = true;
        }

        public void AddLight(LOSLightBase light)
        {
            if (!lights.Contains(light))
            {
                lights.Add(light);
            }
        }

        public void RemoveLight(LOSLightBase light)
        {
            lights.Remove(light);
        }

        public bool CheckDirty()
        {
            if (_isDirty) return true;

            bool result = false;
            foreach (LOSObstacle obstacle in obstacles)
            {
                if (!obstacle.isStatic && obstacle.CheckDirty())
                {
                    result = true;
                }
            }

            if (!Application.isPlaying)
            {
                UpdatePreviousInfo();
            }

            return result;
        }


        // ------------ Helper Functions ------------
        public Vector3 GetPointForRadius(Vector3 origin, Vector3 direction, float radius)
        {
            float c = direction.magnitude;

            float x = radius * direction.x / c + origin.x;
            float y = radius * direction.y / c + origin.y;
            return new Vector3(x, y, 0);
        }


        // ------------End------------


        public enum PhysicsOpt
        {
            Physics_3D,
            Physics_2D,
        }

//		private class ViewBoxLine {
//			public Vector2 start {get; set;}
//			public Vector2 end {get; set;}
//
//			public void SetStartEnd (Vector2 start, Vector2 end) {
//				this.start = start;
//				this.end = end;
//			}
//		}
    }
}