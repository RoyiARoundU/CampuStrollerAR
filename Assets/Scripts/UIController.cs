using System;
using Google.XR.ARCoreExtensions;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public AppManager AppManager;
    public InitializationManager InitializationManager;
    
    /// <summary>
    /// Text displaying <see cref="GeospatialPose"/> information at runtime.
    /// </summary>
    public Text InfoText;
        
    /// <summary>
    /// Text displaying in a snack bar at the bottom of the screen.
    /// </summary>
    public Text SnackBarText;
    
    private const string _initializingLocationServiceMessage = "Initializing location service";
    private const string _initializingArSessionMessage = "Initializing AR session";
    private const string _initializingEarthManagerMessage = "Initializing EarthManager";
    
    private bool _isGetStartedButtonClicked;

    public void Awake()
    {
        
    }

    public void Update()
    {
        UpdateInfoText();
        UpdateSnackBarText();
    }

    // Called by the InfoPanel "Get Started" button OnClick
    public void OnGetStartedButtonClicked()
    {
        _isGetStartedButtonClicked = true;
    }
    
    public bool IsGetStartedButtonClicked() => _isGetStartedButtonClicked;

    private void UpdateInfoText()
    {
        GeospatialPose pose = AppManager.GetGeospatialPose();
        InfoText.text = string.Format(
            "Latitude/Longitude: {1}°, {2}°{0}" +
            "Horizontal Accuracy: {3}m{0}" +
            "Altitude: {4}m{0}" +
            "Vertical Accuracy: {5}m{0}" +
            "Eun Rotation: {6}{0}" +
            "Orientation Yaw Accuracy: {7}°",
            Environment.NewLine,
            pose.Latitude.ToString("F6"),
            pose.Longitude.ToString("F6"),
            pose.HorizontalAccuracy.ToString("F6"),
            pose.Altitude.ToString("F2"),
            pose.VerticalAccuracy.ToString("F2"),
            pose.EunRotation.ToString("F1"),
            pose.OrientationYawAccuracy.ToString("F1"));
    }

    private void UpdateSnackBarText()
    {
        AppManager.AppState appState = AppManager.GetAppState();
        if (appState == AppManager.AppState.Initializing)
        {
            InitializationManager.InitializationState initializationState = InitializationManager.GetInitializationState();
            if (initializationState == InitializationManager.InitializationState.StartLocationService)
            {
                SnackBarText.text = _initializingLocationServiceMessage;
            } else if (initializationState == InitializationManager.InitializationState.StartAvailabilityCheck)
            {
                SnackBarText.text = _initializingArSessionMessage;
            } else if (initializationState == InitializationManager.InitializationState.StartFeatureSupportCheck)
            {
                SnackBarText.text = _initializingEarthManagerMessage;
            }
        } else if (appState == AppManager.AppState.Localizing)
        {
            SnackBarText.text = "Localizing... Please point your camera to signs and buildings around";
        } else if (appState == AppManager.AppState.Loading)
        {
            SnackBarText.text = "Loading assets...";
        } else if (appState == AppManager.AppState.Ready)
        {
            SnackBarText.text = "Ready";
        }
    }
}