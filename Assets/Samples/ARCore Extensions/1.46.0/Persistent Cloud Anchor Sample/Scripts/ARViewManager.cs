//-----------------------------------------------------------------------
// <copyright file="ARViewManager.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// A manager component that helps with hosting and resolving Cloud Anchors.
    /// </summary>
    public class ARViewManager : MonoBehaviour
    {
        /// <summary>
        /// The main controller for Persistent Cloud Anchors sample.
        /// </summary>
        public PersistentCloudAnchorsController Controller;

        /// <summary>
        /// The 3D object that represents a Cloud Anchor.
        /// </summary>
        public GameObject CloudAnchorPrefab;
        
        public GameObject HeartCloudAnchorPrefab;
        
        public GameObject LaughingCloudAnchorPrefab;
       
        public GameObject StarsCloudAnchorPrefab;
       
        public GameObject PartyCloudAnchorPrefab;
        
        public GameObject ThinkingCloudAnchorPrefab;
        

        // Tracks if we are currently dragging (finger is down).
        private bool _isDragging = false;
        private bool _isRotating = false;

        // A temporary preview object that follows the finger as it moves.
        private GameObject _previewObject = null;
        private float _initialRotationAngle;
        private float _currentRotationAngle;
        private const float MIN_PINCH_DISTANCE = 50f;


        /// <summary>
        /// The game object that includes <see cref="MapQualityIndicator"/> to visualize
        /// map quality result.
        /// </summary>
        public GameObject MapQualityIndicatorPrefab;

        /// <summary>
        /// The UI element that displays the instructions to guide hosting experience.
        /// </summary>
        public GameObject InstructionBar;

        /// <summary>
        /// The UI panel that allows the user to name the Cloud Anchor.
        /// </summary>
        public GameObject NamePanel;

        /// <summary>
        /// The UI element that displays warning message for invalid input name.
        /// </summary>
        public GameObject InputFieldWarning;

        /// <summary>
        /// The input field for naming Cloud Anchor.
        /// </summary>
        public InputField NameField;

        /// <summary>
        /// The instruction text in the top instruction bar.
        /// </summary>
        public Text InstructionText;

        /// <summary>
        /// Display the tracking helper text when the session in not tracking.
        /// </summary>
        public Text TrackingHelperText;

        /// <summary>
        /// The debug text in bottom snack bar.
        /// </summary>
        public Text DebugText;
        

        /// <summary>
        /// The button to save the typed name.
        /// </summary>
        public Button SaveButton;

        /// <summary>
        /// The button to save current cloud anchor id into clipboard.
        /// </summary>
        public Button ShareButton;

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.Initializing">.</see>
        /// </summary>
        private const string _initializingMessage = "Tracking is being initialized.";

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.Relocalizing">.</see>
        /// </summary>
        private const string _relocalizingMessage = "Tracking is resuming after an interruption.";

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.InsufficientLight">.</see>
        /// </summary>
        private const string _insufficientLightMessage = "Too dark. Try moving to a well-lit area.";

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.InsufficientLight">
        /// in Android S or above.</see>
        /// </summary>
        private const string _insufficientLightMessageAndroidS =
            "Too dark. Try moving to a well-lit area. " +
            "Also, make sure the Block Camera is set to off in system settings.";

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.InsufficientFeatures">.</see>
        /// </summary>
        private const string _insufficientFeatureMessage =
            "Can't find anything. Aim device at a surface with more texture or color.";

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.ExcessiveMotion">.</see>
        /// </summary>
        private const string _excessiveMotionMessage = "Moving too fast. Slow down.";

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.Unsupported">.</see>
        /// </summary>
        private const string _unsupportedMessage = "Tracking lost reason is not supported.";

        /// <summary>
        /// The time between enters AR View and ARCore session starts to host or resolve.
        /// </summary>
        private const float _startPrepareTime = 3.0f;

        /// <summary>
        /// Android 12 (S) SDK version.
        /// </summary>
        private const int _androidSSDKVesion = 31;

        /// <summary>
        /// Pixel Model keyword.
        /// </summary>
        private const string _pixelModel = "pixel";

        /// <summary>
        /// The timer to indicate whether the AR View has passed the start prepare time.
        /// </summary>
        private float _timeSinceStart;

        /// <summary>
        /// True if the app is in the process of returning to home page due to an invalid state,
        /// otherwise false.
        /// </summary>
        private bool _isReturning;

        /// <summary>
        /// The MapQualityIndicator that attaches to the placed object.
        /// </summary>
        private MapQualityIndicator _qualityIndicator = null;

        /// <summary>
        /// The history data that represents the current hosted Cloud Anchor.
        /// </summary>
        private CloudAnchorHistory _hostedCloudAnchor;

        /// <summary>
        /// An ARAnchor indicating the 3D object has been placed on a flat surface and
        /// is waiting for hosting.
        /// </summary>
        private ARAnchor _anchor = null;

        /// <summary>
        /// The promise for the async hosting operation, if any.
        /// </summary>
        private HostCloudAnchorPromise _hostPromise = null;

        /// <summary>
        /// The result of the hosting operation, if any.
        /// </summary>
        private HostCloudAnchorResult _hostResult = null;

        /// <summary>
        /// The coroutine for the hosting operation, if any.
        /// </summary>
        private IEnumerator _hostCoroutine = null;

        /// <summary>
        /// The promises for the async resolving operations, if any.
        /// </summary>
        private List<ResolveCloudAnchorPromise> _resolvePromises =
            new List<ResolveCloudAnchorPromise>();

        /// <summary>
        /// The results of the resolving operations, if any.
        /// </summary>
        private List<ResolveCloudAnchorResult> _resolveResults =
            new List<ResolveCloudAnchorResult>();

        /// <summary>
        /// The coroutines of the resolving operations, if any.
        /// </summary>
        private List<IEnumerator> _resolveCoroutines = new List<IEnumerator>();

        private Color _activeColor;
        private AndroidJavaClass _versionInfo;

        /// <summary>
        /// Get the camera pose for the current frame.
        /// </summary>
        /// <returns>The camera pose of the current frame.</returns>
        public Pose GetCameraPose()
        {
            return new Pose(Controller.MainCamera.transform.position,
                Controller.MainCamera.transform.rotation);
        }
        public void ReplaceCloudAnchorPrefab(string prefabName)
        {
            // Check if an alternative prefab is assigned
            if (prefabName == "LaughingCloudAnchor" && LaughingCloudAnchorPrefab != null)
            {
                CloudAnchorPrefab = LaughingCloudAnchorPrefab;
                Debug.Log("Laughing Cloud Anchor Prefab replaced.");
            }
            else if (prefabName == "ThinkingCloudAnchor" && ThinkingCloudAnchorPrefab != null)
            {
                CloudAnchorPrefab = ThinkingCloudAnchorPrefab;
                Debug.Log(" Thinking Cloud Anchor Prefab replaced.");

            }
            else if (prefabName == "StarsCloudAnchor" && StarsCloudAnchorPrefab != null)
            {
                CloudAnchorPrefab = StarsCloudAnchorPrefab;
                Debug.Log(" Stars Cloud Anchor Prefab replaced.");

            }
            else if (prefabName == "PartyCloudAnchor" && PartyCloudAnchorPrefab != null)
            {
                CloudAnchorPrefab = PartyCloudAnchorPrefab;
                Debug.Log(" Stars Cloud Anchor Prefab replaced.");
            }
            else if (prefabName == "HeartCloudAnchor" && PartyCloudAnchorPrefab != null)
            {
                CloudAnchorPrefab = HeartCloudAnchorPrefab;
                Debug.Log(" Heart Cloud Anchor Prefab replaced.");
            }
            else
            {
                Debug.LogWarning("No valid Cloud Anchor Prefab has been assigned for: " + prefabName);
            }
        }

        /// <summary>
        /// Callback handling the validation of the input field.
        /// </summary>
        /// <param name="inputString">The current value of the input field.</param>
        public void OnInputFieldValueChanged(string inputString)
        {
            // Cloud Anchor name should only contains: letters, numbers, hyphen(-), underscore(_).
            var regex = new Regex("^[a-zA-Z0-9-_]*$");
            InputFieldWarning.SetActive(!regex.IsMatch(inputString));
            SetSaveButtonActive(!InputFieldWarning.activeSelf && inputString.Length > 0);
        }

        /// <summary>
        /// Callback handling "Ok" button click event for input field.
        /// </summary>
        public void OnSaveButtonClicked()
        {
            _hostedCloudAnchor.Name = NameField.text;
            Controller.SaveCloudAnchorHistory(_hostedCloudAnchor);

            DebugText.text = string.Format("Saved Cloud Anchor:\n{0}.", _hostedCloudAnchor.Name);
            ShareButton.gameObject.SetActive(true);
            NamePanel.SetActive(false);
        }

        /// <summary>
        /// Callback handling "Share" button click event.
        /// </summary>
        public void OnShareButtonClicked()
        {
            GUIUtility.systemCopyBuffer = _hostedCloudAnchor.Id;
            DebugText.text = "Copied cloud id: " + _hostedCloudAnchor.Id;
            ShareButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            _activeColor = SaveButton.GetComponentInChildren<Text>().color;
            _versionInfo = new AndroidJavaClass("android.os.Build$VERSION");
        }

        /// <summary>
        /// The Unity OnEnable() method.
        /// </summary>
        public void OnEnable()
        {
            _timeSinceStart = 0.0f;
            _isReturning = false;
            _anchor = null;
            _qualityIndicator = null;
            _hostPromise = null;
            _hostResult = null;
            _hostCoroutine = null;
            _resolvePromises.Clear();
            _resolveResults.Clear();
            _resolveCoroutines.Clear();

            InstructionBar.SetActive(true);
            NamePanel.SetActive(false);
            InputFieldWarning.SetActive(false);
            ShareButton.gameObject.SetActive(false);
            UpdatePlaneVisibility(true);

            switch (Controller.Mode)
            {
                case PersistentCloudAnchorsController.ApplicationMode.Ready:
                    ReturnToHomePage("Invalid application mode, returning to home page...");
                    break;
                case PersistentCloudAnchorsController.ApplicationMode.Hosting:
                case PersistentCloudAnchorsController.ApplicationMode.Resolving:
                    InstructionText.text = "Detecting flat surface...";
                    DebugText.text = "ARCore is preparing for " + Controller.Mode;
                    break;
            }
        }

        /// <summary>
        /// The Unity OnDisable() method.
        /// </summary>
        public void OnDisable()
        {
            _isDragging = false;
            _isRotating = false;
            if (_previewObject != null)
            {
                Destroy(_previewObject);
                _previewObject = null;
            }
            if (_qualityIndicator != null)
            {
                Destroy(_qualityIndicator.gameObject);
                _qualityIndicator = null;
            }

            if (_anchor != null)
            {
                Destroy(_anchor.gameObject);
                _anchor = null;
            }

            if (_hostCoroutine != null)
            {
                StopCoroutine(_hostCoroutine);
            }

            _hostCoroutine = null;

            if (_hostPromise != null)
            {
                _hostPromise.Cancel();
                _hostPromise = null;
            }

            _hostResult = null;

            foreach (var coroutine in _resolveCoroutines)
            {
                StopCoroutine(coroutine);
            }

            _resolveCoroutines.Clear();

            foreach (var promise in _resolvePromises)
            {
                promise.Cancel();
            }

            _resolvePromises.Clear();

            foreach (var result in _resolveResults)
            {
                if (result.Anchor != null)
                {
                    Destroy(result.Anchor.gameObject);
                }
            }
            if (_hostCoroutine != null)
            {
                StopCoroutine(_hostCoroutine);
                _hostCoroutine = null;
            }

            _resolveResults.Clear();
            UpdatePlaneVisibility(false);
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            // Give ARCore some time to prepare for hosting or resolving
            if (_timeSinceStart < _startPrepareTime)
            {
                _timeSinceStart += Time.deltaTime;
                if (_timeSinceStart >= _startPrepareTime)
                {
                    UpdateInitialInstruction();
                }
                return;
            }

            // Check AR Core lifecycle
            ARCoreLifecycleUpdate();
            if (_isReturning)
            {
                return;
            }

            if (_timeSinceStart >= _startPrepareTime)
            {
                DisplayTrackingHelperMessage();
            }

            // Handle different modes
            if (Controller.Mode == PersistentCloudAnchorsController.ApplicationMode.Resolving)
            {
                ResolvingCloudAnchors();
            }
            else if (Controller.Mode == PersistentCloudAnchorsController.ApplicationMode.Hosting)
            {
                // Handle touch input based on number of fingers
                if (Input.touchCount == 1)
                {
                    HandleSingleTouch(Input.GetTouch(0));
                }
                else if (Input.touchCount == 2)
                {
                    HandleDoubleTouch(Input.GetTouch(0), Input.GetTouch(1));
                }
                else if (Input.touchCount == 0)
                {
                    // Reset states when no touches are detected
                    if (_isDragging || _isRotating)
                    {
                        FinalizePlacement();
                    }
                }

                // Continue with cloud anchor hosting if anchor exists
                if (_anchor != null)
                {
                    HostingCloudAnchor();
                }
            }
        }
 private void HandleSingleTouch(Touch touch)
    {
        // Ignore the touch if it's pointing on UI objects
        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
        {
            return;
        }

        switch (touch.phase)
        {
            case TouchPhase.Began:
                // Start dragging if no anchor exists
                if (_anchor == null && !_isDragging)
                {
                    _isDragging = true;
                    HandleDragStart(touch.position);
                }
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                // Update position while dragging
                if (_isDragging && _previewObject != null)
                {
                    UpdateDragPosition(touch.position);
                }
                break;

            case TouchPhase.Ended:
                // Finalize placement when drag ends
                if (_isDragging)
                {
                    _isDragging = false;
                    FinalizePlacement(touch.position);
                }
                break;
        }
    }

    private void HandleDragStart(Vector2 touchPos)
    {
        List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
        if (Controller.RaycastManager.Raycast(touchPos, hitResults, TrackableType.PlaneWithinPolygon))
        {
            // Create preview object at hit position
            var hitPose = hitResults[0].pose;
            _previewObject = Instantiate(CloudAnchorPrefab, hitPose.position, hitPose.rotation);
        }
    }

    private void UpdateDragPosition(Vector2 touchPos)
    {
        if (_isRotating) return; // Don't update position while rotating

        List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
        if (Controller.RaycastManager.Raycast(touchPos, hitResults, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = hitResults[0].pose;
            _previewObject.transform.position = hitPose.position;
            
            // Only update rotation to face camera if we're not in the middle of a manual rotation
            if (!_isRotating)
            {
                Vector3 cameraPosXZ = new Vector3(
                    Controller.MainCamera.transform.position.x,
                    _previewObject.transform.position.y,
                    Controller.MainCamera.transform.position.z
                );
                Vector3 lookDirection = cameraPosXZ - _previewObject.transform.position;
                if (lookDirection != Vector3.zero)
                {
                    _previewObject.transform.rotation = Quaternion.LookRotation(-lookDirection, Vector3.up);
                }
            }
        }
    }

    private void HandleDoubleTouch(Touch touch0, Touch touch1)
    {
        // Only handle rotation if we have a preview object or anchor
        GameObject targetObject = _previewObject != null ? _previewObject : 
                                (_anchor != null ? _anchor.gameObject : null);
        
        if (targetObject == null) return;

        switch (touch0.phase)
        {
            case TouchPhase.Began:
                StartRotation(touch0, touch1, targetObject);
                break;

            case TouchPhase.Moved:
                if (_isRotating)
                {
                    UpdateRotation(touch0, touch1, targetObject);
                }
                break;

            case TouchPhase.Ended:
                _isRotating = false;
                break;
        }
    }
    private Vector2 _previousTouchPosition;
    private Vector3 _initialRotation;

    private void StartRotation(Touch touch0, Touch touch1, GameObject targetObject)
    {
        // Calculate initial angle between two fingers
        Vector2 initialVector = touch1.position - touch0.position;
        
        // Only start rotation if fingers are far enough apart
        if (initialVector.magnitude >= MIN_PINCH_DISTANCE)
        {
            _isRotating = true;
            _previousTouchPosition = (touch0.position + touch1.position) * 0.5f; // Center point
            _initialRotation = targetObject.transform.eulerAngles;
        }
    }

    private void UpdateRotation(Touch touch0, Touch touch1, GameObject targetObject)
    {
        Vector2 currentVector = touch1.position - touch0.position;
        
        // Only update rotation if fingers are far enough apart
        if (currentVector.magnitude >= MIN_PINCH_DISTANCE)
        {
            Vector2 currentTouchPosition = (touch0.position + touch1.position) * 0.5f;
        
            // Calculate the difference in position from last frame
            Vector2 deltaPosition = currentTouchPosition - _previousTouchPosition;
        
            // Calculate new rotation
            Vector3 newRotation = targetObject.transform.eulerAngles;
        
            // Y-axis rotation (left/right) based on horizontal movement
            newRotation.y += deltaPosition.x * 0.5f; // Adjust mult
            float newXRotation = newRotation.x - deltaPosition.y * 0.5f;
            if (newXRotation > 180f) newXRotation -= 360f;
            newXRotation = Mathf.Clamp(newXRotation, -60f, 60f); // Limit rotation range
            newRotation.x = newXRotation;
            
            targetObject.transform.eulerAngles = newRotation;

            _previousTouchPosition = currentTouchPosition;
        }
    }

    private void FinalizePlacement(Vector2? touchPos = null)
    {
        if (touchPos.HasValue && !_isRotating)
        {
            List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
            if (Controller.RaycastManager.Raycast(touchPos.Value, hitResults, TrackableType.PlaneWithinPolygon))
            {
                var hitPose = hitResults[0].pose;
                ARPlane plane = Controller.PlaneManager.GetPlane(hitResults[0].trackableId);
                
                if (plane != null)
                {
                    // Create the actual anchor with the current rotation of the preview object
                    hitPose.rotation = _previewObject.transform.rotation;
                    _anchor = Controller.AnchorManager.AttachAnchor(plane, hitPose);
                    
                    if (_anchor != null)
                    {
                        // Move preview object to anchor
                        _previewObject.transform.SetParent(_anchor.transform, true);
                        
                        // Attach map quality indicator
                        var indicatorGO = Instantiate(MapQualityIndicatorPrefab, _anchor.transform);
                        _qualityIndicator = indicatorGO.GetComponent<MapQualityIndicator>();
                        _qualityIndicator.DrawIndicator(plane.alignment, Controller.MainCamera);

                        InstructionText.text = "To save this location, walk around the object to capture it from different angles";
                        DebugText.text = "Waiting for sufficient mapping quality...";

                        // Hide plane generator
                        UpdatePlaneVisibility(false);
                    }
                }
            }
            
            // Clean up preview object if anchor creation failed
            if (_anchor == null)
            {
                Destroy(_previewObject);
            }
        }
        
        // Reset states
        _isDragging = false;
        _isRotating = false;
        _previewObject = null;
    }
    private void FinalizePlacement(Vector2 touchPos)
    {
        List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
        if (Controller.RaycastManager.Raycast(touchPos, hitResults, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = hitResults[0].pose;
            ARPlane plane = Controller.PlaneManager.GetPlane(hitResults[0].trackableId);
            
            if (plane != null)
            {
                // Create the actual anchor
                _anchor = Controller.AnchorManager.AttachAnchor(plane, hitPose);
                
                if (_anchor != null)
                {
                    // Move preview object to anchor
                    _previewObject.transform.SetParent(_anchor.transform, true);
                    
                    // Attach map quality indicator
                    var indicatorGO = Instantiate(MapQualityIndicatorPrefab, _anchor.transform);
                    _qualityIndicator = indicatorGO.GetComponent<MapQualityIndicator>();
                    _qualityIndicator.DrawIndicator(plane.alignment, Controller.MainCamera);

                    InstructionText.text = "To save this location, walk around the object to capture it from different angles";
                    DebugText.text = "Waiting for sufficient mapping quality...";

                    // Hide plane generator
                    UpdatePlaneVisibility(false);
                }
            }
        }
        
        // Clean up preview object if anchor creation failed
        if (_anchor == null)
        {
            Destroy(_previewObject);
        }
        _previewObject = null;
    }

        /// <summary>
/// Resolve all cloud anchors in the ResolvingSet when a button is pressed.
                /// </summary>
      
        private void PerformHitTest(Vector2 touchPos)
        {
            List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
            Controller.RaycastManager.Raycast(
                touchPos, hitResults, TrackableType.PlaneWithinPolygon);

            // If there was an anchor placed, then instantiate the corresponding object.
            var planeType = PlaneAlignment.HorizontalUp;
            if (hitResults.Count > 0)
            {
                ARPlane plane = Controller.PlaneManager.GetPlane(hitResults[0].trackableId);
                if (plane == null)
                {
                    Debug.LogWarningFormat("Failed to find the ARPlane with TrackableId {0}",
                        hitResults[0].trackableId);
                    return;
                }

                planeType = plane.alignment;
                var hitPose = hitResults[0].pose;
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    // Point the hitPose rotation roughly away from the raycast/camera
                    // to match ARCore.
                    hitPose.rotation.eulerAngles =
                        new Vector3(0.0f, Controller.MainCamera.transform.eulerAngles.y, 0.0f);
                }

                _anchor = Controller.AnchorManager.AttachAnchor(plane, hitPose);
            }

            if (_anchor != null)
            {
                // Log before instantiation
                Debug.Log("Instantiating Cloud Anchor Prefab.");
                Instantiate(CloudAnchorPrefab, _anchor.transform);

                // Attach map quality indicator to this anchor.
                var indicatorGO =
                    Instantiate(MapQualityIndicatorPrefab, _anchor.transform);
                _qualityIndicator = indicatorGO.GetComponent<MapQualityIndicator>();
                _qualityIndicator.DrawIndicator(planeType, Controller.MainCamera);

                InstructionText.text = " To save this location, walk around the object to " +
                    "capture it from different angles";
                DebugText.text = "Waiting for sufficient mapping quaility...";

                // Hide plane generator so users can focus on the object they placed.
                UpdatePlaneVisibility(false);
            }
        }

        private void HostingCloudAnchor()
        {
            // There is no anchor for hosting.
            if (_anchor == null)
            {
                return;
            }

            // There is a pending or finished hosting task.
            if (_hostPromise != null || _hostResult != null)
            {
                return;
            }

            // Update map quality:
            int qualityState = 2;
            // Can pass in ANY valid camera pose to the mapping quality API.
            // Ideally, the pose should represent users’ expected perspectives.
            FeatureMapQuality quality =
                Controller.AnchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose());
            DebugText.text = "Current mapping quality: " + quality;
            qualityState = (int)quality;
            _qualityIndicator.UpdateQualityState(qualityState);

            // Hosting instructions:
            var cameraDist = (_qualityIndicator.transform.position -
                Controller.MainCamera.transform.position).magnitude;
            if (cameraDist < _qualityIndicator.Radius * 1.5f)
            {
                InstructionText.text = "You are too close, move backward.";
                return;
            }
            else if (cameraDist > 10.0f)
            {
                InstructionText.text = "You are too far, come closer.";
                return;
            }
            else if (_qualityIndicator.ReachTopviewAngle)
            {
                InstructionText.text =
                    "You are looking from the top view, move around from all sides.";
                return;
            }
            else if (!_qualityIndicator.ReachQualityThreshold)
            {
                InstructionText.text = "Save the object here by capturing it from all sides.";
                return;
            }

            // Start hosting:
            InstructionText.text = "Processing...";
            DebugText.text = "Mapping quality has reached sufficient threshold, " +
                "creating Cloud Anchor.";
            DebugText.text = string.Format(
                "FeatureMapQuality has reached {0}, triggering CreateCloudAnchor.",
                Controller.AnchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose()));

            // Creating a Cloud Anchor with lifetime = 1 day.
            // This is configurable up to 365 days when keyless authentication is used.
            var promise = Controller.AnchorManager.HostCloudAnchorAsync(_anchor, 1);
            if (promise.State == PromiseState.Done)
            {
                Debug.LogFormat("Failed to host a Cloud Anchor.");
                OnAnchorHostedFinished(false);
            }
            else
            {
                _hostPromise = promise;
                _hostCoroutine = HostAnchor();
                StartCoroutine(_hostCoroutine);
            }
        }

        private IEnumerator HostAnchor()
        {
            yield return _hostPromise;
            _hostResult = _hostPromise.Result;
            _hostPromise = null;

            if (_hostResult.CloudAnchorState == CloudAnchorState.Success)
            {
                int count = Controller.LoadCloudAnchorHistory().Collection.Count;
                _hostedCloudAnchor =
                    new CloudAnchorHistory("CloudAnchor" + count, _hostResult.CloudAnchorId);
                OnAnchorHostedFinished(true, _hostResult.CloudAnchorId);
            }
            else
            {
                OnAnchorHostedFinished(false, _hostResult.CloudAnchorState.ToString());
            }
        }

        private void ResolvingCloudAnchors()
        {
            // No Cloud Anchor for resolving.
            if (Controller.ResolvingSet.Count == 0)
            {
                return;
            }

            // There are pending or finished resolving tasks.
            if (_resolvePromises.Count > 0 || _resolveResults.Count > 0)
            {
                return;
            }

            // ARCore session is not ready for resolving.
            if (ARSession.state != ARSessionState.SessionTracking)
            {
                return;
            }

            Debug.LogFormat("Attempting to resolve {0} Cloud Anchor(s): {1}",
                Controller.ResolvingSet.Count,
                string.Join(",", new List<string>(Controller.ResolvingSet).ToArray()));
            foreach (string cloudId in Controller.ResolvingSet)
            {
                var promise = Controller.AnchorManager.ResolveCloudAnchorAsync(cloudId);
                if (promise.State == PromiseState.Done)
                {
                    Debug.LogFormat("Faild to resolve Cloud Anchor " + cloudId);
                    OnAnchorResolvedFinished(false, cloudId);
                }
                else
                {
                    _resolvePromises.Add(promise);
                    var coroutine = ResolveAnchor(cloudId, promise);
                    StartCoroutine(coroutine);
                }
            }

            Controller.ResolvingSet.Clear();
        }

        private IEnumerator ResolveAnchor(string cloudId, ResolveCloudAnchorPromise promise)
        {
            yield return promise;
            var result = promise.Result;
            _resolvePromises.Remove(promise);
            _resolveResults.Add(result);

            if (result.CloudAnchorState == CloudAnchorState.Success)
            {
                OnAnchorResolvedFinished(true, cloudId);
                Instantiate(CloudAnchorPrefab, result.Anchor.transform);
            }
            else
            {
                OnAnchorResolvedFinished(false, cloudId, result.CloudAnchorState.ToString());
            }
        }

        private void OnAnchorHostedFinished(bool success, string response = null)
        {
            if (success)
            {
                InstructionText.text = "Finish!";
                Invoke("DoHideInstructionBar", 1.5f);
                DebugText.text =
                    string.Format("Succeed to host the Cloud Anchor: {0}.", response);

                // Display name panel and hide instruction bar.
                NameField.text = _hostedCloudAnchor.Name;
                NamePanel.SetActive(true);
                SetSaveButtonActive(true);

                // Automatically open the resolve menu after a successful placement
             //   Controller.SwitchToResolveMenu();
            }
            else
            {
                InstructionText.text = "Host failed.";
                DebugText.text = "Failed to host a Cloud Anchor" + (response == null ? "." :
                    "with error " + response + ".");
            }
        }

        private void OnAnchorResolvedFinished(bool success, string cloudId, string response = null)
        {
            if (success)
            {
                InstructionText.text = "Resolve success!";
                DebugText.text =
                    string.Format("Succeed to resolve the Cloud Anchor: {0}.", cloudId);
            }
            else
            {
                InstructionText.text = "Resolve failed.";
                DebugText.text = "Failed to resolve Cloud Anchor: " + cloudId +
                    (response == null ? "." : "with error " + response + ".");
            }
        }
          public void ResolveAllCloudAnchors()
        {
                    // Ensure we're in resolving mode
        Controller.Mode = PersistentCloudAnchorsController.ApplicationMode.Resolving;

                    // If the ResolvingSet is empty, show a debug message
            if (Controller.ResolvingSet.Count == 0)
             {
                 DebugText.text = "No Cloud Anchors to resolve.";
                return;
            }

                    // Log the number of anchors being resolved
              Debug.LogFormat("Attempting to resolve {0} Cloud Anchor(s)", Controller.ResolvingSet.Count);
            DebugText.text = string.Format("Resolving {0} Cloud Anchors...", Controller.ResolvingSet.Count);
         }

         
             // First, ensure we have the ResolveMenuManager reference
    
            
                 // Programmatically select all anchors

                 // Set mode to resolving
                
         
         public void ResolveAllSavedAnchors()
         {
             
             ResolveMenuManager resolveMenuManager = FindObjectOfType<ResolveMenuManager>();
             resolveMenuManager.SelectAllAnchors();
             if (resolveMenuManager != null)
             {
                 // Programmatically select all anchors
                 resolveMenuManager.SelectAllAnchors();

                 // Set mode to resolving
                 Controller.Mode = PersistentCloudAnchorsController.ApplicationMode.Resolving;

                 // Update UI
                 DebugText.text = $"Resolving {Controller.ResolvingSet.Count} saved Cloud Anchors...";
                 InstructionText.text = "Resolving saved locations...";
             }

             // Retrieve all saved Cloud Anchor IDs
             List<string> savedAnchorIds = Controller.GetAllSavedCloudAnchorIds();

             // If no saved anchors, show a message
             if (savedAnchorIds.Count == 0)
             {
                 
                 DebugText.text = "No saved Cloud Anchors found.";
                 return;
             }

             // Clear any existing resolving set
             Controller.ResolvingSet.Clear();

             // Add all saved anchor IDs to the resolving set
             foreach (string anchorId in savedAnchorIds)
             {
                 Controller.ResolvingSet.Add(anchorId);
             }

             // Set mode to resolving
             Controller.Mode = PersistentCloudAnchorsController.ApplicationMode.Resolving;

             // Update UI
             DebugText.text = $"Resolving {savedAnchorIds.Count} saved Cloud Anchors...";
             InstructionText.text = "Resolving saved locations...";
         }
        private void UpdateInitialInstruction()
        {
            switch (Controller.Mode)
            {
                case PersistentCloudAnchorsController.ApplicationMode.Hosting:
                    // Initial instruction for hosting flow:
                    InstructionText.text = "Tap to place an object.";
                    DebugText.text = "Tap a vertical or horizontal plane...";
                    return;
                case PersistentCloudAnchorsController.ApplicationMode.Resolving:
                    // Initial instruction for resolving flow:
                    InstructionText.text =
                        "Look at the location you expect to see the AR experience appear.";
                    DebugText.text = string.Format("Attempting to resolve {0} anchors...",
                        Controller.ResolvingSet.Count);
                    return;
                default:
                    return;
            }
        }

        private void UpdatePlaneVisibility(bool visible)
        {
            foreach (var plane in Controller.PlaneManager.trackables)
            {
                plane.gameObject.SetActive(visible);
            }
        }

        private void ARCoreLifecycleUpdate()
        {
            // Only allow the screen to sleep when not tracking.
            var sleepTimeout = SleepTimeout.NeverSleep;
            if (ARSession.state != ARSessionState.SessionTracking)
            {
                sleepTimeout = SleepTimeout.SystemSetting;
            }

            Screen.sleepTimeout = sleepTimeout;

            if (_isReturning)
            {
                return;
            }

            // Return to home page if ARSession is in error status.
            if (ARSession.state != ARSessionState.Ready &&
                ARSession.state != ARSessionState.SessionInitializing &&
                ARSession.state != ARSessionState.SessionTracking)
            {
                ReturnToHomePage(string.Format(
                    "ARCore encountered an error state {0}. Please start the app again.",
                    ARSession.state));
            }
        }

        private void DisplayTrackingHelperMessage()
        {
            if (_isReturning || ARSession.notTrackingReason == NotTrackingReason.None)
            {
                TrackingHelperText.gameObject.SetActive(false);
            }
            else
            {
                TrackingHelperText.gameObject.SetActive(true);
                switch (ARSession.notTrackingReason)
                {
                    case NotTrackingReason.Initializing:
                        TrackingHelperText.text = _initializingMessage;
                        return;
                    case NotTrackingReason.Relocalizing:
                        TrackingHelperText.text = _relocalizingMessage;
                        return;
                    case NotTrackingReason.InsufficientLight:
                        if (_versionInfo.GetStatic<int>("SDK_INT") < _androidSSDKVesion)
                        {
                            TrackingHelperText.text = _insufficientLightMessage;
                        }
                        else
                        {
                            TrackingHelperText.text = _insufficientLightMessageAndroidS;
                        }

                        return;
                    case NotTrackingReason.InsufficientFeatures:
                        TrackingHelperText.text = _insufficientFeatureMessage;
                        return;
                    case NotTrackingReason.ExcessiveMotion:
                        TrackingHelperText.text = _excessiveMotionMessage;
                        return;
                    case NotTrackingReason.Unsupported:
                        TrackingHelperText.text = _unsupportedMessage;
                        return;
                    default:
                        TrackingHelperText.text =
                            string.Format("Not tracking reason: {0}", ARSession.notTrackingReason);
                        return;
                }
            }
        }

        private void ReturnToHomePage(string reason)
        {
            Debug.Log("Returning home for reason: " + reason);
            if (_isReturning)
            {
                return;
            }

            _isReturning = true;
            DebugText.text = reason;
            Invoke("DoReturnToHomePage", 3.0f);
        }

        private void DoReturnToHomePage()
        {
            Controller.SwitchToHomePage();
        }

        private void DoHideInstructionBar()
        {
            InstructionBar.SetActive(false);
        }

        private void SetSaveButtonActive(bool active)
        {
            SaveButton.enabled = active;
            SaveButton.GetComponentInChildren<Text>().color = active ? _activeColor : Color.gray;
        }
    }
}
