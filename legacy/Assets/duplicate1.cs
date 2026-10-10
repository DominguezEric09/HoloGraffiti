using UnityEngine;
using MixedReality.Toolkit.SpatialManipulation;

public class duplicate1 : MonoBehaviour
{
    [SerializeField] private GameObject cubeToDuplicate;

    // How far in front of the camera the cube spawns (in meters)
    [SerializeField] private float spawnDistance = 0.5f;

    public void DuplicateCube()
    {
        if (cubeToDuplicate != null)
        {
            GameObject newCube = Instantiate(cubeToDuplicate);

            // Grab the main camera (your HoloLens 2 head/gaze origin)
            Camera cam = Camera.main;

            // Place the cube in front of where you're looking
            Vector3 spawnPos = cam.transform.position + cam.transform.forward * spawnDistance;

            // Optional: slight downward offset so it's not directly in your face
            spawnPos += new Vector3(0f, -0.1f, 0f);

            newCube.transform.position = spawnPos;

            // Face the cube toward the user
            newCube.transform.rotation = Quaternion.LookRotation(
                newCube.transform.position - cam.transform.position
            );

            ObjectManipulator manipulator = newCube.GetComponent<ObjectManipulator>();
            if (manipulator == null)
            {
                newCube.AddComponent<ObjectManipulator>();
            }

            Debug.Log($"Cube spawned at {spawnPos}, {spawnDistance}m in front of camera.");
        }
    }
}