using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages multiple PathAgents drawing simultaneously on a shared path.
/// Handles spawning, color modes, spawn modes, and agent lifecycle.
/// </summary>
public class MultiAgentManager : MonoBehaviour
{
    [Header("Shared State")]
    [Tooltip("The shared state all agents will read from")]
    public SharedPathState sharedState;
    
    [Header("Agent Configuration")]
    [Tooltip("Number of agents to spawn (1-16)")]
    [Range(1, 16)]
    public int agentCount = 1;
    
    [Tooltip("Prefab for agent rotor (must have PathAgent component)")]
    public GameObject agentPrefab;
    
    [Tooltip("Auto-create simple sphere agents if no prefab provided")]
    public bool autoCreateAgents = true;
    
    [Header("Color Mode")]
    public AgentColorMode colorMode = AgentColorMode.Rainbow;
    
    public enum AgentColorMode
    {
        Master,     // All agents use master color
        Rainbow,    // Distribute agents across hue spectrum
        Individual, // Each agent gets a predefined color from palette
        Custom      // Agents can have custom colors set individually
    }
    
    [Header("Spawn Mode")]
    public AgentSpawnMode spawnMode = AgentSpawnMode.Simultaneous;
    
    public enum AgentSpawnMode
    {
        Simultaneous,   // All agents start at once, same position
        Sequential,     // Agents start one after another with delay
        Staggered,      // Agents distributed evenly along path
        Competitive     // Agents start together, race with speed variations
    }
    
    [Header("Spawn Configuration")]
    [Tooltip("Delay between agent spawns in Sequential mode (seconds)")]
    public float sequentialDelay = 0.5f;
    
    [Tooltip("Speed variation for Competitive mode (±%)")]
    [Range(0f, 0.5f)]
    public float competitiveSpeedVariation = 0.2f;
    
    [Header("Active Agents")]
    [Tooltip("List of all spawned agents")]
    public List<PathAgent> agents = new List<PathAgent>();
    
    [Tooltip("Currently selected agent (for camera follow and UI)")]
    public PathAgent selectedAgent = null;
    
    [Header("Global Stats")]
    public int activeAgentCount = 0;
    public int pausedAgentCount = 0;
    public int completedAgentCount = 0;
    public float averageProgress = 0f;
    public float totalDistanceCovered = 0f;
    
    [Header("Multi-Agent Mode")]
    [Tooltip("Is multi-agent mode currently enabled?")]
    public bool isMultiAgentMode = false;
    
    // Events
    public delegate void AgentEventHandler(PathAgent agent);
    public event AgentEventHandler OnAgentSelected;
    public event AgentEventHandler OnAgentCompleted;
    
    void Start()
    {
        // Validate shared state
        if (sharedState == null)
        {
            Debug.LogError("[MultiAgentManager] No SharedPathState assigned! Creating one...");
            GameObject stateObj = new GameObject("SharedPathState");
            sharedState = stateObj.AddComponent<SharedPathState>();
        }
    }
    
    void Update()
    {
        if (!isMultiAgentMode || agents.Count == 0) return;
        
        // Update global stats
        UpdateGlobalStats();
    }
    
    /// <summary>
    /// Enable multi-agent mode and spawn agents
    /// </summary>
    public void EnableMultiAgentMode()
    {
        if (isMultiAgentMode)
        {
            Debug.LogWarning("[MultiAgentManager] Multi-agent mode already enabled");
            return;
        }
        
        isMultiAgentMode = true;
        SpawnAgents();
        Debug.Log($"[MultiAgentManager] Multi-agent mode enabled with {agentCount} agents");
    }
    
    /// <summary>
    /// Disable multi-agent mode and clean up agents
    /// </summary>
    public void DisableMultiAgentMode()
    {
        if (!isMultiAgentMode)
        {
            return;
        }
        
        isMultiAgentMode = false;
        ClearAllAgents();
        Debug.Log("[MultiAgentManager] Multi-agent mode disabled");
    }
    
