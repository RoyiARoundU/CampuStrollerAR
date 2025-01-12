using System.Collections.Generic;
using UnityEngine;

public class ProximityActivator : MonoBehaviour
{
    [SerializeField] private GameObject targetObject; // The target GameObject
    [SerializeField] private List<GameObject> objectsToActivate; // List of GameObjects to show
    [SerializeField] private float activationDistance = 1.0f; // Distance in meters to activate objects

    private Transform deviceTransform;

    void Start()
    {
        // Hide all objects in the list initially
        foreach (var obj in objectsToActivate)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // Get the AR device's transform (Main Camera)
        deviceTransform = Camera.main.transform;
    }

    void Update()
    {
        if (targetObject == null || deviceTransform == null)
            return;

        // Calculate distance between the device and the target object
        float distance = Vector3.Distance(deviceTransform.position, targetObject.transform.position);

        // If within the activation distance, show the objects
        if (distance <= activationDistance)
        {
            foreach (var obj in objectsToActivate)
            {
                if (obj != null && !obj.activeSelf)
                    obj.SetActive(true);
            }
        }
        else
        {
            // Optionally, hide objects when out of range
            foreach (var obj in objectsToActivate)
            {
                if (obj != null && obj.activeSelf)
                    obj.SetActive(false);
            }
        }
    }
}