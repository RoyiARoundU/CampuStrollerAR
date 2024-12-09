using UnityEngine;

public class ShowModelsWithinDistance : MonoBehaviour
{
    [SerializeField] private GameObject models;

    void Update()
    {
        if (models == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, models.transform.position);

        if (distance <= 15.0f)
        {
            models.SetActive(true);
        }
        else
        {
            models.SetActive(false);
        }
    }
}