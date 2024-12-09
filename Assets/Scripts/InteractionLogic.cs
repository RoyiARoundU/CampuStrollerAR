using UnityEngine;

public class InteractionLogic : MonoBehaviour
{
    private GameObject targetObject;
    private float initialDistance;
    private Vector3 initialScale;
    private float initialAngle;
    private Quaternion initialRotation;
    private bool isDraggingUp = false;
    private float dragStartY;
    private float objectStartY;
    private float elevationSensitivity = 0.001f;
    private bool isDragging = false;
    private Vector2 dragStartPosition;
    private Vector3 objectStartPosition;
    private float dragSensitivity = 0.001f;
    private bool isElevating = false;
    public float elevationThreshold = 50f;

    public void SetTargetObject(GameObject obj)
    {
        targetObject = obj;
    }

    public void SnapToGround()
    {
        if (targetObject != null)
        {
            Vector3 position = targetObject.transform.position;
            position.y = 0;
            targetObject.transform.position = position;
        }
    }

    void Update()
    {
        if (Input.touchCount == 2 && targetObject != null)
        {
            var touch0 = Input.GetTouch(0);
            var touch1 = Input.GetTouch(1);

            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                initialDistance = Vector2.Distance(touch0.position, touch1.position);
                initialScale = targetObject.transform.localScale;

                initialAngle = GetAngleBetweenTouches(touch0.position, touch1.position);
                initialRotation = targetObject.transform.rotation;
            }
            else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                // Scaling
                float currentDistance = Vector2.Distance(touch0.position, touch1.position);
                float scaleFactor = currentDistance / initialDistance;
                targetObject.transform.localScale = initialScale * scaleFactor;

                // Rotation
                float currentAngle = GetAngleBetweenTouches(touch0.position, touch1.position);
                float angleDifference = currentAngle - initialAngle;
                targetObject.transform.rotation = initialRotation * Quaternion.Euler(0, -angleDifference, 0);
            }
        }
        else if (Input.touchCount == 1 && targetObject != null)
        {
            var touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    isDragging = true;
                    isElevating = false;
                    dragStartPosition = touch.position;
                    objectStartPosition = targetObject.transform.position;
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        Vector2 delta = touch.position - dragStartPosition;

                        if (!isElevating && Mathf.Abs(delta.y) > elevationThreshold)
                        {
                            isElevating = true;
                            dragStartPosition = touch.position;
                        }

                        if (isElevating)
                        {
                            float yDelta = (touch.position.y - dragStartPosition.y) * elevationSensitivity;
                            Vector3 newPosition = targetObject.transform.position;
                            newPosition.y = Mathf.Max(objectStartPosition.y + yDelta, 0);
                            targetObject.transform.position = newPosition;
                        }
                        else
                        {
                            Camera camera = Camera.main;
                            Vector3 forward = camera.transform.forward;
                            forward.y = 0;
                            forward.Normalize();
                            Vector3 right = camera.transform.right;
                            right.y = 0;
                            right.Normalize();

                            Vector3 newPosition = objectStartPosition;
                            newPosition += forward * (delta.y * dragSensitivity);
                            newPosition += right * (delta.x * dragSensitivity);
                            newPosition.y = objectStartPosition.y;
                            targetObject.transform.position = newPosition;
                        }
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isDragging = false;
                    isElevating = false;
                    break;
            }
        }
    }

    private float GetAngleBetweenTouches(Vector2 touch0, Vector2 touch1)
    {
        Vector2 direction = touch1 - touch0;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }
}