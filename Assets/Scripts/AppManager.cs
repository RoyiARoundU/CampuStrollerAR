using UnityEngine;
using System.Collections;
using Google.XR.ARCoreExtensions;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class AppManager : MonoBehaviour
{
    [Header("AR Components")]

    /// <summary>
    /// The XROrigin used in the sample.
    /// </summary>
    public XROrigin Origin;

    /// <summary>
    /// The ARSession used in the sample.
    /// </summary>
    public ARSession Session;
    
    /// <summary>
    /// UI element containing all AR view contents.
    /// </summary>
    public GameObject ARViewCanvas;
    
    /// <summary>
    /// The ARCoreExtensions used in the sample.
    /// </summary>
    public ARCoreExtensions ARCoreExtensions;
    
    /// <summary>
    /// The AREarthManager used in the sample.
    /// </summary>
    public AREarthManager EarthManager;
    
    public GameObject PrivacyPromptCanvas;
    
    public InitializationManager initializationManager;
    
    public enum AppState
    {
        Initializing,
        Localizing,
        Loading,
        Ready,
        Error
    }
    
    private const float _accuracyThreshold = 25.0f;

    private AppState _currentState;
    private IEnumerator _asyncCheck;

    private GeospatialPose _pose;
    private float _configurePrepareTime;
    private bool _enablingGeospatial;

    public GeospatialPose GetGeospatialPose() => _pose;
    
    public AppState GetAppState() => _currentState;

    private void Awake()
    {
        // Lock screen to portrait.
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.orientation = ScreenOrientation.Portrait;
        
        // Enable geospatial sample to target 60fps camera capture frame rate
        // on supported devices.
        // Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
        Application.targetFrameRate = 60;
        
        SetState(AppState.Initializing);
    }

    private void Start()
    {
        // Start the FSM logic
        StartCoroutine(ProcessStates());
    }

    private IEnumerator ProcessStates()
    {
        while (true)
        {
            switch (_currentState)
            {
                case AppState.Initializing:
                    HandleInitializing();
                    break;

                case AppState.Localizing:
                    HandleLocalizing();
                    break;

                case AppState.Loading:
                    HandleLoading();
                    break;

                case AppState.Ready:
                    HandleReady();
                    break;

                case AppState.Error:
                    HandleError();
                    break;
            }
            yield return null;
        }
    }

    private void HandleInitializing()
    {
        InitializationManager.InitializationState initializationState = initializationManager.GetInitializationState();
        if (initializationState == InitializationManager.InitializationState.NotStarted)
        {
            Debug.Log("[AppManager] Starting services in background...");
            StartCoroutine(initializationManager.InitializeServices());
            
            SwitchToARView(true);
        } else if (initializationState == InitializationManager.InitializationState.Complete)
        {
            Debug.Log("[AppManager] Moving on to localizing");
            SetState(AppState.Localizing);
        }
    }

    private void HandleLocalizing()
    {
        TrackingState earthTrackingState = EarthManager.EarthTrackingState;
        bool tracking = earthTrackingState == TrackingState.Tracking;
        _pose = tracking ? EarthManager.CameraGeospatialPose : new GeospatialPose();
        if (tracking
            && _pose.HorizontalAccuracy < _accuracyThreshold
            && _pose.VerticalAccuracy < _accuracyThreshold
            && _pose.OrientationYawAccuracy < _accuracyThreshold)
        {
            SetState(AppState.Loading);
        }
    }

    private void HandleLoading()
    {
        SetState(AppState.Ready);
    }

    private void HandleReady()
    {
        // Debug.Log("localization is ready");
        TrackingState earthTrackingState = EarthManager.EarthTrackingState;
        bool tracking = earthTrackingState == TrackingState.Tracking;
        _pose = tracking ? EarthManager.CameraGeospatialPose : new GeospatialPose();
        if (!tracking
            || _pose.HorizontalAccuracy >= _accuracyThreshold
            || _pose.VerticalAccuracy >= _accuracyThreshold
            || _pose.OrientationYawAccuracy >= _accuracyThreshold)
        {
            SetState(AppState.Localizing);
        }
    }

    private void HandleError()
    {
        Debug.LogError("[AppManager] State: ERROR");
    }

    private void SetState(AppState newState)
    {
        Debug.Log($"[AppManager] Transitioning to state: {newState}");
        _currentState = newState;
    }
    
    private void SwitchToARView(bool enable)
    {
        Origin.gameObject.SetActive(enable);
        Session.gameObject.SetActive(enable);
        ARCoreExtensions.gameObject.SetActive(enable);
        ARViewCanvas.SetActive(enable);
        PrivacyPromptCanvas.SetActive(!enable);
        Debug.Log($"Switch to AR View: {enable}");
    }

    private void UpdatePose()
    {
        var earthTrackingState = EarthManager.EarthTrackingState;
        _pose = earthTrackingState == TrackingState.Tracking ?
            EarthManager.CameraGeospatialPose : new GeospatialPose();
    }
}
