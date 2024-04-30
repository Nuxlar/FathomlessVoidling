using UnityEngine;

public class OrbitAround : MonoBehaviour
{
    public float orbitSpeed = 10f;   // Speed of orbit

    private Vector3 axisOfRotation; // Axis around which to orbit

    void Start()
    {
        // Calculate the axis of rotation (perpendicular to both up direction and vector to centerObject)
        axisOfRotation = Vector3.Cross(transform.up, transform.position - Vector3.zero).normalized;
    }

    void FixedUpdate()
    {
        // Rotate the object around the centerObject
        transform.RotateAround(Vector3.zero, axisOfRotation, orbitSpeed * Time.deltaTime);
    }
}
