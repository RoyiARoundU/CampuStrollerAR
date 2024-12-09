using UnityEngine;
using UnityEngine.UI;
using Google.XR.ARCoreExtensions.GeospatialCreator;
using System.Collections.Generic;
using TMPro;

public class MapManager : MonoBehaviour
{
    [Header("Pin Settings")]
    [SerializeField] private Sprite pinSprite;
    [SerializeField] private float pinSize = 30f;
    [SerializeField] private Color pinColor = Color.red;

    [Header("References")]
    [SerializeField] private MapTileController mapViewController;
    [SerializeField] private Camera arCamera;  // Reference to AR Camera
    [SerializeField] private float maxRange = 20f;  // Maximum range in meters

    [Header("UI Panels")]
    [SerializeField] private GameObject inRangePanel;    // Panel to show when in range
    [SerializeField] private GameObject outOfRangePanel; // Panel to show when out of range

    private Dictionary<ARGeospatialCreatorAnchor, Image> anchorPins = new Dictionary<ARGeospatialCreatorAnchor, Image>();

    void Start()
    {
        if (mapViewController == null)
            mapViewController = FindAnyObjectByType<MapTileController>();
        
        if (arCamera == null)
            arCamera = Camera.main;

        // Initially hide both panels
        if (inRangePanel) inRangePanel.SetActive(false);
        if (outOfRangePanel) outOfRangePanel.SetActive(false);

        RefreshPins();
    }

    public void RefreshPins()
    {
        // Clear existing pins
        foreach (var pin in anchorPins.Values)
        {
            if (pin != null)
            {
                Destroy(pin.gameObject);
            }
        }
        anchorPins.Clear();

        // Find all anchors in the scene
        var anchors = FindObjectsByType<ARGeospatialCreatorAnchor>(FindObjectsSortMode.None);
        Debug.Log($"Found {anchors.Length} anchors in the scene");

        foreach (var anchor in anchors)
        {
            if (anchor != null)
            {
                CreatePinForAnchor(anchor);
                Debug.Log($"Created pin for anchor: {anchor.name} at Lat:{anchor.Latitude}, Lon:{anchor.Longitude}");
            }
        }
    }

    private void CreatePinForAnchor(ARGeospatialCreatorAnchor anchor)
    {
        if (anchor == null || mapViewController == null) return;

        GameObject pinObject = new GameObject($"Pin_{anchor.name}");
        pinObject.transform.SetParent(mapViewController.GetMapRect(), false);

        Image pinImage = pinObject.AddComponent<Image>();
        pinImage.sprite = pinSprite;
        pinImage.color = pinColor;

        RectTransform pinRect = pinImage.GetComponent<RectTransform>();
        pinRect.anchorMin = Vector2.zero;
        pinRect.anchorMax = Vector2.zero;
        pinRect.pivot = new Vector2(0.5f, 0.5f);
        pinRect.sizeDelta = new Vector2(pinSize, pinSize);

        // Add Button and click handler
        Button pinButton = pinObject.AddComponent<Button>();
        pinButton.onClick.AddListener(() => OnPinClicked(anchor));

        anchorPins[anchor] = pinImage;
        UpdatePinPosition(anchor, pinImage);
    }

    private void UpdatePinPosition(ARGeospatialCreatorAnchor anchor, Image pinImage)
    {
        if (anchor == null || pinImage == null || mapViewController == null) return;

        var boundaries = mapViewController.GetMapBoundaries();
        var mapRect = mapViewController.GetMapRect();

        // Calculate normalized position (0-1 range)
        float normalizedX = Mathf.InverseLerp((float)boundaries.west, (float)boundaries.east, (float)anchor.Longitude);
        float normalizedY = Mathf.InverseLerp((float)boundaries.south, (float)boundaries.north, (float)anchor.Latitude);

        // Update pin position
        RectTransform pinRect = pinImage.GetComponent<RectTransform>();
        pinRect.anchoredPosition = new Vector2(
            normalizedX * mapRect.rect.width,
            normalizedY * mapRect.rect.height
        );

        // Debug position
        Debug.Log($"Pin {anchor.name} positioned at ({pinRect.anchoredPosition.x:F2}, {pinRect.anchoredPosition.y:F2})");
    }

    private void OnPinClicked(ARGeospatialCreatorAnchor anchor)
    {
        if (anchor == null || arCamera == null) return;

        // Calculate distance to anchor
        float distance = Vector3.Distance(arCamera.transform.position, anchor.transform.position);
        Debug.Log($"Distance to {anchor.name}: {distance:F2}m");

        // Show appropriate panel based on distance
        if (distance <= maxRange)
        {
            ShowInRangePanel();
            Debug.Log($"In range of {anchor.name}");
        }
        else
        {
            ShowOutOfRangePanel();
            Debug.Log($"Out of range of {anchor.name}. Need to be within {maxRange}m");
        }
    }

    private void ShowInRangePanel()
    {
        if (inRangePanel)
        {
            inRangePanel.SetActive(true);
            outOfRangePanel?.SetActive(false);
        }
    }

    private void ShowOutOfRangePanel()
    {
        if (outOfRangePanel)
        {
            outOfRangePanel.SetActive(true);
            inRangePanel?.SetActive(false);
        }
    }

    public void HidePanels()
    {
        if (inRangePanel) inRangePanel.SetActive(false);
        if (outOfRangePanel) outOfRangePanel.SetActive(false);
    }

    void Update()
    {
        // Update pin positions if map has moved
        foreach (var pair in anchorPins)
        {
            if (pair.Key != null && pair.Value != null)
            {
                UpdatePinPosition(pair.Key, pair.Value);
            }
        }
    }
}
