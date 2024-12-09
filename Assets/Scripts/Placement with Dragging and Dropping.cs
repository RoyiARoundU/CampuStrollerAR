using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AR;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

[RequireComponent(typeof(ARRaycastManager))]
//[RequireComponent(typeof(ARKitReferenceObjectEntry))]
//[RequireComponent(typeof(ARPlaneManager))]
public class ReferencePointManager : MonoBehaviour
{
    private ARRaycastManager arRaycastManager;
    
    private ARKitReferenceObjectEntry arReferencePointManager;
    
    private ARPlaneManager arPlaneManager;

    [SerializeField]
    private GameObject placedPrefab;
    
    [SerializeField] private XRInteractionManager _interactionManager;
    
    private List<GameObject> addedInstances = new List<GameObject>();
    
    [SerializeField]
    private GameObject welcomePanel;
    
    [SerializeField]
    private Button startButton;
    
    [SerializeField]
    private Camera arCamera;
    
    private Vector2 touchPosition = default;

    
    private static List<ARRaycastHit> hits = new List <ARRaycastHit>();
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            touchPosition = touch.position;

            if (touch.phase == TouchPhase.Began)
            {
                // Check if we can place an object at the touch position
                if (TryGetTouchPosition(touchPosition))
                {
                    
                    PlaceObject();
                }
            }
        }
    }

    private bool TryGetTouchPosition(Vector2 touchPosition)
    {

        return false;
    }

    private void PlaceObject()
    {
        GameObject placedObject = Instantiate(placedPrefab, hits[0].pose.position, hits[0].pose.rotation);
        // Add the placed object to your array or list of placed objects if needed
    }
    
    //static List<ARRaycastHit> hits = new List<ARRaycastHit>();>
}
