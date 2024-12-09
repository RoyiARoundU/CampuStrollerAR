using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using Object = UnityEngine.Object;

public class SimpleCreate : MonoBehaviour
{
    [SerializeField]
    private Material m_ObjectMaterial;
    
    private PrimitiveType m_ObjectType = PrimitiveType.Sphere;
    private Camera arCamera;

    void Start()
    {
        arCamera = Camera.main;
    }

    void Update()
    {
        // Check for touch input
        if (Input.touchCount > 0)
        {
            CreateObjectOnTap();
        }
    }

    private void CreateObjectOnTap()
    {
        // Skip Plane primitive type
        if (m_ObjectType == PrimitiveType.Plane)
        {
            m_ObjectType = PrimitiveType.Sphere;
        }

        // Create primitive at 2 meters in front of camera
        Vector3 spawnPosition = arCamera.transform.position + arCamera.transform.forward * 2f;
        GameObject createdObject = GameObject.CreatePrimitive(m_ObjectType);
        
        // Setup transform
        createdObject.transform.position = spawnPosition;
        createdObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        createdObject.transform.rotation = Quaternion.identity;

        // Add interactable components
        AddInteractable(createdObject);

        // Cycle to next primitive type
        m_ObjectType++;
    }

    private void AddInteractable(GameObject createdObject)
    {
        createdObject.AddComponent<XRGrabInteractable>();
        createdObject.GetComponent<XRGrabInteractable>().useDynamicAttach = true;
        createdObject.GetComponent<XRGrabInteractable>().matchAttachPosition = true;
        createdObject.GetComponent<XRGrabInteractable>().matchAttachRotation = true;
        createdObject.GetComponent<XRGrabInteractable>().snapToColliderVolume = false;
        createdObject.GetComponent<XRGrabInteractable>().throwOnDetach = false;
        createdObject.AddComponent<XRGeneralGrabTransformer>();
        createdObject.GetComponent<XRGrabInteractable>().AddMultipleGrabTransformer(createdObject.GetComponent<XRGeneralGrabTransformer>());

        createdObject.GetComponent<Rigidbody>().isKinematic = true;
        createdObject.GetComponent<Rigidbody>().useGravity = false;
        
        createdObject.GetComponent<Renderer>().material = m_ObjectMaterial; 
    }
}