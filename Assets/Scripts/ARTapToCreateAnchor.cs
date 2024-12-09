using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Google.XR.ARCoreExtensions;
using Unity.XR.CoreUtils; // Add this namespace
using UnityEngine.UI;

[RequireComponent(typeof(ARRaycastManager))] // Ensure these components are present
[RequireComponent(typeof(ARAnchorManager))] // Ensure these components are present
[RequireComponent(typeof(ARTrackableManager<XRAnchorSubsystem, XRAnchorSubsystemDescriptor, XRAnchorSubsystem.Provider, XRAnchor, ARAnchor>))] // Ensure these components are present
[RequireComponent(typeof(ARSession))] // Ensure these components are present
[RequireComponent(typeof(XROrigin))] // Ensure these components are present
[DisallowMultipleComponent]
[DefaultExecutionOrder(-2147483647)]
[AddComponentMenu("XR/AR Foundation/AR Anchor Manager")]



public class ARTapToCreateAnchor : MonoBehaviour
{ 
    public GameObject ARobject;
    private GameObject spawnedObject;
    private ARRaycastManager arRaycastManager;
    private ARAnchorManager arAnchorManager;
    private InteractionLogic interactionLogic;
    private ARSessionOrigin arSessionOrigin;
    private ARSession arSession;
    private ARAnchorManager anchorManager;
    private ARGeospatialAnchor geoAnchor;
    
    [SerializeField] private TMPro.TextMeshProUGUI debugText; // Reference to UI text for debugging

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private void Awake()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        arAnchorManager = GetComponent<ARAnchorManager>();
        interactionLogic = GetComponent<InteractionLogic>();
        arSessionOrigin = FindObjectOfType<ARSessionOrigin>();
        arSession = FindObjectOfType<ARSession>();
        anchorManager = GetComponent<ARAnchorManager>();
    }

    void Update()
    {
        if (Input.touchCount == 1)
        {
            Vector2 touchPosition = Input.GetTouch(0).position;

            if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                if (hits.Count > 0 && spawnedObject == null)  // Only create once
                {
                    var hitPose = hits[0].pose;
                    CreateObjectAndAnchor(hitPose);
                }
            }
        }
    }

    private async void CreateObjectAndAnchor(Pose hitPose)
    {
        Pose modifiedPose = new Pose(hitPose.position, Quaternion.LookRotation(Vector3.forward, hitPose.up));
        
        Debug.Log("1. Spawning object at position: " + modifiedPose.position);
        spawnedObject = Instantiate(ARobject, modifiedPose.position, modifiedPose.rotation);
        interactionLogic.SetTargetObject(spawnedObject);

        // Add ARGeospatialCreatorAnchor component if it doesn't exist
        geoAnchor = spawnedObject.GetComponent<ARGeospatialAnchor>();
        if (geoAnchor == null)
        {
            geoAnchor = spawnedObject.AddComponent<ARGeospatialAnchor>();
        }

        // Start coroutine to wait for geospatial anchor initialization
        StartCoroutine(WaitForGeoAnchorInitialization(geoAnchor, spawnedObject, modifiedPose.position));
    }

    private IEnumerator WaitForGeoAnchorInitialization(ARGeospatialAnchor geoAnchorComponent, 
        GameObject prefab, Vector3 position)
    {
        yield return new WaitForSeconds(5.09f); // Wait for initialization

        if (geoAnchorComponent != null)
        {
           geoAnchorComponent.gameObject.SetActive(true);
           geoAnchorComponent.transform.position = position;

            // Update debug UI
            if (debugText != null)
            {
                Debug.Log($"Geospatial Anchor Was not created  at: local position: {position}");

            }

            Debug.Log($"Geospatial Anchor created at: {position}");

            // Ensure the object stays at the correct position
            prefab.transform.position = position;
        }
        else
        {
            Debug.LogError("Geospatial anchor component is null");
        }
    }

    // Method to resolve a cloud anchor using its ID
    public async void ResolveCloudAnchor(string cloudId)
    {
        Debug.Log($"Attempting to resolve cloud anchor: {cloudId}");
        var resolveResult =  arAnchorManager.ResolveCloudAnchorAsync(cloudId);
        
        // if (resolveResult.State == CloudAnchorState)
        // {
        //     Debug.Log("Cloud anchor resolved successfully");
        //     // Create your AR object at the resolved anchor position
        //     spawnedObject = Instantiate(ARobject, resolveResult.Pose.position, resolveResult.Pose.rotation);
        //     spawnedObject.transform.SetParent(resolveResult.Anchor.transform);
        //     interactionLogic.SetTargetObject(spawnedObject);
        // }
        // else
        // {
        //     Debug.LogError($"Failed to resolve cloud anchor: {resolveResult.State}");
        // }
    }

    // Optional: Method to check feature map quality
    // public FeatureMapQuality CheckFeatureMapQuality()
    // {
    //     return arAnchorManager.EstimateFeatureMapQualityForHosting(Camera.main.transform.position);
    // }
}