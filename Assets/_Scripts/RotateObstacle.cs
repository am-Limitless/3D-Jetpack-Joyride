using UnityEngine;

public class RotateObstacle : MonoBehaviour
{
    public Vector3 rotationAxis = Vector3.right; // Default rotation around the Y-axis
    public float rotationSpeed = 10f; // Adjust speed in the Inspector

    void Update()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}
