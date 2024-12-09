using Unity.VisualScripting;
using UnityEngine;

public class SceneManagerScript : MonoBehaviour
{
    
    UiManagar manuUImanager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        manuUImanager = GameObject.Find("Canvas").GetComponent<UiManagar>();
    }

    // Update is called once per frame
    void Update()
    {
    //    playerLocation = GameObject.Find("ARCamera").GetComponent<LocalizationAsset>().ViewportPointToRay(new Vector3(0.5f, 0.5f));
        
    }

    private void OnTouch()
    {
        
    }
  
}
