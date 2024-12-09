using UnityEngine;
using Google.XR.ARCoreExtensions.GeospatialCreator;

public class LocationSnapper : MonoBehaviour
{
    [System.Serializable]
    public class AnchorPosition
    {
        public string name;
        public double latitude;
        public double longitude;
        public double altitude;
        public GameObject targetModel;
    }

    [Header("Predefined Positions")]
    [SerializeField] private AnchorPosition[] anchorPositions;

    [Header("Map References")]
    [SerializeField] private MapTileController mapController;
    [SerializeField] private MapManager mapManager;

    void Start()
    {
        // Delay the position reset
        Invoke("ResetAllModelsToZero", 10f);
        Debug.Log("Position reset scheduled for 10 seconds");
    }

    // Reset all models to zero position
    public void ResetAllModelsToZero(string reset)
    {
        foreach (var position in anchorPositions)
        {
            if (position.targetModel != null)
            {
                // Store current coordinates
                var anchor = position.targetModel.GetComponent<ARGeospatialCreatorAnchor>();
                if (anchor != null)
                {
                    Debug.Log($"Before reset - {position.name} at position: {position.targetModel.transform.position}");
                    
                    // Only reset transform values
                    position.targetModel.transform.localPosition = Vector3.zero;
                    position.targetModel.transform.localRotation = Quaternion.identity;
                    position.targetModel.transform.localScale = Vector3.one;
                    
                    Debug.Log($"Reset {position.name} transform to zero. Coordinates maintained at Lat:{anchor.Latitude}, Lon:{anchor.Longitude}");
                }
            }
        }
    }

  

    public void CenterMap()
    {
        if (mapController != null)
        {
            mapController.ResetToInitialPosition();
            Debug.Log("Map centered to initial position");
        }
    }
}
