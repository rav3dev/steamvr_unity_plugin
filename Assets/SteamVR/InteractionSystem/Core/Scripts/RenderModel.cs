﻿ using UnityEngine;
using System.Collections;
using System;

namespace Valve.VR.InteractionSystem
{
    public class RenderModel : MonoBehaviour
    {
        public GameObject handPrefab;
        protected GameObject handInstance;
        protected Renderer[] handRenderers;
        public bool displayHandByDefault = true;
        protected SteamVR_Input_Skeleton handSkeleton;
        protected Animator handAnimator;

        protected string animatorParameterStateName = "AnimationState";
        protected int handAnimatorStateId = -1;

        [Space]

        public GameObject controllerPrefab;
        protected GameObject controllerInstance;
        protected Renderer[] controllerRenderers;
        protected SteamVR_RenderModel controllerRenderModel;
        public bool displayControllerByDefault = true;
        protected Material delayedSetMaterial;

        public event Action onControllerLoaded;

        protected SteamVR_Events.Action renderModelLoadedAction;

        protected void Awake()
        {
            renderModelLoadedAction = SteamVR_Events.RenderModelLoadedAction(OnRenderModelLoaded);

            if (handPrefab != null)
            {
                handInstance = GameObject.Instantiate(handPrefab);
                handInstance.transform.parent = this.transform;
                handInstance.transform.localPosition = Vector3.zero;
                handInstance.transform.localRotation = Quaternion.identity;
                handInstance.transform.localScale = handPrefab.transform.localScale;
                handSkeleton = handInstance.GetComponent<SteamVR_Input_Skeleton>();
                handSkeleton.updatePose = false;

                handRenderers = handInstance.GetComponentsInChildren<Renderer>();
                if (displayHandByDefault == false)
                    SetHandVisibility(false);

                handAnimator = handInstance.GetComponentInChildren<Animator>();
            }

            if (controllerPrefab != null)
            {
                controllerInstance = GameObject.Instantiate(controllerPrefab);
                controllerInstance.transform.parent = this.transform;
                controllerInstance.transform.localPosition = Vector3.zero;
                controllerInstance.transform.localRotation = Quaternion.identity;
                controllerInstance.transform.localScale = controllerPrefab.transform.localScale;
                controllerRenderModel = controllerInstance.GetComponent<SteamVR_RenderModel>();
            }
        }

        protected void OnEnable()
        {
            renderModelLoadedAction.enabled = true;
        }

        protected void OnDisable()
        {
            renderModelLoadedAction.enabled = false;
        }

        public virtual void OnHandInitialized(int deviceIndex)
        {
            controllerRenderModel.SetDeviceIndex(deviceIndex);
        }

        public void MatchHandToTransform(Transform match)
        {
            handInstance.transform.position = match.transform.position;
            handInstance.transform.rotation = match.transform.rotation;
        }

        public void SetHandPosition(Vector3 newPosition)
        {
            handInstance.transform.position = newPosition;
        }

        public void SetHandRotation(Quaternion newRotation)
        {
            handInstance.transform.rotation = newRotation;
        }

        public Vector3 GetHandPosition()
        {
            return handInstance.transform.position;
        }

        public Quaternion GetHandRotation()
        {
            return handInstance.transform.rotation;
        }

        private void OnRenderModelLoaded(SteamVR_RenderModel loadedRenderModel, bool success)
        {
            if (controllerRenderModel == loadedRenderModel)
            {
                controllerRenderers = controllerInstance.GetComponentsInChildren<Renderer>();
                if (displayControllerByDefault == false)
                    SetControllerVisibility(false);

                if (delayedSetMaterial != null)
                    SetControllerMaterial(delayedSetMaterial);

                if (onControllerLoaded != null)
                    onControllerLoaded.Invoke();
            }
        }

        public void SetVisibility(bool state, bool overrideDefault = false)
        {
            if (state == false || displayControllerByDefault || overrideDefault)
                SetControllerVisibility(state);

            if (state == false || displayHandByDefault || overrideDefault)
                SetHandVisibility(state);
        }

