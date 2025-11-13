using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Individual agent that follows a shared path state.
/// Multiple agents can draw simultaneously, each maintaining their own position.
/// This is similar to SpirographRoller but reads from SharedPathState instead of direct control.
/// </summary>
public class PathAgent : MonoBehaviour
{
    [Header("Agent Identity")]
    public int agentIndex = 0;
    public string agentName = "Agent 0";
    
    [Header("Shared State Reference")]
    [Tooltip("The shared state this agent reads from")]
    public SharedPathState sharedState;
    
    [Header("Agent-Specific Settings")]
    [Tooltip("Starting position on the path (0-1, where 0 is start, 1 is end)")]
    [Range(0f, 1f)]
    public float startPositionPercent = 0f;
    
    [Tooltip("Speed multiplier for competitive mode (1.0 = normal)")]
    [Range(0.5f, 2f)]
    public float speedMultiplier = 1f;
    
    [Tooltip("Individual pause state (doesn't affect other agents)")]
    public bool isPaused = false;
    
    [Header("Agent State")]
    public AgentStatus status = AgentStatus.Idle;
    public float currentDistance = 0f;
    public int currentCycle = 0;
    public Vector3 currentPosition = Vector3.zero;
    
    [Header("Agent Color")]
    public Color agentColor = Color.cyan;
    
    [Header("Progress Tracking")]
    [Tooltip("Segment this agent is responsible for (0-1 range)")]
    public float segmentStart = 0f;
    public float segmentEnd = 1f;
    
    [Tooltip("Progress within assigned segment (0-1)")]
    public float segmentProgress = 0f;
    
    [Tooltip("Total distance traveled by this agent")]
    public float totalDistanceTraveled = 0f;
    
    [Tooltip("Time elapsed since agent started")]
    public float elapsedTime = 0f;
    
    public enum AgentStatus
    {
        Idle,       // Not started yet
        Active,     // Currently drawing
        Paused,     // Temporarily stopped
        Completed   // Finished all cycles
    }
    
    // Internal state
    private List<Vector3> staticPathCache = new List<Vector3>();
    private float totalPathLength = 0f;
    private float currentAngle = 0f;
    private Vector3 previousPosition = Vector3.zero;
    private GameObject penObject;
    private GameObject penDotVisual;
    private LineRenderer radiusLine;
    private TrailRenderer trailRenderer;
    private float baseRotorRadius;
    private Vector3 startWorldPosition;
    
    void Start()
    {
        // Validate shared state
        if (sharedState == null)
        {
            Debug.LogError($"[PathAgent] Agent {agentIndex}: No SharedPathState assigned!");
            enabled = false;
            return;
        }
        
        if (sharedState.pathPoints == null || sharedState.pathPoints.Length == 0)
        {
            Debug.LogError($"[PathAgent] Agent {agentIndex}: No path points in SharedPathState!");
            enabled = false;
            return;
        }
        
        // Cache the path
        CacheStaticPath();
        
        // Calculate total path length
        CalculateTotalPathLength();
        
        // Set starting position based on percentage
        currentDistance = startPositionPercent * totalPathLength;
        
        // Get rotor radius
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            baseRotorRadius = renderer.bounds.extents.x;
        }
        else
        {
            baseRotorRadius = 0.5f; // Default fallback
        }
        
        // Create pen object
        CreatePenObject();
        
        // Create visual indicators
        CreatePenDotVisual();
        CreateRadiusLine();
        
        // Create trail renderer
        CreateTrailRenderer();
        
        // Set initial position
        startWorldPosition = GetPointOnPath(currentDistance);
        transform.position = startWorldPosition;
        previousPosition = startWorldPosition;
        currentPosition = startWorldPosition;
        
        // Set initial status
        status = AgentStatus.Idle;
        
