using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public float distance = 0.5f;
    public float verticalOffset = -0.15f;
    public float smoothSpeed = 2f;

    void Update()
    {
        Transform cam = Camera.main.transform;
        Vector3 targetPosition = cam.position 
            + cam.forward * distance 
            + Vector3.up * verticalOffset;

        transform.position = Vector3.Lerp(
            transform.position, 
            targetPosition, 
            Time.deltaTime * smoothSpeed
        );

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.LookRotation(transform.position - cam.position),
            Time.deltaTime * smoothSpeed
        );
    }
}