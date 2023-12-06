using UnityEngine;

public class DroneCamera : MonoBehaviour
{
   public Transform droneTransform; // Reference to the drone's Transform
    public float followSpeed = 5f; // Speed at which the camera follows the drone
    public float distanceBehind = 5f; // Distance behind the drone

    private Vector3 offset; // Offset between camera and drone

    void Start()
    {
        if (droneTransform == null)
        {
            Debug.LogError("Drone Transform not assigned!");
            enabled = false; // Disable the script if droneTransform is not assigned
        }

        // Calculate initial offset between camera and drone
        offset = droneTransform.position - transform.position;
    }

    void LateUpdate()
    {
        // Create a rotation based on the drone's yaw only
        Quaternion droneYawRotation = Quaternion.Euler(0f, droneTransform.eulerAngles.y, 0f);

        // Calculate the desired position for the camera
        Vector3 targetPosition = droneTransform.position - (droneYawRotation * Vector3.forward) * distanceBehind;

        // Smoothly move the camera towards the desired position
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // Make the camera always look at the drone without tilting
        transform.LookAt(droneTransform.position);
    }
}
