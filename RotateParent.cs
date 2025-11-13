using UnityEngine;
using UnityEngine.UI;

public class RotateParent : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Rotation speed in degrees per second")]
    public Vector3 rotationSpeed = new Vector3(10, 10, 10);
    
    [Tooltip("Rotate in local space or world space")]
    public bool useLocalSpace = true;
    
    [Header("Rotation Point")]
    [Tooltip("Custom point to rotate around (leave empty to use object's pivot)")]
    public Transform rotationPoint;
    
    [Header("Spirograph Mode")]
    [Tooltip("When enabled, only rotates visual representation - path remains stationary in world space")]
    public bool spirographMode = false;
    
    [Header("UI Controls")]
    [Tooltip("Create UI automatically on start")]
    public bool createUI = true;
    
    private Slider rotationSpeedSlider;
    private Text rotationSpeedText;
    private float rotationSpeedMultiplier = 1f;
    
    void Start()
    {
        ConnectToUI();
        Debug.Log($"RotateParent initialized - rotationSpeed: {rotationSpeed}, multiplier: {rotationSpeedMultiplier}, spirographMode: {spirographMode}");
    }
    
    void ConnectToUI()
    {
        // Look for UI element created by SpirographUIManager
        rotationSpeedSlider = GameObject.Find("ObjectRotationSpeedSlider")?.GetComponent<Slider>();
        
        if (rotationSpeedSlider != null)
        {
            rotationSpeedText = rotationSpeedSlider.GetComponentInChildren<Text>();
            rotationSpeedSlider.value = rotationSpeedMultiplier;
            rotationSpeedSlider.onValueChanged.AddListener((value) => {
                rotationSpeedMultiplier = value;
                if (rotationSpeedText != null)
                    rotationSpeedText.text = "Object Rotation Speed: " + value.ToString("F1");
            });
        }
    }
    
    void Update()
    {
        Vector3 effectiveSpeed = rotationSpeed * rotationSpeedMultiplier;
        
        if (effectiveSpeed.magnitude < 0.01f) return; // Skip if not rotating
        
        if (spirographMode)
        {
            // SPIROGRAPH MODE:
            // In true spirograph mathematics, the guide circle (path) is stationary
            // Only rotate visual meshes, not the logical path points
            // This prevents the rotating reference frame problem
            
            RotateVisualChildrenOnly(effectiveSpeed * Time.deltaTime);
        }
        else
        {
            // STANDARD MODE:
            // Rotate the entire object including all children
            if (rotationPoint != null)
            {
                // Rotate around custom point
                transform.RotateAround(rotationPoint.position, effectiveSpeed.normalized, effectiveSpeed.magnitude * Time.deltaTime);
            }
            else
            {
                // Rotate around own center (pivot point)
                if (useLocalSpace)
                {
                    transform.Rotate(effectiveSpeed * Time.deltaTime, Space.Self);
                }
                else
                {
                    transform.Rotate(effectiveSpeed * Time.deltaTime, Space.World);
                }
            }
            
            // Update the rotor's cached path after rotation
            NotifyRotorOfPathChange();
        }
    }
    
    void NotifyRotorOfPathChange()
    {
        // Find the rotor and tell it to update its cached path
        SpirographRoller rotor = FindFirstObjectByType<SpirographRoller>();
        if (rotor != null && rotor.useWorldSpacePath)
        {
            rotor.SendMessage("CacheStaticPath", SendMessageOptions.DontRequireReceiver);
        }
    }
    
    void RotateVisualChildrenOnly(Vector3 rotation)
    {
        // Rotate all children EXCEPT path points
        // The roller can rotate (it manages its own rotation independently)
        // Only skip objects that define the guide path geometry
        foreach (Transform child in transform)
        {
            // Check if this is a path point (the stationary guide circle)
            bool isPathPoint = child.name.Contains("PathPoint") || 
                             child.name.Contains("Path") ||
                             child.name.Contains("Point");
            
            if (!isPathPoint)
            {
                // Rotate everything else (including roller's visual mesh)
                if (useLocalSpace)
                    child.Rotate(rotation, Space.Self);
                else
                    child.Rotate(rotation, Space.World);
            }
        }
    }
    
}
