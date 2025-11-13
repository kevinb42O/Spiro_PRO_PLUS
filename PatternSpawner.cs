using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime pattern spawner - creates geometric patterns with independent rotors
/// Each generated pattern gets:
/// - A unique parent object at a specific location
/// - An AutoPathTracer to organize the points
/// - A SpirographRoller (rotor) that follows the path
/// </summary>
public class PatternSpawner : MonoBehaviour
{
    [Header("Spawning Configuration")]
    [Tooltip("Spacing between generated patterns")]
    public float patternSpacing = 15f;
    
    [Tooltip("Current spawn position offset")]
    private Vector3 currentSpawnOffset = Vector3.zero;
    
    [Header("Rotor Prefab")]
    [Tooltip("Prefab for the rotor object (must have SpirographRoller component)")]
    public GameObject rotorPrefab;
    
    [Header("Auto-create rotor if no prefab")]
    [Tooltip("If true and no prefab provided, will create a simple sphere rotor")]
    public bool autoCreateRotor = true;
    
    [Header("Default Rotor Settings")]
    public float defaultSpeed = 50f;
    public float defaultRotationSpeed = 0.5f;
    public float defaultPenDistance = 0.3f;
    public int defaultCycles = 50;
    
    [Header("Generated Patterns Tracking")]
    [Tooltip("List of all spawned pattern parents")]
    public List<GameObject> spawnedPatterns = new List<GameObject>();
    
    [Header("Active Rotor Management")]
    [Tooltip("The currently active rotor that the UI controls")]
    public SpirographRoller activeRoller = null;
    
    [Tooltip("List of all spawned rotors")]
    public List<SpirographRoller> allRotors = new List<SpirographRoller>();
    
    private static int patternCounter = 0;
    
    // Events for rotor changes
    public delegate void RotorChangedHandler(SpirographRoller newActiveRotor, SpirographRoller oldRotor);
    public event RotorChangedHandler OnActiveRotorChanged;
    
    void Start()
    {
        // Try to find a rotor prefab in the scene if not assigned
        if (rotorPrefab == null && autoCreateRotor)
        {
            Debug.Log("No rotor prefab assigned. Will auto-create rotors.");
        }
    }
    
