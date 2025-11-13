using UnityEngine;

/// <summary>
/// Single source of truth for multi-agent spirograph system.
/// All agents read from this shared state to stay synchronized.
/// This component is controlled by UI sliders when multi-agent mode is enabled.
/// </summary>
public class SharedPathState : MonoBehaviour
{
    [Header("Master Motion Settings")]
    [Tooltip("Master travel speed - all agents follow this")]
    [Range(0f, 750f)]
    public float masterSpeed = 50f;
    
    [Tooltip("Master rotation speed - all agents follow this")]
    [Range(0f, 1f)]
    public float masterRotationSpeed = 0.5f;
    
    [Tooltip("Master pen distance - all agents follow this")]
    [Range(0f, 5f)]
    public float masterPenDistance = 0.3f;
    
    [Tooltip("Master cycles count - all agents follow this")]
    [Range(1, 500)]
    public int masterCycles = 50;
    
    [Header("Master Visual Settings")]
    [Tooltip("Master line width - all agents follow this")]
    [Range(0.01f, 2f)]
    public float masterLineWidth = 0.3f;
    
    [Tooltip("Master line brightness - all agents follow this")]
    [Range(0f, 1f)]
    public float masterLineBrightness = 1f;
    
    [Header("Master Color Settings")]
    [Tooltip("Master line color (used in Master color mode)")]
    public Color masterLineColor = Color.cyan;
    
    [Tooltip("Current HSV values for UI synchronization")]
    public float masterHue = 0.5f;
    public float masterSaturation = 0.8f;
    public float masterValue = 1f;
    
    [Header("Path Reference")]
    [Tooltip("The path that all agents will follow")]
    public Transform[] pathPoints;
    
    [Header("Debug Info")]
    [Tooltip("Number of active agents currently reading from this state")]
    public int activeAgentCount = 0;
    
    /// <summary>
    /// Get a color for an agent based on the color mode and agent index
    /// </summary>
    public Color GetAgentColor(MultiAgentManager.AgentColorMode colorMode, int agentIndex, int totalAgents)
    {
        switch (colorMode)
        {
            case MultiAgentManager.AgentColorMode.Master:
                return masterLineColor;
                
            case MultiAgentManager.AgentColorMode.Rainbow:
                // Distribute agents evenly across the hue spectrum
                float hue = (float)agentIndex / totalAgents;
                return Color.HSVToRGB(hue, 0.8f, 1f);
                
            case MultiAgentManager.AgentColorMode.Individual:
                // Each agent gets a distinct color from a predefined palette
                Color[] palette = new Color[] {
                    Color.cyan, Color.magenta, Color.yellow, Color.green,
                    Color.red, Color.blue, new Color(1f, 0.5f, 0f), // orange
                    new Color(0.5f, 0f, 1f), // purple
                    new Color(1f, 0.4f, 0.7f), // pink
                    new Color(0.3f, 0.8f, 0.3f), // lime
                    new Color(1f, 0.8f, 0f), // gold
                    new Color(0.2f, 0.6f, 0.8f), // sky blue
                    new Color(0.8f, 0.2f, 0.4f), // crimson
                    new Color(0.4f, 0.2f, 0.8f), // indigo
                    new Color(0.6f, 0.8f, 0.2f), // chartreuse
                    new Color(0.8f, 0.6f, 0.2f) // amber
                };
                return palette[agentIndex % palette.Length];
                
            case MultiAgentManager.AgentColorMode.Custom:
                // Return master color, but agents can override individually
                return masterLineColor;
                
            default:
                return masterLineColor;
        }
    }
    
    /// <summary>
    /// Update master color from HSV values (called by UI)
    /// </summary>
    public void UpdateMasterColorFromHSV(float h, float s, float v)
    {
        masterHue = h;
        masterSaturation = s;
        masterValue = v;
        masterLineColor = Color.HSVToRGB(h, s, v);
    }
    
    void OnValidate()
    {
        // Ensure color stays in sync with HSV values in editor
        if (Application.isPlaying)
        {
            UpdateMasterColorFromHSV(masterHue, masterSaturation, masterValue);
        }
    }
}
