using System;
using System.Collections.Generic;
using Google.XR.ARCoreExtensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class GeospatialStreetscapeManager : MonoBehaviour
{
    [SerializeField] private ARStreetscapeGeometryManager streetscapeGeometryManager;

    [SerializeField] private Material buldingMaterial;

    [SerializeField] private Material terrainMaterial;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARBallShooter CameraARshooter;

    [SerializeField] private GameObject objectToSpawn;

    private Dictionary<TrackableId, GameObject> streetScapeGeometryCache = new Dictionary<TrackableId, GameObject>();

    private static List<XRRaycastHit> raycastHits = new List<XRRaycastHit>();

    private bool allowPlacement;

    // private StreetscapeManuOptions options = new ARStreetscapeManuOptions()



    private void OnEnable()
    {
      //  streetscapeGeometryManager.StreetscapeGeometriesChanged += StreetscapeGeometriesChanged;
    }



    private void OnDisable()
    {
     //   streetscapeGeometryManager.StreetscapeGeometriesChanged -= StreetscapeGeometriesChanged;

    }
    private void StreetscapeGeometriesChanged (ARStreetscapeGeometriesChangedEventArgs obj)
    {
        
    }
    internal class ARBallShooter
    {
    }
}