    /// <summary>
    /// Spawn all agents according to current configuration
    /// </summary>
    public void SpawnAgents()
    {
        // Clear existing agents first
        ClearAllAgents();
        
        // Validate path points
        if (sharedState.pathPoints == null || sharedState.pathPoints.Length == 0)
        {
            Debug.LogError("[MultiAgentManager] No path points in SharedPathState!");
            return;
        }
        
        // Spawn based on mode
        switch (spawnMode)
        {
            case AgentSpawnMode.Simultaneous:
                SpawnAgentsSimultaneous();
                break;
                
            case AgentSpawnMode.Sequential:
                StartCoroutine(SpawnAgentsSequential());
                break;
                
            case AgentSpawnMode.Staggered:
                SpawnAgentsStaggered();
                break;
                
            case AgentSpawnMode.Competitive:
                SpawnAgentsCompetitive();
                break;
        }
        
        // Select first agent by default
        if (agents.Count > 0)
        {
            SelectAgent(0);
        }
        
        Debug.Log($"[MultiAgentManager] Spawned {agents.Count} agents in {spawnMode} mode");
    }
    
    void SpawnAgentsSimultaneous()
    {
        for (int i = 0; i < agentCount; i++)
        {
            PathAgent agent = CreateAgent(i);
            agent.startPositionPercent = 0f; // All start at the same position
            agent.speedMultiplier = 1f;
            agent.StartDrawing();
        }
    }
    
    System.Collections.IEnumerator SpawnAgentsSequential()
    {
        for (int i = 0; i < agentCount; i++)
        {
            PathAgent agent = CreateAgent(i);
            agent.startPositionPercent = 0f;
            agent.speedMultiplier = 1f;
            agent.StartDrawing();
            
            if (i < agentCount - 1) // Don't wait after last agent
            {
                yield return new WaitForSeconds(sequentialDelay);
            }
        }
    }
    
    void SpawnAgentsStaggered()
    {
        for (int i = 0; i < agentCount; i++)
        {
            PathAgent agent = CreateAgent(i);
            // Distribute agents evenly along the path
            agent.startPositionPercent = (float)i / agentCount;
            agent.speedMultiplier = 1f;
            
            // Set segment boundaries for visualization
            agent.segmentStart = (float)i / agentCount;
            agent.segmentEnd = (float)(i + 1) / agentCount;
            
            agent.StartDrawing();
        }
    }
    
    void SpawnAgentsCompetitive()
    {
        for (int i = 0; i < agentCount; i++)
        {
            PathAgent agent = CreateAgent(i);
            agent.startPositionPercent = 0f; // All start together
            
            // Add random speed variation for competition
            float variation = Random.Range(-competitiveSpeedVariation, competitiveSpeedVariation);
            agent.speedMultiplier = 1f + variation;
            
            agent.StartDrawing();
        }
    }
    
    PathAgent CreateAgent(int index)
    {
        GameObject agentObj;
        
        // Create agent GameObject
        if (agentPrefab != null)
        {
            agentObj = Instantiate(agentPrefab, transform);
        }
        else if (autoCreateAgents)
        {
            agentObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            agentObj.transform.SetParent(transform);
            agentObj.transform.localScale = Vector3.one * 0.4f;
            
            // Add PathAgent component
            PathAgent agentComponent = agentObj.AddComponent<PathAgent>();
        }
        else
        {
            Debug.LogError("[MultiAgentManager] No agent prefab and autoCreateAgents is false!");
            return null;
        }
        
        agentObj.name = $"Agent_{index}";
        
        // Get or add PathAgent component
        PathAgent agent = agentObj.GetComponent<PathAgent>();
        if (agent == null)
        {
            agent = agentObj.AddComponent<PathAgent>();
        }
        
        // Configure agent
        agent.agentIndex = index;
        agent.agentName = $"Agent {index}";
        agent.sharedState = sharedState;
        
        // Set color based on mode
        Color agentColor = sharedState.GetAgentColor(colorMode, index, agentCount);
        agent.agentColor = agentColor;
        
        // Apply color to renderer if exists
        Renderer renderer = agentObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = agentColor;
            mat.SetFloat("_Metallic", 0.6f);
            mat.SetFloat("_Glossiness", 0.8f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", agentColor * 1.5f);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            renderer.material = mat;
        }
        
        // Add to agents list
        agents.Add(agent);
        
        return agent;
    }
    
    /// <summary>
    /// Clear all spawned agents
    /// </summary>
    public void ClearAllAgents()
    {
        foreach (PathAgent agent in agents)
        {
            if (agent != null)
            {
                Destroy(agent.gameObject);
            }
        }
        
        agents.Clear();
        selectedAgent = null;
        ResetGlobalStats();
        
        Debug.Log("[MultiAgentManager] All agents cleared");
    }
    
