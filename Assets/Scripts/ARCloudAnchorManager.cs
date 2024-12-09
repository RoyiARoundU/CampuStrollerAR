using UnityEngine;
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Events;

public class AnchorCreatedEvent : UnityEvent<Transform>{}
public class ARCloudAnchorManager : MonoBehaviour
{
    [SerializeField] private Camera arCamera = null;
    [SerializeField] private float resolvedAnchorPassedTimeout = 10.0f;
    private ARAnchorManager arAnchorManager = null;
    private ARAnchor pendingHostAnchor = null;
    public ARCloudAnchor cloudAnchor = null;
    private string anchorToResolve;
    
    private bool anchorHostInProgress = false;
    
    private bool anchorResolveInProgress = false;

    private float safeToResolvePassed = 0;
    
    private AnchorCreatedEvent cloudAnchorCreateEvent = null;

    private void Awake()
    {
        cloudAnchorCreateEvent = new AnchorCreatedEvent();
        //cloudAnchorCreateEvent.AddListener((t) => ARPlacementManager.instance.ReCreatePlacement(t));
    }

    private Pose GetCameraPose()
    {
        return new Pose(arCamera.transform.position, arCamera.transform.rotation);
    }

    public void QueueAnchor(ARAnchor arAnchor)
    {
        pendingHostAnchor = arAnchor;
    }
    
    public void HostAnchor()
    {
        Debug.Log("ARCloudAnchorManager::HostAnchor");
        FeatureMapQuality quality = arAnchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose());
        Debug.Log("ARCloudAnchorManager::HostAnchor quality: " + quality.ToString());
        cloudAnchor = arAnchorManager.HostCloudAnchor(pendingHostAnchor, 1);
        if (cloudAnchor == null)
        {
            Debug.Log("Failed to host cloud anchor");
        }
        else
        {
            anchorHostInProgress = true;
        }
        
    }
    public void Resolve()
    {
        Debug.Log("ARCloudAnchorManager::Resolve");
        cloudAnchor = arAnchorManager.ResolveCloudAnchorId(anchorToResolve);
      if (cloudAnchor == null)
       {
          Debug.Log($"Failed to host cloud anchor {anchorToResolve}");
       }
       else
       {
        anchorResolveInProgress = true;
       }
    }
    private void CheckHostingProgress()
    {
        CloudAnchorState cloudAnchorState = cloudAnchor.cloudAnchorState;
        
        if (cloudAnchorState==CloudAnchorState.Success)
        {
            anchorHostInProgress = false;
            anchorToResolve = cloudAnchor.cloudAnchorId;
        }
        else if (cloudAnchorState != CloudAnchorState.TaskInProgress)
        {
            Debug.Log($"Error while hosting cloud anchor{cloudAnchorState}");
            anchorHostInProgress = false;
        }
    }
    private void CheckResolveProgress()
    {
        CloudAnchorState cloudAnchorState = cloudAnchor.cloudAnchorState;
        
        if (cloudAnchorState == CloudAnchorState.Success)
        {
            anchorResolveInProgress = false;
          //  anchorToResolve = cloudAnchor.cloudAnchorId;
            
            cloudAnchorCreateEvent?.Invoke(cloudAnchor.transform);
        }
        else if (cloudAnchorState != CloudAnchorState.TaskInProgress)
        {
            Debug.Log($"Error while resolving cloud anchor{cloudAnchorState}");
            anchorResolveInProgress = false;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    void Update()
    {
        if (anchorHostInProgress)
        {
            CheckHostingProgress();
            return;
        }

        if (anchorResolveInProgress && safeToResolvePassed <= 0)
        {
            safeToResolvePassed = resolvedAnchorPassedTimeout;

            if (!string.IsNullOrEmpty(anchorToResolve))
            {
                Debug.Log($"Resolving {anchorToResolve}");
                CheckResolveProgress();
            }
        }
        else
        {
            safeToResolvePassed -= Time.deltaTime * 1.0f;
        }
            
    }
}
