using UnityEngine;

public class UiManagar : MonoBehaviour
{

    [SerializeField] private GameObject eventPanelUserInRange; 
    [SerializeField] private GameObject eventPanelUserNotInRange; 
    [SerializeField] private GameObject MapPanelAsset;

    [SerializeField] private string linkToOpen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MapPanelAsset.SetActive(false);

    }
    
    public void onPressOpenMapPanel(string buttonName)
    {
        if (buttonName == "Map")
        {
            MapPanelAsset.SetActive(!MapPanelAsset.activeSelf);
            
        }
    }
    
    public void OpenURL(string buttonName)
    {
        if (buttonName == "Link")
        {
            Application.OpenURL(linkToOpen);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayEventPanelUserInRange()
    {
        eventPanelUserInRange.SetActive(true);
    }
    public void DisplayEventPanelUserNotInRange()
    {
        eventPanelUserNotInRange.SetActive(true);
    }
    
}

