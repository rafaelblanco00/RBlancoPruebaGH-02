/*
 * Copyright (c) 2022, 2024 Qualcomm Technologies, Inc. and/or its subsidiaries.
 * All rights reserved.
 */

using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace Qualcomm.Snapdragon.Spaces.Samples
{
    public class MainMenuSampleController : SampleController
    {
        public GameObject ContentGameObject;
        public GameObject ComponentVersionsGameObject;
        public Transform ComponentVersionContent;
        public GameObject ComponentVersionPrefab;
        public ScrollRect ScrollRect;
        public Scrollbar VerticalScrollbar;
        public InputActionReference TouchpadInputAction;
        public GameObject ExtendedContext;
        public Toggle PassthroughToggle;
        private bool _instantiatedComponentVersions;

        public override void Start()
        {
            base.Start();
            if (!_baseRuntimeFeature)
            {
                Debug.LogWarning("Base Runtime Feature isn't available.");
                return;
            }

            if (!_baseRuntimeFeature.IsPassthroughSupported())
            {
                return;
            }

            _baseRuntimeFeature.OnPassthroughChangedEventDelegate += OnPassthroughChanged;

            ExtendedContext.SetActive(_baseRuntimeFeature.IsPassthroughSupported());

            PassthroughToggle.SetIsOnWithoutNotify(_isPassthroughOn);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            TouchpadInputAction.action.performed += OnTouchpadInput;
            _primaryButtonPressed += TogglePassthroughCheckbox;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            TouchpadInputAction.action.performed -= OnTouchpadInput;
            _primaryButtonPressed -= TogglePassthroughCheckbox;
        }

        public void OnInfoButtonPress()
        {
            SendHapticImpulse();
            ContentGameObject.SetActive(!ContentGameObject.activeSelf);
            ComponentVersionsGameObject.SetActive(!ComponentVersionsGameObject.activeSelf);
            if (!_instantiatedComponentVersions)
            {
                AddElementToComponentVersionScreen("Unity", String.Empty, Application.unityVersion, String.Empty);
                if (_baseRuntimeFeature != null)
                {
                    var componentVersions = _baseRuntimeFeature.ComponentVersions;
                    for (int i = 0; i < componentVersions.Count; i++)
                    {
                        AddElementToComponentVersionScreen(componentVersions[i].ComponentName, componentVersions[i].BuildIdentifier, componentVersions[i].VersionIdentifier, componentVersions[i].BuildDateTime);
                    }
                }

                _instantiatedComponentVersions = true;
            }
        }

        public void OnVerticalScrollViewChanged(float value)
        {
            SendHapticImpulse(frequency: 10f, duration: 0.1f);
            ScrollRect.verticalNormalizedPosition = Mathf.Clamp01(ScrollRect.verticalNormalizedPosition + (value * Time.deltaTime));
            VerticalScrollbar.value = ScrollRect.verticalNormalizedPosition;
        }

        private void OnTouchpadInput(InputAction.CallbackContext context)
        {
            var touchpadValue = context.ReadValue<Vector2>();
            if (touchpadValue.y > 0f)
            {
                OnVerticalScrollViewChanged(0.44f);
            }
            else
            {
                OnVerticalScrollViewChanged(-0.44f);
            }
        }

        private void TogglePassthroughCheckbox()
        {
            PassthroughToggle.SetIsOnWithoutNotify(_isPassthroughOn);
        }

        private void AddElementToComponentVersionScreen(string componentName, string buildId, string versionId, string buildDate)
        {
            var componentVersionObject = Instantiate(ComponentVersionPrefab, ComponentVersionContent);
            var componentVersionDisplay = componentVersionObject.GetComponent<ComponentVersionDisplay>();
            componentVersionDisplay.ComponentNameText = componentName;
            componentVersionDisplay.BuildIdentifierText = buildId;
            componentVersionDisplay.VersionIdentifierText = versionId;
            componentVersionDisplay.BuildDateTimeText = buildDate;
        }

        private void OnPassthroughChanged(XrEnvironmentBlendMode xrEnvironmentBlendMode)
        {
            Debug.Log("Passthrough state changed");
        }
    }
}
