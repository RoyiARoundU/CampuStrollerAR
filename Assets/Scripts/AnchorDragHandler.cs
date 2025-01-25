using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class AnchorDragHandler : MonoBehaviour
{
    private ARRaycastManager raycastManager;
    private Vector2 initialTouchPoint;
    private static AnchorDragHandler currentlyInteracting;
    private Camera mainCamera;

    [SerializeField] private float minScale = 0.1f;
    [SerializeField] private float maxScale = 10f;
    
    private bool isDragging = false;
    private bool isRotating = false;
    private bool isScaling = false;
    private Vector3 offset;
    private Vector3 initialScale;
    private float initialPinchDistance;
    private Quaternion initialRotation;
    
    void Awake()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.touchCount == 1)
        {
            HandleSingleTouch();
        }
        else if (Input.touchCount == 2)
        {
            HandleTwoFingerTouch();
        }
        else if (Input.touchCount == 3)
        {
            HandleThreeFingerTouch();
        }
        else
        {
            // Reset all interactions when no touches are detected
            EndAllInteractions();
        }
    }

    private void HandleSingleTouch()
    {
        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                TryStartDrag(touch.position);
                break;
            case TouchPhase.Moved:
                UpdateDragPosition(touch.position);
                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                EndDrag();
                break;
        }
    }

    private void HandleTwoFingerTouch()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        // Calculate center point and vector between touches
        Vector2 touchCenter = (touch0.position + touch1.position) * 0.5f;
        float currentPinchDistance = Vector2.Distance(touch0.position, touch1.position);

        if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
        {
            if (currentlyInteracting == null || currentlyInteracting == this)
            {
                isScaling = true;
                isRotating = true;
                currentlyInteracting = this;
                initialPinchDistance = currentPinchDistance;
                initialScale = transform.localScale;
                initialTouchPoint = touchCenter;
                initialRotation = transform.rotation;
            }
        }
        else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
        {
            if (currentlyInteracting != this) return;

            // Handle scaling
            if (isScaling)
            {
                float scaleFactor = currentPinchDistance / initialPinchDistance;
                Vector3 newScale = initialScale * scaleFactor;
                newScale = new Vector3(
                    Mathf.Clamp(newScale.x, minScale, maxScale),
                    Mathf.Clamp(newScale.y, minScale, maxScale),
                    Mathf.Clamp(newScale.z, minScale, maxScale)
                );
                transform.localScale = newScale;
            }

            // Handle rotation
            if (isRotating)
            {
                // Calculate rotation deltas for each axis
                float deltaX = (touchCenter.y - initialTouchPoint.y) * 0.5f; // Pitch (X-axis)
                float deltaY = (touchCenter.x - initialTouchPoint.x) * 0.5f; // Yaw (Y-axis)
                
                // Update initial touch point for next frame
                initialTouchPoint = touchCenter;
                
                // Apply rotation on both axes
                transform.Rotate(deltaX, deltaY, 0, Space.World);
            }
        }
        else if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended)
        {
            EndPinchToScale();
            EndRotation();
        }
    }

    private void HandleThreeFingerTouch()
    {
        if (Input.touchCount != 3) return;

        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);
        Touch touch2 = Input.GetTouch(2);

        // Calculate center point of the three touches
        Vector2 touchCenter = (touch0.position + touch1.position + touch2.position) / 3f;

        if (touch0.phase == TouchPhase.Began && touch1.phase == TouchPhase.Began && touch2.phase == TouchPhase.Began)
        {
            StartRotation();
            initialTouchPoint = touchCenter;
        }
        else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
        {
            if (!isRotating || currentlyInteracting != this) return;

            // Calculate rotation deltas for each axis
            float deltaX = (touchCenter.y - initialTouchPoint.y) * 0.5f; // Pitch (X-axis)
            float deltaY = (touchCenter.x - initialTouchPoint.x) * 0.5f; // Yaw (Y-axis)
            
            // Update initial touch point for next frame
            initialTouchPoint = touchCenter;
            
            // Apply rotation on all axes
            transform.Rotate(deltaX, deltaY, 0, Space.World);
        }
        else if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended)
        {
            EndRotation();
        }
    }

    private void StartPinchToScale(Touch touch0, Touch touch1)
    {
        if (currentlyInteracting == null || currentlyInteracting == this)
        {
            isScaling = true;
            currentlyInteracting = this;
            initialPinchDistance = Vector2.Distance(touch0.position, touch1.position);
            initialScale = transform.localScale;
        }
    }

    private void ContinuePinchToScale(Touch touch0, Touch touch1)
    {
        if (!isScaling || currentlyInteracting != this) return;

        float currentPinchDistance = Vector2.Distance(touch0.position, touch1.position);
        float scaleFactor = currentPinchDistance / initialPinchDistance;

        Vector3 newScale = initialScale * scaleFactor;
        newScale = new Vector3(
            Mathf.Clamp(newScale.x, minScale, maxScale),
            Mathf.Clamp(newScale.y, minScale, maxScale),
            Mathf.Clamp(newScale.z, minScale, maxScale)
        );

        transform.localScale = newScale;
    }

    private void StartRotation()
    {
        if (currentlyInteracting == null || currentlyInteracting == this)
        {
            isRotating = true;
            currentlyInteracting = this;
            initialRotation = transform.rotation;
        }
    }

    private void ContinueRotation()
    {
        if (!isRotating || currentlyInteracting != this) return;

        Vector2 currentTouchPoint = Input.GetTouch(0).position;
        float rotationDelta = (currentTouchPoint.x - initialTouchPoint.x) * 0.5f;
        transform.rotation = initialRotation * Quaternion.Euler(0, 0, rotationDelta);
    }

    private void TryStartDrag(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
        {
            if (currentlyInteracting == null || currentlyInteracting == this)
            {
                isDragging = true;
                currentlyInteracting = this;
                offset = transform.position - hit.point;
            }
        }
    }

    private void UpdateDragPosition(Vector2 screenPosition)
    {
        if (!isDragging || currentlyInteracting != this) return;

        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(screenPosition, hits, TrackableType.Planes))
        {
            ARRaycastHit closestHit = hits[0];
            transform.position = closestHit.pose.position + offset;
        }
    }

    private void EndDrag()
    {
        if (isDragging && currentlyInteracting == this)
        {
            isDragging = false;
            if (!isRotating && !isScaling)
            {
                currentlyInteracting = null;
            }
        }
    }

    private void EndPinchToScale()
    {
        if (isScaling && currentlyInteracting == this)
        {
            isScaling = false;
            if (!isDragging && !isRotating)
            {
                currentlyInteracting = null;
            }
        }
    }

    private void EndRotation()
    {
        if (isRotating && currentlyInteracting == this)
        {
            isRotating = false;
            if (!isDragging && !isScaling)
            {
                currentlyInteracting = null;
            }
        }
    }

    private void EndAllInteractions()
    {
        if (currentlyInteracting == this)
        {
            isDragging = false;
            isScaling = false;
            isRotating = false;
            currentlyInteracting = null;
        }
    }
}