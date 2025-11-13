using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The object to look at or follow")]
    public Transform target;
    
    [Header("Camera Settings")]
    [Range(0.1f, 10f)] public float mouseSensitivity = 3f;
    [Range(1f, 50f)] public float movementSpeed = 15f;
    [Range(0.5f, 20f)] public float zoomSpeed = 5f;
    [Range(0.5f, 100f)] public float minDistance = 2f;
    [Range(5f, 200f)] public float maxDistance = 50f;
    [Range(1f, 20f)] public float smoothFollowSpeed = 8f;
    
    [Header("Advanced Settings")]
    [Range(0f, 1f)] public float movementDamping = 0.85f;
    [Range(0f, 1f)] public float rotationDamping = 0.9f;
    [Range(1f, 3f)] public float sprintMultiplier = 2.5f;
    [Range(0f, 5f)] public float lookAhead = 1.5f;
    [Range(40f, 120f)] public float fieldOfView = 60f;
    [Range(0.1f, 2f)] public float zoomDamping = 0.3f;
    
    [Header("Auto Orbit Settings")]
    [Tooltip("Controlled by Auto Orbit button")]
    public bool autoOrbitEnabled = false;
    [Tooltip("Custom orbit center point (if null, uses main target)")]
    public Transform orbitTarget;
    [Range(-50f, 50f)] public float orbitSpeed = 10f;
    [Range(-89f, 89f)] public float orbitElevation = 20f;
    [Range(0.5f, 100f)] public float orbitDistance = 15f;
    [Tooltip("Orbit follows target movement")]
    public bool orbitFollowsTarget = true;
    [Tooltip("Smoothly transition into orbit mode")]
    [Range(0.1f, 5f)] public float orbitTransitionSpeed = 1f;
    
    [Header("UI")]
    public bool createUI = true;
    
    public enum CameraMode { FreeFly, SmoothFollow, AutoOrbit }
    private CameraMode currentMode = CameraMode.SmoothFollow;
    
    private float currentDistance = 10f;
    private float targetDistance = 10f;
    private float horizontalAngle = 0f;
    private float verticalAngle = 30f;
    private Vector3 targetOffset;
    
    // Smoothing variables for AAA feel
    private Vector3 velocity = Vector3.zero;
    private Vector3 currentVelocity = Vector3.zero;
    private Vector2 rotationVelocity = Vector2.zero;
    private float currentFOV;
    
    // Smart scroll variables
    private float lastScrollTime = 0f;
    private float scrollSpeedMultiplier = 1f;
    private float scrollResetDelay = 2f;
    private int scrollCount = 0;
    
    // Auto orbit variables
    private bool isOrbiting = false;
    private float orbitAngle = 0f;
    private float orbitTransitionProgress = 0f;
    private float storedHorizontalAngle = 0f;
    private float storedVerticalAngle = 0f;
    private float storedDistance = 0f;
    
    private Button lookAtButton;
    private Button smoothFollowButton;
    private Button autoOrbitButton;
    private Text lookAtButtonText;
    private Text smoothFollowButtonText;
    private Text autoOrbitButtonText;
    
    // Input System variables - AAA quality input handling
    private Mouse mouse;
    private Keyboard keyboard;
    
    // UI EventSystem management
    private EventSystem eventSystem;
    
    void Start()
    {
        // Initialize Input System devices - CRITICAL for AAA input handling
        mouse = Mouse.current;
        keyboard = Keyboard.current;
        
        // Find EventSystem to disable it when controlling camera
        eventSystem = EventSystem.current;
        
        if (mouse == null)
        {
            Debug.LogWarning("CameraController: No mouse detected!");
        }
        
        if (keyboard == null)
        {
            Debug.LogWarning("CameraController: No keyboard detected!");
        }
        
        if (target == null)
        {
            // Try to find the target automatically
            GameObject parentObj = GameObject.Find("PathParent");
            if (parentObj != null)
                target = parentObj.transform;
        }
        
        if (target != null)
        {
            // Calculate initial distance
            currentDistance = Vector3.Distance(transform.position, target.position);
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            targetDistance = currentDistance;
            
            // Calculate initial angles
            Vector3 direction = transform.position - target.position;
            horizontalAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            verticalAngle = Mathf.Asin(direction.y / direction.magnitude) * Mathf.Rad2Deg;
            
            targetOffset = target.position;
        }
        
        // Initialize FOV
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            currentFOV = cam.fieldOfView;
            fieldOfView = currentFOV;
        }
        
        ConnectToUI();
    }
    
    void OnEnable()
    {
        // Refresh input devices when enabled
        mouse = Mouse.current;
        keyboard = Keyboard.current;
        eventSystem = EventSystem.current;
    }
    
    void ConnectToUI()
    {
        // Look for UI elements created by SpirographUIManager
        lookAtButton = GameObject.Find("LookAtButton")?.GetComponent<Button>();
        smoothFollowButton = GameObject.Find("SmoothFollowButton")?.GetComponent<Button>();
        autoOrbitButton = GameObject.Find("AutoOrbitButton")?.GetComponent<Button>();
        
        if (lookAtButton != null)
        {
            lookAtButtonText = lookAtButton.GetComponentInChildren<Text>();
            lookAtButton.onClick.AddListener(() => SetCameraMode(CameraMode.FreeFly));
        }
        
        if (smoothFollowButton != null)
        {
            smoothFollowButtonText = smoothFollowButton.GetComponentInChildren<Text>();
            smoothFollowButton.onClick.AddListener(() => SetCameraMode(CameraMode.SmoothFollow));
        }
        
        if (autoOrbitButton != null)
        {
            autoOrbitButtonText = autoOrbitButton.GetComponentInChildren<Text>();
            autoOrbitButton.onClick.AddListener(() => ToggleAutoOrbit());
        }
        
        // Set initial button colors
        UpdateButtonColors();
    }
    
    void Update()
    {
        // Smart EventSystem management
        if (eventSystem == null) eventSystem = EventSystem.current;
        
        if (eventSystem != null)
        {
            // Check if we're actively using camera controls (keyboard input)
            bool usingCameraControls = false;
            
            if (keyboard != null && currentMode == CameraMode.FreeFly)
            {
                usingCameraControls = keyboard.zKey.isPressed || keyboard.wKey.isPressed || 
                                     keyboard.sKey.isPressed || keyboard.qKey.isPressed || 
                                     keyboard.aKey.isPressed || keyboard.dKey.isPressed ||
                                     keyboard.spaceKey.isPressed || keyboard.leftCtrlKey.isPressed ||
                                     keyboard.rightCtrlKey.isPressed || keyboard.leftShiftKey.isPressed;
            }
            
            // Disable EventSystem ONLY when actively using camera controls with keyboard
            // This allows mouse to work on UI when you're not pressing keys
            eventSystem.enabled = !usingCameraControls;
        }
    }
    
    void LateUpdate()
    {
        // Handle auto orbit mode
        if (isOrbiting)
        {
            UpdateAutoOrbitMode();
        }
        else
        {
            // Apply normal camera mode
            if (currentMode == CameraMode.FreeFly)
            {
                UpdateFreeFlyMode();
            }
            else
            {
                UpdateSmoothFollowMode();
            }
        }
    }
    
    void ToggleAutoOrbit()
    {
        isOrbiting = !isOrbiting;
        
        if (isOrbiting)
        {
            // Store current camera state for smooth transition
            storedHorizontalAngle = horizontalAngle;
            storedVerticalAngle = verticalAngle;
            storedDistance = currentDistance;
            orbitAngle = horizontalAngle;
            orbitTransitionProgress = 0f;
            
            Debug.Log("Auto Orbit: ENABLED");
        }
        else
        {
            // Restore camera angles when exiting orbit
            horizontalAngle = orbitAngle;
            verticalAngle = Mathf.Lerp(verticalAngle, storedVerticalAngle, 0.5f);
            orbitTransitionProgress = 0f;
            
            Debug.Log("Auto Orbit: DISABLED");
        }
    }
    
    void UpdateAutoOrbitMode()
    {
        // Determine which target to orbit around
        Transform activeOrbitTarget = orbitTarget != null ? orbitTarget : target;
        
        if (activeOrbitTarget == null) return;
        
        // Smooth transition into orbit mode
        orbitTransitionProgress = Mathf.Min(orbitTransitionProgress + Time.deltaTime * orbitTransitionSpeed, 1f);
        
        // Update orbit angle continuously
        orbitAngle += orbitSpeed * Time.deltaTime;
        
        // Normalize angle to 0-360
        if (orbitAngle > 360f) orbitAngle -= 360f;
        if (orbitAngle < 0f) orbitAngle += 360f;
        
        // Scroll wheel zoom in orbit mode
        if (mouse != null)
        {
            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                orbitDistance -= scroll * zoomSpeed * 0.05f;
                orbitDistance = Mathf.Clamp(orbitDistance, minDistance, maxDistance);
            }
        }
        
        // Smooth transition between current angle and orbit angle
        float currentHorizontal = Mathf.LerpAngle(storedHorizontalAngle, orbitAngle, orbitTransitionProgress);
        float currentVertical = Mathf.Lerp(storedVerticalAngle, orbitElevation, orbitTransitionProgress);
        float currentDist = Mathf.Lerp(storedDistance, orbitDistance, orbitTransitionProgress);
        
        // Calculate orbit center position
        Vector3 orbitCenter = orbitFollowsTarget && activeOrbitTarget != null ? activeOrbitTarget.position : targetOffset;
        
        // Smooth follow if following target
        if (orbitFollowsTarget)
        {
            targetOffset = Vector3.Lerp(targetOffset, orbitCenter, smoothFollowSpeed * Time.deltaTime);
        }
        
        // Calculate camera position using spherical coordinates
        float radH = currentHorizontal * Mathf.Deg2Rad;
        float radV = currentVertical * Mathf.Deg2Rad;
        
        Vector3 offset = new Vector3(
            Mathf.Sin(radH) * Mathf.Cos(radV),
            Mathf.Sin(radV),
            Mathf.Cos(radH) * Mathf.Cos(radV)
        );
        
        Vector3 desiredPosition = targetOffset + offset * currentDist;
        
        // Smooth camera movement
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 0.15f);
        
        // Smooth look-at (always look at the orbit center)
        Quaternion targetRotation = Quaternion.LookRotation(targetOffset - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 8f * Time.deltaTime);
        
        // Allow manual interruption with mouse
        if (mouse != null && mouse.rightButton.wasPressedThisFrame)
        {
            ToggleAutoOrbit();
        }
        
        // Dynamic FOV
        UpdateFieldOfView(false);
    }
    
    void UpdateFreeFlyMode()
    {
        // Unity Editor-style scene navigation with MOUSE ONLY controls
        
        // Refresh input devices if null (hot-plugging support)
        if (mouse == null) mouse = Mouse.current;
        if (keyboard == null) keyboard = Keyboard.current;
        if (eventSystem == null) eventSystem = EventSystem.current;
        
        // Check if mouse is over UI - if so, don't control camera with mouse
        bool isMouseOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        
        // MOUSE ONLY CONTROLS - Unity Editor Style
        bool isControllingCamera = false;
        
        if (mouse != null && !isMouseOverUI)
        {
            isControllingCamera = mouse.rightButton.isPressed || mouse.middleButton.isPressed || 
                                 (keyboard != null && keyboard.leftAltKey.isPressed && mouse.leftButton.isPressed);
        }
        
        // COMPLETELY DISABLE EventSystem when controlling camera to prevent ALL UI interference
        if (eventSystem != null)
        {
            eventSystem.enabled = !isControllingCamera;
        }
        
        // ========== UNITY EDITOR-STYLE MOUSE CONTROLS ==========
        
        if (mouse != null && !isMouseOverUI)
        {
            bool isAltPressed = keyboard != null && keyboard.leftAltKey.isPressed;
            Vector2 mouseDelta = mouse.delta.ReadValue();
            
            // RIGHT MOUSE BUTTON: Free Look / Rotate view (FPS-style rotation)
            if (mouse.rightButton.isPressed)
            {
                // Apply rotation with smoothing
                rotationVelocity.x += mouseDelta.x * mouseSensitivity * 0.05f;
                rotationVelocity.y += mouseDelta.y * mouseSensitivity * 0.05f;
                
                // Lock cursor while rotating
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            // MIDDLE MOUSE BUTTON: Pan / Drag scene
            else if (mouse.middleButton.isPressed)
            {
                // Pan camera: horizontal mouse = move left/right, vertical mouse = move up/down
                Vector3 panMovement = Vector3.zero;
                panMovement -= transform.right * mouseDelta.x * movementSpeed * 0.015f; // Left/Right
                panMovement -= transform.up * mouseDelta.y * movementSpeed * 0.015f; // Up/Down (relative to camera)
                
                transform.position += panMovement;
                
                // Lock cursor while panning
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            // ALT + LEFT MOUSE BUTTON: Orbit around focus point
            else if (isAltPressed && mouse.leftButton.isPressed)
            {
                // Orbit mode: rotate camera around a focal point
                // Find a focus point (use target if available, otherwise center of scene)
                Vector3 focusPoint = target != null ? target.position : Vector3.zero;
                float distanceToFocus = Vector3.Distance(transform.position, focusPoint);
                
                // Apply orbital rotation
                rotationVelocity.x += mouseDelta.x * mouseSensitivity * 0.05f;
                rotationVelocity.y += mouseDelta.y * mouseSensitivity * 0.05f;
                
                // Lock cursor while orbiting
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                
                // Update angles for orbit
                horizontalAngle += mouseDelta.x * mouseSensitivity * 0.3f;
                verticalAngle -= mouseDelta.y * mouseSensitivity * 0.3f;
                verticalAngle = Mathf.Clamp(verticalAngle, -89f, 89f);
                
                // Calculate new position around focus point
                float radH = horizontalAngle * Mathf.Deg2Rad;
                float radV = verticalAngle * Mathf.Deg2Rad;
                
                Vector3 offset = new Vector3(
                    Mathf.Sin(radH) * Mathf.Cos(radV),
                    Mathf.Sin(radV),
                    Mathf.Cos(radH) * Mathf.Cos(radV)
                );
                
                transform.position = focusPoint + offset * distanceToFocus;
                transform.LookAt(focusPoint);
                
                // Clear rotation velocity since we're manually positioning
                rotationVelocity = Vector2.zero;
                return; // Skip normal rotation code below
            }
            else
            {
                // Show cursor when not holding any mouse button
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else if (!isMouseOverUI)
        {
            // Show cursor when not over UI
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // Apply rotation damping for smooth AAA feel (only for right mouse button rotation)
        if (mouse != null && mouse.rightButton.isPressed && !isMouseOverUI)
        {
            horizontalAngle += rotationVelocity.x;
            verticalAngle -= rotationVelocity.y;
            verticalAngle = Mathf.Clamp(verticalAngle, -89f, 89f);
            rotationVelocity *= rotationDamping;
            
            // Smooth rotation application
            Quaternion targetRotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);
        }
        else
        {
            // Dampen rotation velocity when not actively rotating
            rotationVelocity *= rotationDamping;
        }
        
        // SMART SCROLL ZOOM - Unity Editor style (zoom toward/away from view center)
        if (mouse != null && !isMouseOverUI)
        {
            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                float currentTime = Time.time;
                
                // Check if this scroll is within the acceleration window
                if (currentTime - lastScrollTime < scrollResetDelay)
                {
                    // Rapid scrolling detected - increase speed multiplier
                    scrollCount++;
                    scrollSpeedMultiplier = 1f + (scrollCount * 0.5f); // Each scroll adds 50% speed
                    scrollSpeedMultiplier = Mathf.Min(scrollSpeedMultiplier, 10f); // Cap at 10x speed
                }
                else
                {
                    // First scroll or after delay - reset to normal speed
                    scrollCount = 0;
                    scrollSpeedMultiplier = 1f;
                }
                
                lastScrollTime = currentTime;
                
                // Zoom toward the center of view (Unity Editor style)
                Vector3 zoomVelocity = transform.forward * scroll * zoomSpeed * 0.5f * scrollSpeedMultiplier;
                currentVelocity = Vector3.Lerp(currentVelocity, zoomVelocity, 0.5f);
            }
            else
            {
                // No scroll input - check if we should reset multiplier
                if (Time.time - lastScrollTime > scrollResetDelay)
                {
                    scrollCount = 0;
                    scrollSpeedMultiplier = 1f;
                }
            }
        }
        
        transform.position += currentVelocity * Time.deltaTime;
        currentVelocity *= 0.85f; // Damping
        
        // ========== KEYBOARD MOVEMENT ==========
        // Only allow keyboard movement when NOT controlling camera with mouse
        if (keyboard != null && !isControllingCamera && !isMouseOverUI)
        {
            Vector3 moveDirection = Vector3.zero;
            
            // WASD / ZQSD movement
            if (keyboard.wKey.isPressed || keyboard.zKey.isPressed) // Forward (W or Z for AZERTY)
                moveDirection += transform.forward;
            if (keyboard.sKey.isPressed) // Backward
                moveDirection -= transform.forward;
            if (keyboard.aKey.isPressed || keyboard.qKey.isPressed) // Left (A or Q for AZERTY)
                moveDirection -= transform.right;
            if (keyboard.dKey.isPressed) // Right
                moveDirection += transform.right;
            
            // Vertical movement
            if (keyboard.spaceKey.isPressed) // Up
                moveDirection += Vector3.up;
            if (keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed) // Down
                moveDirection -= Vector3.up;
            
            // Apply movement with sprint modifier
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                float speed = movementSpeed;
                
                // Sprint when holding Shift
                if (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed)
                {
                    speed *= sprintMultiplier;
                }
                
                moveDirection.Normalize();
                currentVelocity = Vector3.Lerp(currentVelocity, moveDirection * speed, 10f * Time.deltaTime);
            }
        }
        
        // Dynamic FOV
        UpdateFieldOfView(false);
    }
    
    void UpdateSmoothFollowMode()
    {
        if (target == null) return;
        
        // Refresh input devices if null (hot-plugging support)
        if (mouse == null) mouse = Mouse.current;
        if (keyboard == null) keyboard = Keyboard.current;
        
        // Mouse look with smooth rotation
        if (mouse != null && mouse.rightButton.isPressed)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();
            
            // Apply rotation with smoothing (reduced sensitivity for less twitchy feel)
            rotationVelocity.x += mouseDelta.x * mouseSensitivity * 0.03f;
            rotationVelocity.y += mouseDelta.y * mouseSensitivity * 0.03f;
            
            // Hide cursor while rotating
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // Show cursor when not rotating
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // Apply rotation damping for AAA feel
        horizontalAngle += rotationVelocity.x;
        verticalAngle -= rotationVelocity.y;
        verticalAngle = Mathf.Clamp(verticalAngle, -89f, 89f);
        rotationVelocity *= rotationDamping;
        
        // Smooth zoom with scroll wheel
        if (mouse != null)
        {
            float scroll = mouse.scroll.ReadValue().y;
            targetDistance -= scroll * zoomSpeed * 0.05f;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }
        
        // Smooth distance interpolation for AAA feel
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, zoomDamping);
        
        // Smooth follow with look-ahead prediction
        Vector3 targetPosition = target.position;
        
        // Look-ahead: predict where target is going
        if (lookAhead > 0f)
        {
            Rigidbody targetRb = target.GetComponent<Rigidbody>();
            if (targetRb != null && targetRb.linearVelocity.magnitude > 0.1f)
            {
                targetPosition += targetRb.linearVelocity.normalized * lookAhead;
            }
        }
        
        targetOffset = Vector3.Lerp(targetOffset, targetPosition, smoothFollowSpeed * Time.deltaTime);
        
        // Calculate camera position with spherical coordinates
        float radH = horizontalAngle * Mathf.Deg2Rad;
        float radV = verticalAngle * Mathf.Deg2Rad;
        
        Vector3 offset = new Vector3(
            Mathf.Sin(radH) * Mathf.Cos(radV),
            Mathf.Sin(radV),
            Mathf.Cos(radH) * Mathf.Cos(radV)
        );
        
        Vector3 desiredPosition = targetOffset + offset * currentDistance;
        
        // Smooth position interpolation for AAA feel
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 0.1f);
        
        // Smooth look-at with slight lag for cinematic feel
        Quaternion targetRotation = Quaternion.LookRotation(targetOffset - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        
        // Dynamic FOV
        UpdateFieldOfView(false);
    }
    
    void UpdateFieldOfView(bool isSprinting)
    {
        // Dynamic FOV for AAA feel - increase when sprinting
        float targetFOV = fieldOfView;
        if (isSprinting)
        {
            targetFOV = fieldOfView * 1.15f; // 15% increase when sprinting
        }
        
        // Smooth FOV transition
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, 5f * Time.deltaTime);
        
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.fieldOfView = currentFOV;
        }
    }
    
    public void SetCameraMode(CameraMode mode)
    {
        // If requesting AutoOrbit mode, toggle orbit instead of switching internal follow/freefly
        if (mode == CameraMode.AutoOrbit)
        {
            ToggleAutoOrbit();
            return;
        }
        // Preserve current camera orientation when switching modes
        Vector3 currentEuler = transform.rotation.eulerAngles;
        
        // Normalize angles to -180 to 180 range
        horizontalAngle = currentEuler.y;
        verticalAngle = currentEuler.x;
        if (verticalAngle > 180f) verticalAngle -= 360f;
        
        // Reset rotation velocity to avoid sudden movements
        rotationVelocity = Vector2.zero;
        
        // If switching to follow mode, set distance based on current position
        if (mode == CameraMode.SmoothFollow && target != null)
        {
            currentDistance = Vector3.Distance(transform.position, target.position);
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            targetDistance = currentDistance;
            targetOffset = target.position;
        }
        
        currentMode = mode;
        UpdateButtonColors();
    }
    
    void UpdateButtonColors()
    {
        // Update button colors
        if (lookAtButton != null && smoothFollowButton != null)
        {
            ColorBlock lookAtColors = lookAtButton.colors;
            ColorBlock smoothFollowColors = smoothFollowButton.colors;
            
            if (currentMode == CameraMode.FreeFly)
            {
                lookAtColors.normalColor = new Color(0.3f, 0.7f, 0.3f, 0.8f);
                smoothFollowColors.normalColor = new Color(0.2f, 0.5f, 0.8f, 0.8f);
            }
            else
            {
                lookAtColors.normalColor = new Color(0.2f, 0.5f, 0.8f, 0.8f);
                smoothFollowColors.normalColor = new Color(0.3f, 0.7f, 0.3f, 0.8f);
            }
            
            lookAtButton.colors = lookAtColors;
            smoothFollowButton.colors = smoothFollowColors;
        }
        
        // Update auto orbit button color
        if (autoOrbitButton != null)
        {
            ColorBlock orbitColors = autoOrbitButton.colors;
            
            if (isOrbiting)
            {
                orbitColors.normalColor = new Color(1f, 0.6f, 0f, 0.8f); // Orange when active
            }
            else
            {
                orbitColors.normalColor = new Color(0.2f, 0.5f, 0.8f, 0.8f); // Blue when inactive
            }
            
            autoOrbitButton.colors = orbitColors;
        }
    }
}
