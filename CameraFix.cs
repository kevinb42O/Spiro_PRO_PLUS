using UnityEngine;

/// <summary>
/// Emergency camera fix script
/// Add this to any GameObject and it will fix the camera on Start
/// </summary>
public class CameraFix : MonoBehaviour
{
    void Start()
    {
        FixCamera();
    }
    
    [ContextMenu("Fix Camera Now")]
    public void FixCamera()
    {
        CameraController camController = FindFirstObjectByType<CameraController>();
        
        if (camController == null)
        {
            Debug.LogError("CameraController not found!");
            return;
        }
        
        // Find the first ROLLER in scene
        SpirographRoller[] rollers = FindObjectsByType<SpirographRoller>(FindObjectsSortMode.None);
        
        if (rollers != null && rollers.Length > 0)
        {
            // Set target to first roller found
            camController.target = rollers[0].transform;
            Debug.Log($"✓ Camera target set to: {rollers[0].gameObject.name}");
        }
        else
        {
            // No roller found, look for PathParent
            GameObject pathParent = GameObject.Find("PathParent");
            if (pathParent != null)
            {
                camController.target = pathParent.transform;
                Debug.Log($"✓ Camera target set to: PathParent");
            }
            else
            {
                Debug.LogWarning("No suitable target found for camera!");
            }
        }
        
        // Reset to FreeFly mode so you can move around
        camController.SetCameraMode(CameraController.CameraMode.FreeFly);
        
        Debug.Log("✓ Camera fixed! You should be able to move now.");
        Debug.Log("  → Right-click + drag to look around");
        Debug.Log("  → Middle mouse wheel to zoom");
        Debug.Log("  → Hold Shift + right-click to move faster");
    }
}
