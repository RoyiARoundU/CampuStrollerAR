using UnityEngine;

public class dissolveEdges : MonoBehaviour
{
    [SerializeField] private float dissolveAmount = 0f;
    [SerializeField] private float edgeWidth = 0.1f;
    private Material material;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Create a new material instance to avoid modifying the shared material
        material = new Material(spriteRenderer.material);
        spriteRenderer.material = material;
        
        // Set initial shader properties
        material.SetFloat("_DissolveAmount", dissolveAmount);
        material.SetFloat("_EdgeWidth", edgeWidth);
    }

    // Update is called once per frame
    void Update()
    {
        // You can animate the dissolve amount here if needed
        material.SetFloat("_DissolveAmount", dissolveAmount);
    }

    private void OnDestroy()
    {
        // Clean up the material when the object is destroyed
        if (material != null)
        {
            Destroy(material);
        }
    }
}
