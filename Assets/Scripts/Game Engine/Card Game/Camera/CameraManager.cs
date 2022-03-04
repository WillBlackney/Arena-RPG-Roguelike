﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace CardGameEngine
{
    public class CameraManager : Singleton<CameraManager>
    {
        // Properties + Component References
        #region
        [Header("Component References")]
        [SerializeField] private Camera mainCamera;
        [PropertySpace(SpaceBefore = 20, SpaceAfter = 0)]

        [Header("Small Shake Properties")]
        public float sMagnitude;
        public float sRoughness;
        public float sDuration;
        [PropertySpace(SpaceBefore = 20, SpaceAfter = 0)]

        [Header("Medium Shake Properties")]
        public float mMagnitude;
        public float mRoughness;
        public float mDuration;
        [PropertySpace(SpaceBefore = 20, SpaceAfter = 0)]

        [Header("Large Shake Properties")]
        public float lMagnitude;
        public float lRoughness;
        public float lDuration;
        [PropertySpace(SpaceBefore = 20, SpaceAfter = 0)]
        #endregion

        // Property Accessors
        #region
        public Camera MainCamera
        {
            get { return mainCamera; }
            private set { mainCamera = value; }
        }
        #endregion

        // Camera Shake Logic
        #region
        public void CreateCameraShake(CameraShakeType shakeType)
        {
            if (shakeType == CameraShakeType.Small)
            {
                CreateSmallCameraShake();
            }
            else if (shakeType == CameraShakeType.Medium)
            {
                CreateMediumCameraShake();
            }
            else if (shakeType == CameraShakeType.Large)
            {
                CreateLargeCameraShake();
            }
        }
        private void CreateSmallCameraShake()
        {
            CameraShaker.Instance.ShakeOnce(sMagnitude, sRoughness, 0.1f, sDuration);
        }
        private void CreateMediumCameraShake()
        {
            CameraShaker.Instance.ShakeOnce(mMagnitude, mRoughness, 0.1f, mDuration);
        }
        private void CreateLargeCameraShake()
        {
            CameraShaker.Instance.ShakeOnce(lMagnitude, lRoughness, 0.1f, lDuration);
        }
        #endregion

        // Camera Zoom
        #region
        public void DoCameraZoom(float startOrthoSize, float endOrthoSize, float duration)
        {
            MainCamera.orthographicSize = startOrthoSize;
            MainCamera.DOOrthoSize(endOrthoSize, duration);
        }
        public void DoCameraMove(float x, float y, float duration)
        {
            Vector3 destination = new Vector3(x, y, -15);
            MainCamera.transform.DOMove(destination, duration);
        }
        public void ResetMainCameraPositionAndZoom()
        {
            // Reset position
            MainCamera.transform.DOMove(new Vector3(0, 0, -15), 0f);

            // Reset Orthographic size
            MainCamera.orthographicSize = 5;
        }
        #endregion
    }
}