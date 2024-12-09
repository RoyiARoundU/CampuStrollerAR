using UnityEngine;
using UnityEngine.UI;

public class ToggleObject : MonoBehaviour
{
    [SerializeField] private GameObject targetObject; // Assign the GameObject you want to toggle
    private bool isObjectActive;

    void Start()
    {
        // Ensure the target object is initially active
        if (targetObject != null)
        {
            targetObject.SetActive(true);
            isObjectActive = true;
        }
        else
        {
            Debug.LogWarning("Target object is not assigned!");
        }
    }
    public void ToggleActiveState()
    {
        // Toggle the active state
        if (targetObject != null)
        {
            isObjectActive = !isObjectActive;
            targetObject.SetActive(isObjectActive);
        }
        else
        {
            Debug.LogWarning("Target object is not assigned!");
        }
    }
}