        Debug.Log($"[PathAgent] Agent {agentIndex} initialized at {startPositionPercent:P0} of path");
    }
    
    void Update()
    {
        if (sharedState == null) return;
        
        // Check if we should be drawing
        if (status == AgentStatus.Idle || status == AgentStatus.Completed || isPaused)
        {
            if (isPaused && status == AgentStatus.Active)
            {
                status = AgentStatus.Paused;
            }
            return;
        }
        
        // Update elapsed time
        elapsedTime += Time.deltaTime;
        
        // Read motion parameters from shared state
        float speed = sharedState.masterSpeed * speedMultiplier;
        float rotationSpeed = sharedState.masterRotationSpeed;
        float penDistance = sharedState.masterPenDistance;
        int targetCycles = sharedState.masterCycles;
        
        // Calculate movement for this frame
        float frameDistance = speed * Time.deltaTime;
        currentDistance += frameDistance;
        totalDistanceTraveled += frameDistance;
        
        // Handle cycle completion
        if (currentDistance >= totalPathLength)
        {
            currentCycle++;
            currentDistance = currentDistance % totalPathLength;
            
            if (currentCycle >= targetCycles)
            {
                status = AgentStatus.Completed;
                currentDistance = 0f;
                Debug.Log($"[PathAgent] Agent {agentIndex} completed {targetCycles} cycles!");
                return;
            }
        }
        
        // Update segment progress
        float segmentLength = (segmentEnd - segmentStart) * totalPathLength;
        float distanceInSegment = currentDistance - (segmentStart * totalPathLength);
        if (segmentLength > 0)
        {
            segmentProgress = Mathf.Clamp01(distanceInSegment / segmentLength);
        }
        
        // Get new position on path
        Vector3 newPosition = GetPointOnPath(currentDistance);
        
        // Update rotation
        float angleIncrement = rotationSpeed * Time.deltaTime * 360f;
        currentAngle -= angleIncrement;
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);
        
        // Move to new position
        transform.position = newPosition;
        currentPosition = newPosition;
        
        // Update pen offset
        UpdatePenOffset(penDistance);
        
        // Update trail position
        if (trailRenderer != null)
        {
            trailRenderer.transform.position = penObject.transform.position;
        }
        
        // Update visual indicators
        UpdateRadiusLine();
        
        previousPosition = newPosition;
    }
    
    void CacheStaticPath()
    {
        staticPathCache.Clear();
        foreach (Transform point in sharedState.pathPoints)
        {
            if (point != null)
            {
                staticPathCache.Add(point.position);
            }
        }
    }
    
    void CalculateTotalPathLength()
    {
        totalPathLength = 0f;
        for (int i = 1; i < staticPathCache.Count; i++)
        {
            totalPathLength += Vector3.Distance(staticPathCache[i - 1], staticPathCache[i]);
        }
        // Add closing segment
        if (staticPathCache.Count > 0)
        {
            totalPathLength += Vector3.Distance(staticPathCache[staticPathCache.Count - 1], staticPathCache[0]);
        }
    }
    
    Vector3 GetPointOnPath(float distance)
    {
        if (staticPathCache.Count == 0) return Vector3.zero;
        
        float accumulatedDistance = 0f;
        for (int i = 1; i < staticPathCache.Count; i++)
        {
            float segmentLength = Vector3.Distance(staticPathCache[i - 1], staticPathCache[i]);
            if (accumulatedDistance + segmentLength >= distance)
            {
                float t = (distance - accumulatedDistance) / segmentLength;
                return Vector3.Lerp(staticPathCache[i - 1], staticPathCache[i], t);
            }
            accumulatedDistance += segmentLength;
        }
        
        // Handle wrap-around
        if (staticPathCache.Count > 0)
        {
            float segmentLength = Vector3.Distance(staticPathCache[staticPathCache.Count - 1], staticPathCache[0]);
            if (accumulatedDistance + segmentLength >= distance)
            {
                float t = (distance - accumulatedDistance) / segmentLength;
                return Vector3.Lerp(staticPathCache[staticPathCache.Count - 1], staticPathCache[0], t);
            }
        }
        
        return staticPathCache.Count > 0 ? staticPathCache[0] : Vector3.zero;
    }
    
    void CreatePenObject()
    {
        penObject = new GameObject("Pen");
        penObject.transform.SetParent(transform);
        penObject.transform.localPosition = Vector3.zero;
    }
    
    void UpdatePenOffset(float penDistance)
    {
        if (penObject != null)
        {
            float actualDistance = penDistance * baseRotorRadius;
            penObject.transform.localPosition = new Vector3(actualDistance, 0, 0);
        }
    }
    
    void CreatePenDotVisual()
    {
        if (penObject == null) return;
        
        penDotVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        penDotVisual.name = $"PenDot_Agent{agentIndex}";
        penDotVisual.transform.SetParent(penObject.transform);
        penDotVisual.transform.localPosition = Vector3.zero;
        penDotVisual.transform.localScale = Vector3.one * 0.12f;
        
        // Remove collider
        Collider collider = penDotVisual.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        
        // Create glowing material
        Renderer renderer = penDotVisual.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = agentColor;
            mat.SetFloat("_Metallic", 0f);
            mat.SetFloat("_Glossiness", 1f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", agentColor * 3f);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            renderer.material = mat;
        }
    }
    
    void CreateRadiusLine()
    {
        GameObject lineObj = new GameObject($"RadiusLine_Agent{agentIndex}");
        lineObj.transform.SetParent(transform);
        lineObj.transform.localPosition = Vector3.zero;
        
        radiusLine = lineObj.AddComponent<LineRenderer>();
        radiusLine.positionCount = 2;
        radiusLine.startWidth = 0.03f;
        radiusLine.endWidth = 0.03f;
        radiusLine.useWorldSpace = false;
        
        Material lineMat = new Material(Shader.Find("Standard"));
        lineMat.color = agentColor;
        lineMat.SetFloat("_Metallic", 0f);
        lineMat.SetFloat("_Glossiness", 0.8f);
        lineMat.EnableKeyword("_EMISSION");
        lineMat.SetColor("_EmissionColor", agentColor * 1.5f);
        lineMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        radiusLine.material = lineMat;
    }
    
    void UpdateRadiusLine()
    {
        if (radiusLine != null && penObject != null)
        {
            radiusLine.SetPosition(0, Vector3.zero);
            radiusLine.SetPosition(1, penObject.transform.localPosition);
        }
    }
    
    void CreateTrailRenderer()
    {
        GameObject trailObj = new GameObject($"Trail_Agent{agentIndex}");
        trailObj.transform.position = penObject != null ? penObject.transform.position : transform.position;
        
        trailRenderer = trailObj.AddComponent<TrailRenderer>();
        trailRenderer.time = 1000f;
        trailRenderer.startWidth = sharedState.masterLineWidth;
        trailRenderer.endWidth = sharedState.masterLineWidth;
        trailRenderer.minVertexDistance = 0.01f;
        trailRenderer.numCornerVertices = 5;
        trailRenderer.numCapVertices = 5;
        
        // Create material
        Material trailMat = new Material(Shader.Find("Particles/Standard Unlit"));
        trailMat.color = agentColor;
        if (trailMat.HasProperty("_EmissionColor"))
        {
            trailMat.EnableKeyword("_EMISSION");
            trailMat.SetColor("_EmissionColor", agentColor * 0.5f);
        }
        trailRenderer.material = trailMat;
    }
    
    /// <summary>
    /// Start this agent's drawing
    /// </summary>
    public void StartDrawing()
    {
        if (status == AgentStatus.Idle || status == AgentStatus.Paused)
        {
            status = AgentStatus.Active;
            isPaused = false;
            Debug.Log($"[PathAgent] Agent {agentIndex} started drawing");
        }
    }
    
    /// <summary>
    /// Pause this agent
    /// </summary>
    public void Pause()
    {
        if (status == AgentStatus.Active)
        {
            isPaused = true;
            status = AgentStatus.Paused;
            Debug.Log($"[PathAgent] Agent {agentIndex} paused");
        }
    }
    
    /// <summary>
    /// Resume this agent
    /// </summary>
    public void Resume()
    {
        if (status == AgentStatus.Paused)
        {
            isPaused = false;
            status = AgentStatus.Active;
            Debug.Log($"[PathAgent] Agent {agentIndex} resumed");
        }
    }
    
    /// <summary>
    /// Reset this agent to starting position
    /// </summary>
    public void ResetAgent()
    {
        currentDistance = startPositionPercent * totalPathLength;
        currentCycle = 0;
        currentAngle = 0f;
        elapsedTime = 0f;
        totalDistanceTraveled = 0f;
        segmentProgress = 0f;
        status = AgentStatus.Idle;
        isPaused = false;
        
        transform.position = GetPointOnPath(currentDistance);
        transform.rotation = Quaternion.identity;
        
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
        
        Debug.Log($"[PathAgent] Agent {agentIndex} reset");
    }
    
    /// <summary>
    /// Update agent color (also updates visual elements)
    /// </summary>
    public void SetColor(Color newColor)
    {
        agentColor = newColor;
        
        // Update pen dot
        if (penDotVisual != null)
        {
            Renderer renderer = penDotVisual.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = newColor;
                renderer.material.SetColor("_EmissionColor", newColor * 3f);
            }
        }
        
        // Update radius line
        if (radiusLine != null && radiusLine.material != null)
        {
            radiusLine.material.color = newColor;
            radiusLine.material.SetColor("_EmissionColor", newColor * 1.5f);
        }
        
        // Update trail
        if (trailRenderer != null && trailRenderer.material != null)
        {
            trailRenderer.material.color = newColor;
            if (trailRenderer.material.HasProperty("_EmissionColor"))
            {
                trailRenderer.material.SetColor("_EmissionColor", newColor * 0.5f);
            }
        }
    }
    
    /// <summary>
    /// Update trail width from shared state
    /// </summary>
    public void UpdateLineWidth(float width)
    {
        if (trailRenderer != null)
        {
            trailRenderer.startWidth = width;
            trailRenderer.endWidth = width;
        }
    }
    
    /// <summary>
    /// Highlight this agent's trail (pulse effect)
    /// </summary>
    public void HighlightTrail(bool highlight)
    {
        if (trailRenderer != null && trailRenderer.material != null)
        {
            if (highlight)
            {
                // Increase emission for highlight effect
                if (trailRenderer.material.HasProperty("_EmissionColor"))
                {
                    trailRenderer.material.SetColor("_EmissionColor", agentColor * 2f);
                }
                // Slightly increase width
                float baseWidth = sharedState != null ? sharedState.masterLineWidth : 0.3f;
                trailRenderer.startWidth = baseWidth * 1.5f;
                trailRenderer.endWidth = baseWidth * 1.5f;
            }
            else
            {
                // Reset to normal
                if (trailRenderer.material.HasProperty("_EmissionColor"))
                {
                    trailRenderer.material.SetColor("_EmissionColor", agentColor * 0.5f);
                }
                // Reset width
                float baseWidth = sharedState != null ? sharedState.masterLineWidth : 0.3f;
                trailRenderer.startWidth = baseWidth;
                trailRenderer.endWidth = baseWidth;
            }
        }
    }
}