    /// <summary>
    /// Select an agent by index (for camera follow and UI focus)
    /// </summary>
    public void SelectAgent(int index)
    {
        if (index < 0 || index >= agents.Count)
        {
            Debug.LogWarning($"[MultiAgentManager] Invalid agent index: {index}");
            return;
        }
        
        selectedAgent = agents[index];
        OnAgentSelected?.Invoke(selectedAgent);
        
        Debug.Log($"[MultiAgentManager] Selected Agent {index}");
    }
    
    /// <summary>
    /// Pause a specific agent
    /// </summary>
    public void PauseAgent(int index)
    {
        if (index >= 0 && index < agents.Count)
        {
            agents[index].Pause();
        }
    }
    
    /// <summary>
    /// Resume a specific agent
    /// </summary>
    public void ResumeAgent(int index)
    {
        if (index >= 0 && index < agents.Count)
        {
            agents[index].Resume();
        }
    }
    
    /// <summary>
    /// Pause all agents
    /// </summary>
    public void PauseAllAgents()
    {
        foreach (PathAgent agent in agents)
        {
            agent.Pause();
        }
    }
    
    /// <summary>
    /// Resume all agents
    /// </summary>
    public void ResumeAllAgents()
    {
        foreach (PathAgent agent in agents)
        {
            agent.Resume();
        }
    }
    
    /// <summary>
    /// Reset all agents to starting positions
    /// </summary>
    public void ResetAllAgents()
    {
        foreach (PathAgent agent in agents)
        {
            agent.ResetAgent();
        }
        
        ResetGlobalStats();
        Debug.Log("[MultiAgentManager] All agents reset");
    }
    
    /// <summary>
    /// Update global statistics
    /// </summary>
    void UpdateGlobalStats()
    {
        if (agents.Count == 0) return;
        
        activeAgentCount = 0;
        pausedAgentCount = 0;
        completedAgentCount = 0;
        totalDistanceCovered = 0f;
        float totalProgress = 0f;
        
        foreach (PathAgent agent in agents)
        {
            if (agent == null) continue;
            
            switch (agent.status)
            {
                case PathAgent.AgentStatus.Active:
                    activeAgentCount++;
                    break;
                case PathAgent.AgentStatus.Paused:
                    pausedAgentCount++;
                    break;
                case PathAgent.AgentStatus.Completed:
                    completedAgentCount++;
                    break;
            }
            
            totalDistanceCovered += agent.totalDistanceTraveled;
            
            // Calculate progress (cycle + position within cycle)
            float cycleProgress = agent.currentCycle;
            float positionProgress = agent.currentDistance / Mathf.Max(1f, agent.totalPathLength);
            totalProgress += cycleProgress + positionProgress;
        }
        
        averageProgress = totalProgress / agents.Count;
        
        // Update shared state with agent count
        if (sharedState != null)
        {
            sharedState.activeAgentCount = activeAgentCount;
        }
    }
    
    void ResetGlobalStats()
    {
        activeAgentCount = 0;
        pausedAgentCount = 0;
        completedAgentCount = 0;
        averageProgress = 0f;
        totalDistanceCovered = 0f;
    }
    
    /// <summary>
    /// Update agent count (will respawn agents)
    /// </summary>
    public void SetAgentCount(int count)
    {
        count = Mathf.Clamp(count, 1, 16);
        if (count != agentCount)
        {
            agentCount = count;
            if (isMultiAgentMode)
            {
                SpawnAgents(); // Respawn with new count
            }
        }
    }
    
    /// <summary>
    /// Update color mode (will recolor existing agents)
    /// </summary>
    public void SetColorMode(AgentColorMode mode)
    {
        colorMode = mode;
        
        // Recolor existing agents
        for (int i = 0; i < agents.Count; i++)
        {
            if (agents[i] != null)
            {
                Color newColor = sharedState.GetAgentColor(colorMode, i, agents.Count);
                agents[i].SetColor(newColor);
            }
        }
    }
    
    /// <summary>
    /// Update spawn mode (will respawn agents)
    /// </summary>
    public void SetSpawnMode(AgentSpawnMode mode)
    {
        if (mode != spawnMode)
        {
            spawnMode = mode;
            if (isMultiAgentMode)
            {
                SpawnAgents(); // Respawn with new mode
            }
        }
    }
    
    /// <summary>
    /// Get agent by index
    /// </summary>
    public PathAgent GetAgent(int index)
    {
        if (index >= 0 && index < agents.Count)
        {
            return agents[index];
        }
        return null;
    }
    
    /// <summary>
    /// Get list of all agents (for UI display)
    /// </summary>
    public List<PathAgent> GetAllAgents()
    {
        return new List<PathAgent>(agents);
    }
}
