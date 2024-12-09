using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Collections;

public class MapTileController : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private string apiKey;
    [SerializeField] private float initialLat = 40.754974f;
    [SerializeField] private float initialLon = -73.956436f;
    [SerializeField] private int initialZoom = 18;
    [SerializeField] private RectTransform mapRect;
    
    [Header("Control Settings")]
    [SerializeField] private float panSensitivity = 0.0001f;
    [SerializeField] private float zoomSensitivity = 0.5f;
    [SerializeField] private float updateInterval = 0.1f; // Update map every 0.1 seconds
    
    private const int MIN_ZOOM = 1;
    private const int MAX_ZOOM = 20;
    
    private float lat, lon;
    private int zoom;
    private bool isDragging;
    private Vector2 initialTouchPosition;
    private float initialTouchDistance;
    private float nextMapUpdate;
    private bool updateMap;
    private string mapType = "roadmap";
    
    private double mapNorthLat, mapSouthLat, mapEastLon, mapWestLon;
    private RawImage mapImage;

    void Start()
    {
        lat = initialLat;
        lon = initialLon;
        zoom = initialZoom;
        
        mapImage = mapRect.GetComponent<RawImage>();
        if (mapImage == null)
        {
            mapImage = mapRect.gameObject.AddComponent<RawImage>();
        }
        
        updateMap = true;
        nextMapUpdate = Time.time;
    }

    void Update()
    {
        HandleTouchInput();
        
        if (updateMap && Time.time >= nextMapUpdate)
        {
            StartCoroutine(GetGoogleMap());
            nextMapUpdate = Time.time + updateInterval;
            updateMap = false;
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    initialTouchPosition = touch.position;
                    isDragging = true;
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        Vector2 delta = touch.position - initialTouchPosition;
                        
                        // Normalize the movement and invert both X and Y
                        float normalizedX = (delta.x / Screen.width) * panSensitivity * Time.deltaTime;
                        float normalizedY = (delta.y / Screen.height) * panSensitivity * Time.deltaTime;
                        
                        // Invert both axes (minus sign for both X and Y)
                        lon -= normalizedX;  // Inverted X movement
                        lat -= normalizedY;  // Inverted Y movement
                        
                        initialTouchPosition = touch.position;
                        updateMap = true;
                    }
                    break;

                case TouchPhase.Ended:
                    isDragging = false;
                    break;
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                initialTouchDistance = Vector2.Distance(touch0.position, touch1.position);
            }
            else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                float currentTouchDistance = Vector2.Distance(touch0.position, touch1.position);
                float delta = currentTouchDistance - initialTouchDistance;

                // Normalize zoom based on screen size
                float normalizedDelta = (delta / Screen.width) * zoomSensitivity;
                
                if (Mathf.Abs(normalizedDelta) > 0.01f)  // Add threshold to prevent too sensitive zooming
                {
                    int zoomDirection = normalizedDelta > 0 ? 1 : -1;
                    SetZoom(zoom + zoomDirection);
                    initialTouchDistance = currentTouchDistance;
                }
            }
        }
    }

    private void SetZoom(int zoomDirection)
    {
        throw new NotImplementedException();
    }

    private IEnumerator GetGoogleMap()
    {
        if (string.IsNullOrEmpty(apiKey)) yield break;

        int width = Mathf.RoundToInt(mapRect.rect.width);
        int height = Mathf.RoundToInt(mapRect.rect.height);
        
        if (width <= 0 || height <= 0) yield break;

        string url = $"https://maps.googleapis.com/maps/api/staticmap" +
                    $"?center={lat},{lon}" +
                    $"&zoom={zoom}" +
                    $"&size={width}x{height}" +
                    "&scale=2" +
                    $"&maptype={mapType}" +
                    $"&key={apiKey}";

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                mapImage.texture = DownloadHandlerTexture.GetContent(www);
                SetMapBoundaries(lat, lon, zoom);
            }
        }
    }

    public void SetMapBoundaries(double centerLat, double centerLon, float zoomLevel)
    {
        double latRange = 180.0 / Math.Pow(2, zoomLevel);
        double lonRange = 360.0 / Math.Pow(2, zoomLevel);

        mapNorthLat = centerLat + (latRange / 2);
        mapSouthLat = centerLat - (latRange / 2);
        mapEastLon = centerLon + (lonRange / 2);
        mapWestLon = centerLon - (lonRange / 2);
    }

    public RectTransform GetMapRect() => mapRect;
    
    public (double north, double south, double east, double west) GetMapBoundaries()
        => (mapNorthLat, mapSouthLat, mapEastLon, mapWestLon);

    public void ResetToInitialPosition()
    {
        lat = initialLat;
        lon = initialLon;
        zoom = initialZoom;
        updateMap = true;
    }
}