        public void Show(bool overrideDefault = false)
        {
            SetVisibility(true, overrideDefault);
        }

        public void Hide()
        {
            SetVisibility(false);
        }

        public virtual void SetMaterial(Material material)
        {
            SetControllerMaterial(material);
            SetHandMaterial(material);
        }

        public void SetControllerMaterial(Material material)
        {
            if (controllerRenderers == null)
            {
                delayedSetMaterial = material;
                return;
            }

            for (int rendererIndex = 0; rendererIndex < controllerRenderers.Length; rendererIndex++)
            {
                controllerRenderers[rendererIndex].material = material;
            }
        }

        public void SetHandMaterial(Material material)
        {
            for (int rendererIndex = 0; rendererIndex < handRenderers.Length; rendererIndex++)
            {
                handRenderers[rendererIndex].material = material;
            }
        }

        public void SetControllerVisibility(bool state, bool permanent = false)
        {
            if (permanent)
                displayControllerByDefault = state;

            if (controllerRenderers == null)
                return;

            for (int rendererIndex = 0; rendererIndex < controllerRenderers.Length; rendererIndex++)
            {
                controllerRenderers[rendererIndex].enabled = state;
            }
        }

        public void SetHandVisibility(bool state, bool permanent = false)
        {
            if (permanent)
                displayHandByDefault = state;

            if (handRenderers == null)
                return;

            for (int rendererIndex = 0; rendererIndex < handRenderers.Length; rendererIndex++)
            {
                handRenderers[rendererIndex].enabled = state;
            }
        }


        public void SetSkeletonRangeOfMotion(EVRSkeletalMotionRange newRangeOfMotion, float blendOverSeconds = 0.1f)
        {
            if (handSkeleton != null)
            {
                handSkeleton.SetRangeOfMotion(newRangeOfMotion, blendOverSeconds);
            }
        }

        public EVRSkeletalMotionRange GetSkeletonRangeOfMotion
        {
            get
            {
                return handSkeleton.rangeOfMotion;
            }
        }

        public void SetTemporarySkeletonRangeOfMotion(SkeletalMotionRangeChange temporaryRangeOfMotionChange, float blendOverSeconds = 0.1f)
        {
            if (handSkeleton != null)
            {
                handSkeleton.SetTemporaryRangeOfMotion((EVRSkeletalMotionRange)temporaryRangeOfMotionChange, blendOverSeconds);
            }
        }

        public void ResetTemporarySkeletonRangeOfMotion(float blendOverSeconds = 0.1f)
        {
            if (handSkeleton != null)
            {
                handSkeleton.ResetTemporaryRangeOfMotion(blendOverSeconds);
            }
        }

        public void SetAnimationState(int stateValue)
        {
            if (handSkeleton != null)
            {
                if (handSkeleton.isBlending == false)
                    handSkeleton.BlendToAnimation();

                if (CheckAnimatorInit())
                    handAnimator.SetInteger(handAnimatorStateId, stateValue);
            }
        }

        public void StopAnimation()
        {
            if (handSkeleton != null)
            {
                if (handSkeleton.isBlending == false)
                    handSkeleton.BlendToSkeleton();

                if (CheckAnimatorInit())
                    handAnimator.SetInteger(handAnimatorStateId, 0);
            }
        }

        private bool CheckAnimatorInit()
        {
            if (handAnimatorStateId == -1 && handAnimator != null)
            {
                if (handAnimator.gameObject.activeInHierarchy && handAnimator.isInitialized)
                {
                    AnimatorControllerParameter[] parameters = handAnimator.parameters;
                    for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
                    {
                        if (string.Equals(parameters[parameterIndex].name, animatorParameterStateName, StringComparison.CurrentCultureIgnoreCase))
                            handAnimatorStateId = parameters[parameterIndex].nameHash;
                    }
                }
            }

            return handAnimatorStateId != -1 && handAnimator != null && handAnimator.isInitialized;
        }

        
    }
}