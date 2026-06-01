using UnityEngine;

public class CarCamera : MonoBehaviour
{
    // The car we're following
    public Transform target;

    // Camera position relative to the car
    // X = left/right
    // Y = height
    // Z = distance behind
    public Vector3 offset = new Vector3(0f, 4f, -8f);

    // How quickly the camera moves to its desired position
    public float followSmoothness = 8f;

    // How quickly the camera rotates to look at the car
    public float rotationSmoothness = 6f;

    // Makes the camera look ahead of the car instead of directly at it
    public float lookAheadDistance = 6f;

    // Slight vertical offset so we're not staring at the wheels
    public float lookHeight = 1.5f;

    private void LateUpdate()
    {
        // Safety check
        if (target == null)
            return;

        // Calculate where the camera SHOULD be
        // TransformDirection converts local offset into world space
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);

        // Smoothly move camera towards desired position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSmoothness * Time.deltaTime);

        // Point the camera slightly ahead of the vehicle
        Vector3 lookTarget = target.position + target.forward * lookAheadDistance + Vector3.up * lookHeight;

        // Calculate desired rotation
        Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);

        // Smoothly rotate camera
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothness * Time.deltaTime);
    }
}