    /// <summary>
    /// Spawn a new geometric pattern at runtime
    /// </summary>
    /// <param name="shapeType">The shape to generate</param>
    /// <returns>The parent GameObject containing the pattern and rotor</returns>
    public GameObject SpawnPattern(GeometricPatternGenerator.ShapeType shapeType)
    {
        patternCounter++;
        
        // Create parent container for this pattern
        GameObject patternParent = new GameObject($"Pattern_{patternCounter}_{shapeType}");
        patternParent.transform.position = transform.position + currentSpawnOffset;
        spawnedPatterns.Add(patternParent);
        
        // Create pattern generator container
        GameObject generatorObj = new GameObject("PatternGenerator");
        generatorObj.transform.SetParent(patternParent.transform);
        generatorObj.transform.localPosition = Vector3.zero;
        
        // Add GeometricPatternGenerator
        GeometricPatternGenerator generator = generatorObj.AddComponent<GeometricPatternGenerator>();
        
        // Use reflection to set private fields (they're SerializeField but private)
        var generatorType = typeof(GeometricPatternGenerator);
        
        var shapeTypeField = generatorType.GetField("shapeType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (shapeTypeField != null) shapeTypeField.SetValue(generator, shapeType);
        
        var pointsField = generatorType.GetField("numberOfPoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (pointsField != null) pointsField.SetValue(generator, 64);
        
        var radiusField = generatorType.GetField("radius", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (radiusField != null) radiusField.SetValue(generator, 50f); // 10x larger (was 5f, now 50f)
        
        var createParentField = generatorType.GetField("createParentObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (createParentField != null) createParentField.SetValue(generator, true);
        
        var keepPreviousField = generatorType.GetField("keepPreviousGenerations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (keepPreviousField != null) keepPreviousField.SetValue(generator, false);
        
        // Generate the pattern immediately
        generator.GeneratePattern();
        
        // Wait one frame for pattern to fully generate, then setup rotor
        StartCoroutine(SetupRotorAfterGeneration(patternParent, generatorObj));
        
        // Update spawn position for next pattern (spiral outward)
        UpdateSpawnPosition();
        
        Debug.Log($"✓ Spawned {shapeType} at {patternParent.transform.position}");
        
        return patternParent;
    }
    
    System.Collections.IEnumerator SetupRotorAfterGeneration(GameObject patternParent, GameObject generatorObj)
    {
        Debug.Log($"[PatternSpawner] Coroutine started for {patternParent.name}");
        
        // Wait for pattern generation to complete
        yield return new WaitForEndOfFrame();
        yield return null;
        
        // GeometricPatternGenerator creates the shape parent as a SIBLING in the scene, NOT as a child!
        // It names it like "GeometricShape_1_StarBurst"
        // We need to find it and reparent it
        Transform shapeParent = null;
        int maxAttempts = 5;
        
        for (int attempt = 0; attempt < maxAttempts && shapeParent == null; attempt++)
        {
            // Search for the generated shape in the scene root
            GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            Debug.Log($"[PatternSpawner] Attempt {attempt + 1}: Searching {rootObjects.Length} root objects...");
            
            foreach (GameObject obj in rootObjects)
            {
                // Look for objects with "GeometricShape_" prefix
                if (obj.name.StartsWith("GeometricShape_"))
                {
                    // Check if this is a recently created shape (has empty children list or point children)
                    bool hasPointChildren = false;
                    foreach (Transform child in obj.transform)
                    {
                        if (child.name.StartsWith("Point_"))
                        {
                            hasPointChildren = true;
                            break;
                        }
                    }
                    
                    if (hasPointChildren || obj.transform.childCount > 0)
                    {
                        Debug.Log($"[PatternSpawner] Found shape object: {obj.name} with {obj.transform.childCount} children");
                        shapeParent = obj.transform;
                        
                        // REPARENT IT under our patternParent structure
                        shapeParent.SetParent(generatorObj.transform);
                        shapeParent.localPosition = Vector3.zero;
                        Debug.Log($"[PatternSpawner] ✓ Reparented {obj.name} under {generatorObj.name}");
                        
                        // Add RotateParent component to the shape parent (where path points are)
                        RotateParent rotateParent = obj.AddComponent<RotateParent>();
                        rotateParent.spirographMode = false; // Standard rotation mode
                        rotateParent.useLocalSpace = false; // Use world space for orbit rotation
                        rotateParent.rotationSpeed = new Vector3(0f, 10f, 0f); // Only Y-axis rotation!
                        rotateParent.createUI = false; // UI is managed by SpirographUIManager
                        
                        // Create orbit point 50 units away from pattern center
                        GameObject orbitPoint = new GameObject("OrbitPoint");
                        orbitPoint.transform.position = obj.transform.position + new Vector3(50f, 0f, 0f);
                        rotateParent.rotationPoint = orbitPoint.transform;
                        Debug.Log($"✓ [PatternSpawner] Created orbit point at {orbitPoint.transform.position}");
                        
                        // Set rotationSpeedMultiplier to 0 so pattern doesn't auto-rotate!
                        var multiplierField = typeof(RotateParent).GetField("rotationSpeedMultiplier", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (multiplierField != null)
                        {
                            multiplierField.SetValue(rotateParent, 0f);
                        }
                        
                        Debug.Log($"✓ [PatternSpawner] Added RotateParent to {obj.name} - orbiting around point 50 units away, multiplier at 0 (STILL)");
                        
                        break;
                    }
                }
            }
            
            if (shapeParent == null)
            {
                Debug.Log($"[PatternSpawner] Shape not ready yet, waiting... (attempt {attempt + 1}/{maxAttempts})");
                yield return new WaitForSeconds(0.1f); // Wait a bit longer
            }
        }
        
        if (shapeParent == null)
        {
            Debug.LogError($"[PatternSpawner] Pattern generation FAILED - no GeometricShape_* found in scene after {maxAttempts} attempts!");
            yield break;
        }
        
        Debug.Log($"[PatternSpawner] Found shape: {shapeParent.name}, child count: {shapeParent.childCount}");
        
        // Create or instantiate rotor
        Debug.Log($"[PatternSpawner] Creating rotor (rotorPrefab={rotorPrefab != null}, autoCreateRotor={autoCreateRotor})...");
        GameObject rotor = CreateRotor(patternParent.transform);
        
        if (rotor == null)
        {
            Debug.LogError("[PatternSpawner] Failed to create rotor!");
            yield break;
        }
        
        Debug.Log($"[PatternSpawner] Rotor created: {rotor.name}");
        
        // Get SpirographRoller component
        SpirographRoller roller = rotor.GetComponent<SpirographRoller>();
        if (roller == null)
        {
            Debug.LogError("[PatternSpawner] Rotor has no SpirographRoller component!");
            yield break;
        }
        
        Debug.Log($"[PatternSpawner] SpirographRoller component found on rotor");
        
        // Collect path points in their ORIGINAL ORDER (Point_0, Point_1, Point_2, etc.)
        // DO NOT use AutoPathTracer - it reorders points and destroys the pattern!
        List<Transform> orderedPoints = new List<Transform>();
        
        // GeometricPatternGenerator creates points named "Point_0", "Point_1", etc.
        // We need to preserve this exact sequential order
        for (int i = 0; i < shapeParent.childCount; i++)
        {
            Transform child = shapeParent.GetChild(i);
            if (child.name.StartsWith("Point_"))
            {
                orderedPoints.Add(child);
            }
        }
        
        // Sort by point number to ensure correct order (in case children aren't in order)
        orderedPoints.Sort((a, b) => {
            int numA = ExtractPointNumber(a.name);
            int numB = ExtractPointNumber(b.name);
            return numA.CompareTo(numB);
        });
        
        // Assign path points to roller in correct sequential order
        roller.pathPoints = orderedPoints.ToArray();
        Debug.Log($"✓ [PatternSpawner] Assigned {orderedPoints.Count} path points in SEQUENTIAL ORDER (preserving original pattern)");
        
        // Configure roller settings - START WITH EVERYTHING AT 0 (COMPLETELY STILL!)
        roller.speed = 0f;              // NO movement
        roller.rotationSpeed = 0f;      // NO rotation
        roller.penDistance = 0f;        // NO pen arm extension
        roller.cycles = 1;              // Minimum cycles (won't matter until speed > 0)
        roller.penOffset = Vector3.zero; // NO pen offset
        roller.useWorldSpacePath = true; // CRITICAL: Use world space so pattern isn't affected by parent rotation
        
        // CRITICAL: Force the roller to recalculate its path cache with the newly assigned points
        // The roller's Start() may have already run with empty pathPoints, so we need to refresh
        roller.SendMessage("CacheStaticPath", SendMessageOptions.DontRequireReceiver);
        Debug.Log($"✓ [PatternSpawner] Path cache recalculated with {orderedPoints.Count} points");
        
        Debug.Log($"[PatternSpawner] ⚠️ NEW ROTOR COMPLETELY STILL - ALL values at 0! Use UI sliders to animate!");
        
        int pathPointCount = roller.pathPoints != null ? roller.pathPoints.Length : 0;
        Debug.Log($"✓ [PatternSpawner] Rotor FULLY configured for {patternParent.name} with {pathPointCount} path points!");
        Debug.Log($"  → Rotor position: {rotor.transform.position}, visible: {rotor.activeSelf}");
        
        // === ROTOR MANAGEMENT SYSTEM ===
        if (roller != null)
        {
            // Store reference to this rotor
            allRotors.Add(roller);
            
            // Store old active rotor reference
            SpirographRoller oldRotor = activeRoller;
            
            // Set this as the new active rotor
            activeRoller = roller;
            
            // Notify listeners (UI, Camera) about the rotor change
            OnActiveRotorChanged?.Invoke(activeRoller, oldRotor);
            
            Debug.Log($"✓ [PatternSpawner] Active rotor switched! Old rotor continues with its settings, new rotor is now UI-controlled.");
        }
        else
        {
            Debug.LogError("[PatternSpawner] Roller is null - cannot set as active rotor!");
        }
        
        // Switch camera to follow new rotor (only if rotor was successfully created)
        if (rotor != null && rotor.activeSelf)
        {
            CameraController camController = FindFirstObjectByType<CameraController>();
            if (camController != null)
            {
                camController.target = rotor.transform;
                camController.SetCameraMode(CameraController.CameraMode.SmoothFollow);
                Debug.Log($"✓ [PatternSpawner] Camera now following new rotor in SmoothFollow mode!");
            }
            else
            {
                Debug.LogWarning("[PatternSpawner] CameraController not found - camera won't follow new rotor");
            }
        }
        else
        {
            Debug.LogWarning("[PatternSpawner] Rotor not valid - skipping camera update");
        }
    }
    
    GameObject CreateRotor(Transform parent)
    {
        GameObject rotor;
        
        if (rotorPrefab != null)
        {
            // Use provided prefab
            rotor = Instantiate(rotorPrefab, parent);
            rotor.name = "Rotor";
        }
        else if (autoCreateRotor)
        {
            // Auto-create a simple rotor
            rotor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rotor.name = "Rotor";
            rotor.transform.SetParent(parent);
            rotor.transform.localScale = Vector3.one * 0.5f;
            
            // Add SpirographRoller component
            SpirographRoller roller = rotor.AddComponent<SpirographRoller>();
            
            // Make it visually distinct
            Renderer renderer = rotor.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = Color.cyan;
                mat.SetFloat("_Metallic", 0.8f);
                mat.SetFloat("_Glossiness", 0.9f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.cyan * 2f);
                renderer.material = mat;
            }
        }
        else
        {
            Debug.LogError("No rotor prefab and autoCreateRotor is disabled!");
            return null;
        }
        
        rotor.transform.localPosition = Vector3.zero;
        return rotor;
    }
    
    void UpdateSpawnPosition()
    {
        // Spiral outward pattern for nice distribution
        float angle = patternCounter * 137.5f * Mathf.Deg2Rad; // Golden angle for nice distribution
        float radius = Mathf.Sqrt(patternCounter) * patternSpacing;
        
        currentSpawnOffset = new Vector3(
            Mathf.Cos(angle) * radius,
            0f,
            Mathf.Sin(angle) * radius
        );
    }
    
    /// <summary>
    /// Clear all spawned patterns
    /// </summary>
    public void ClearAllPatterns()
    {
        // Clear all trail renderers from all rotors first
        foreach (SpirographRoller roller in allRotors)
        {
            if (roller != null)
            {
                // Clear active trail
                TrailRenderer[] trails = roller.GetComponentsInChildren<TrailRenderer>();
                foreach (TrailRenderer trail in trails)
                {
                    if (trail != null)
                    {
                        trail.Clear();
                        Destroy(trail.gameObject);
                    }
                }
            }
        }
        
        // Destroy all pattern parent GameObjects (this destroys rotors, paths, everything)
        foreach (GameObject pattern in spawnedPatterns)
        {
            if (pattern != null)
            {
                Destroy(pattern);
            }
        }
        
        // Clear all tracking lists
        spawnedPatterns.Clear();
        allRotors.Clear();
        activeRoller = null;
        
        // Reset counters
        patternCounter = 0;
        currentSpawnOffset = Vector3.zero;
        
        // Notify listeners that active rotor is now null
        OnActiveRotorChanged?.Invoke(null, null);
        
        Debug.Log("✓ All patterns, rotors, and trail lines cleared!");
    }
    
    /// <summary>
    /// Clear the most recent pattern
    /// </summary>
    public void ClearLastPattern()
    {
        if (spawnedPatterns.Count > 0)
        {
            GameObject lastPattern = spawnedPatterns[spawnedPatterns.Count - 1];
            if (lastPattern != null)
            {
                Destroy(lastPattern);
            }
            spawnedPatterns.RemoveAt(spawnedPatterns.Count - 1);
            patternCounter--;
            
            Debug.Log("✓ Last pattern cleared");
        }
    }
    
    /// <summary>
    /// Extract point number from name like "Point_42" -> 42
    /// </summary>
    int ExtractPointNumber(string pointName)
    {
        string[] parts = pointName.Split('_');
        if (parts.Length >= 2)
        {
            int num;
            if (int.TryParse(parts[1], out num))
            {
                return num;
            }
        }
        return 0;
    }
}
