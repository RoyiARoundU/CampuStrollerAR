using UnityEngine;
using System.Collections;
using System;
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARFoundation;

public class InitializationManager : MonoBehaviour
{
    /// <summary>
    /// The AREarthManager used in the sample.
    /// </summary>
    public AREarthManager EarthManager;
    
    /// <summary>
    /// The ARCoreExtensions used in the sample.
    /// </summary>
    public ARCoreExtensions ARCoreExtensions;
    
    public enum InitializationState
    {
        NotStarted,
        StartLocationService,
        StartAvailabilityCheck,
        StartFeatureSupportCheck,
        Complete,
        Error
    }
    
    private InitializationState _initializationState;
    private bool _waitingForLocationService;

    public InitializationState GetInitializationState()
    {
        return _initializationState;
    }
    
    private void Awake()
    {
        _initializationState = InitializationState.NotStarted;
    }

    public IEnumerator InitializeServices()
    {
        _initializationState = InitializationState.StartLocationService;
        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [InitializationManager] Starting initialization...");
        // Simulate some initialization work
        yield return StartCoroutine(StartLocationService());
        if (_initializationState == InitializationState.Error)
        {
            yield break;
        }

        _initializationState = InitializationState.StartAvailabilityCheck;
        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Start ARSession availability check");
        yield return StartCoroutine(AvailabilityCheck());
        if (_initializationState == InitializationState.Error)
        {
            yield break;
        }
        
        _initializationState = InitializationState.StartFeatureSupportCheck;
        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Start EarthManager & ARCoreExtension support check");
        yield return StartCoroutine(FeatureSupportCheck());
        if (_initializationState == InitializationState.Error)
        {
            yield break;
        }

        // Mark initialization as complete
        _initializationState = InitializationState.Complete;
        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [InitializationManager] Initialization finished.");
    }

    private IEnumerator StartLocationService()
    {
        _waitingForLocationService = true;
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Debug.Log("Requesting the fine location permission.");
                Permission.RequestUserPermission(Permission.FineLocation);
                yield return new WaitForSeconds(3.0f);
            }
#endif

        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("Location service is disabled by the user.");
            _waitingForLocationService = false;
            yield break;
        }

        Debug.Log("Starting location service.");
        Input.location.Start();

        while (Input.location.status == LocationServiceStatus.Initializing)
        {
            yield return null;
        }

        _waitingForLocationService = false;
        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.LogWarningFormat(
                "Location service ended with {0} status.", Input.location.status);
            Input.location.Stop();
        }
    }
    
    private IEnumerator AvailabilityCheck()
    {
        if (ARSession.state == ARSessionState.None)
        {
            yield return ARSession.CheckAvailability();
        }

        // Waiting for ARSessionState.CheckingAvailability.
        yield return null;

        if (ARSession.state == ARSessionState.NeedsInstall)
        {
            yield return ARSession.Install();
        }

#if UNITY_ANDROID

        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Debug.Log("Requesting camera permission.");
            Permission.RequestUserPermission(Permission.Camera);
            yield return new WaitForSeconds(3.0f);
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            // User has denied the request.
            Debug.LogWarning(
                "Failed to get the camera permission. VPS availability check isn't available.");
            yield break;
        }
#endif
    }

    private IEnumerator FeatureSupportCheck()
    {
        while (true)
        {
            if (ARSession.state == ARSessionState.SessionInitializing ||
                ARSession.state == ARSessionState.SessionTracking)
            {
                // Check feature support and enable Geospatial API when it's supported.
                var featureSupport = EarthManager.IsGeospatialModeSupported(GeospatialMode.Enabled);
                
                if (featureSupport == FeatureSupported.Unsupported)
                {
                    _initializationState = InitializationState.Error;
                    yield break;
                }

                if (featureSupport == FeatureSupported.Supported)
                {
                    if (ARCoreExtensions.ARCoreExtensionsConfig.GeospatialMode ==
                        GeospatialMode.Disabled)
                    {
                        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Geospatial sample switched to GeospatialMode.Enabled.");
                        ARCoreExtensions.ARCoreExtensionsConfig.GeospatialMode =
                            GeospatialMode.Enabled;
                        ARCoreExtensions.ARCoreExtensionsConfig.StreetscapeGeometryMode =
                            StreetscapeGeometryMode.Enabled;
                        yield return new WaitForSeconds(3.0f);
                    }
                    
                    // Check earth state.
                    var earthState = EarthManager.EarthState;
                    if (earthState == EarthState.Enabled)
                    {
                        yield break;
                    }
                    Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] EarthManager state: {earthState}");
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}