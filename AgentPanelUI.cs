using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Bottom-right panel UI for managing and monitoring multiple agents.
/// Shows agent roster, stats, and provides controls for individual agents.
/// </summary>
public class AgentPanelUI : MonoBehaviour
{
    [Header("Panel Configuration")]
    [Tooltip("Width of the panel")]
    public float panelWidth = 380f;
    
    [Tooltip("Height of the panel")]
    public float panelHeight = 600f;
    
    [Header("References")]
    public MultiAgentManager agentManager;
    public GameObject agentPanel;
    public GameObject agentListContent;
    public ScrollRect agentListScrollRect;
    
    [Header("Global Stats Display")]
    public Text globalStatsText;
    
    [Header("Agent Cards")]
    private List<AgentCard> agentCards = new List<AgentCard>();
    
    // Agent card prefab (created at runtime)
    private GameObject agentCardPrefab;
    
    // Update frequency for stats (10 Hz = every 0.1s)
    private float updateInterval = 0.1f;
    private float timeSinceLastUpdate = 0f;
    
    /// <summary>
    /// Represents a single agent card in the roster
    /// </summary>
    private class AgentCard
    {
        public GameObject cardObject;
        public PathAgent agent;
        public Toggle selectToggle;
        public Button focusButton;
        public Button pauseResumeButton;
        public Text statusText;
        public Text progressText;
        public Text speedText;
        public Slider progressBar;
        public Image statusIcon;
        public Image cardBackground;
        
        // Status colors
        public static Color idleColor = new Color(0.3f, 0.3f, 0.4f, 0.8f);
        public static Color activeColor = new Color(0.1f, 0.3f, 0.2f, 0.8f);
        public static Color pausedColor = new Color(0.3f, 0.2f, 0.1f, 0.8f);
        public static Color completedColor = new Color(0.2f, 0.25f, 0.3f, 0.8f);
    }
    
    void Start()
    {
        // Find agent manager if not assigned
        if (agentManager == null)
        {
            agentManager = FindFirstObjectByType<MultiAgentManager>();
        }
        
        // Subscribe to agent manager events
        if (agentManager != null)
        {
            agentManager.OnAgentSelected += OnAgentSelected;
        }
        
        // Hide panel initially (shown when multi-agent mode enabled)
        if (agentPanel != null)
        {
            agentPanel.SetActive(false);
        }
    }
    
    void Update()
    {
        // Update stats at 10 Hz instead of 60 Hz for performance
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= updateInterval)
        {
            timeSinceLastUpdate = 0f;
            UpdateAllAgentCards();
            UpdateGlobalStats();
        }
    }
    
    /// <summary>
    /// Show the agent panel
    /// </summary>
    public void ShowPanel()
    {
        if (agentPanel != null)
        {
            agentPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Hide the agent panel
    /// </summary>
    public void HidePanel()
    {
        if (agentPanel != null)
        {
            agentPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Create the agent panel UI
    /// </summary>
    public void CreatePanel(Canvas canvas)
    {
        // Create panel container
        GameObject panelObj = new GameObject("AgentPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.pivot = new Vector2(1, 0);
        panelRect.anchoredPosition = new Vector2(-15, 15);
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
        
        // Panel background
        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = new Color(0.02f, 0.02f, 0.08f, 0.85f);
        
        // Add outline
        Outline panelOutline = panelObj.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0.3f, 0.5f, 0.9f, 0.3f);
        panelOutline.effectDistance = new Vector2(2, -2);
        
        // Add glow
        Shadow panelGlow = panelObj.AddComponent<Shadow>();
        panelGlow.effectColor = new Color(0.2f, 0.4f, 0.8f, 0.25f);
        panelGlow.effectDistance = new Vector2(0, 0);
        
        agentPanel = panelObj;
        
        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -10);
        titleRect.sizeDelta = new Vector2(-20, 35);
        
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "◉ AGENT ROSTER";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 16;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = new Color(0.7f, 0.85f, 1f, 0.95f);
        titleText.alignment = TextAnchor.MiddleCenter;
        
        Shadow titleShadow = titleObj.AddComponent<Shadow>();
        titleShadow.effectColor = new Color(0.3f, 0.6f, 1f, 0.5f);
        titleShadow.effectDistance = new Vector2(0, 0);
        
        // Global Stats Section
        GameObject statsObj = new GameObject("GlobalStats");
        statsObj.transform.SetParent(panelObj.transform, false);
        RectTransform statsRect = statsObj.AddComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0, 1);
        statsRect.anchorMax = new Vector2(1, 1);
        statsRect.pivot = new Vector2(0.5f, 1);
        statsRect.anchoredPosition = new Vector2(0, -50);
        statsRect.sizeDelta = new Vector2(-20, 60);
        
        Image statsBg = statsObj.AddComponent<Image>();
        statsBg.color = new Color(0.05f, 0.05f, 0.15f, 0.6f);
        
        globalStatsText = statsObj.AddComponent<Text>();
        globalStatsText.text = "Active: 0 | Paused: 0 | Completed: 0\nAvg Progress: 0%\nTotal Distance: 0m";
        globalStatsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        globalStatsText.fontSize = 10;
        globalStatsText.color = new Color(0.6f, 0.75f, 0.9f, 0.9f);
        globalStatsText.alignment = TextAnchor.UpperLeft;
        
        RectTransform globalStatsTextRect = globalStatsText.GetComponent<RectTransform>();
        globalStatsTextRect.anchorMin = Vector2.zero;
        globalStatsTextRect.anchorMax = Vector2.one;
        globalStatsTextRect.offsetMin = new Vector2(10, 5);
        globalStatsTextRect.offsetMax = new Vector2(-10, -5);
        
        // Agent List Section (Scrollable)
        GameObject listObj = new GameObject("AgentList");
        listObj.transform.SetParent(panelObj.transform, false);
        RectTransform listRect = listObj.AddComponent<RectTransform>();
        listRect.anchorMin = new Vector2(0, 0);
        listRect.anchorMax = new Vector2(1, 1);
        listRect.pivot = new Vector2(0.5f, 1);
        listRect.anchoredPosition = new Vector2(0, -120);
        listRect.sizeDelta = new Vector2(-20, -130);
        
        Image listBg = listObj.AddComponent<Image>();
        listBg.color = new Color(0.03f, 0.03f, 0.1f, 0.7f);
        
        // Add ScrollRect
        agentListScrollRect = listObj.AddComponent<ScrollRect>();
        agentListScrollRect.horizontal = false;
        agentListScrollRect.vertical = true;
        agentListScrollRect.scrollSensitivity = 15f;
        agentListScrollRect.movementType = ScrollRect.MovementType.Clamped;
        agentListScrollRect.inertia = true;
        
        // Viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(listObj.transform, false);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(5, 5);
        viewportRect.offsetMax = new Vector2(-5, -5);
        
        Image viewportImage = viewportObj.AddComponent<Image>();
        viewportImage.color = Color.white;
        Mask viewportMask = viewportObj.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        
        agentListScrollRect.viewport = viewportRect;
        
        // Content (holds agent cards)
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 400);
        
        VerticalLayoutGroup contentLayout = contentObj.AddComponent<VerticalLayoutGroup>();
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = false;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.spacing = 5f;
        contentLayout.padding = new RectOffset(5, 5, 5, 5);
        
        ContentSizeFitter contentFitter = contentObj.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        agentListScrollRect.content = contentRect;
        agentListContent = contentObj;
        
        // Initially hidden
        panelObj.SetActive(false);
        
        Debug.Log("✓ Agent Panel UI created");
    }
    
    /// <summary>
    /// Populate the agent list with cards for all agents
    /// </summary>
    public void PopulateAgentList()
    {
        if (agentManager == null || agentListContent == null) return;
        
        // Clear existing cards
        ClearAgentList();
        
        // Create card for each agent
        List<PathAgent> agents = agentManager.GetAllAgents();
        for (int i = 0; i < agents.Count; i++)
        {
            if (agents[i] != null)
            {
                CreateAgentCard(agents[i], i);
            }
        }
        
        Debug.Log($"✓ Created {agentCards.Count} agent cards");
    }
    
    /// <summary>
    /// Clear all agent cards
    /// </summary>
    void ClearAgentList()
    {
        foreach (AgentCard card in agentCards)
        {
            if (card.cardObject != null)
            {
                Destroy(card.cardObject);
            }
        }
        agentCards.Clear();
    }
    
    /// <summary>
    /// Create a card for a single agent
    /// </summary>
    void CreateAgentCard(PathAgent agent, int index)
    {
        // Card container
        GameObject cardObj = new GameObject($"AgentCard_{index}");
        cardObj.transform.SetParent(agentListContent.transform, false);
        RectTransform cardRect = cardObj.AddComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(0, 90);
        
        LayoutElement cardLayout = cardObj.AddComponent<LayoutElement>();
        cardLayout.preferredHeight = 90;
        cardLayout.flexibleHeight = 0;
        
        // Card background
        Image cardBg = cardObj.AddComponent<Image>();
        cardBg.color = AgentCard.idleColor;
        
        Outline cardOutline = cardObj.AddComponent<Outline>();
        cardOutline.effectColor = new Color(0.3f, 0.5f, 0.8f, 0.3f);
        cardOutline.effectDistance = new Vector2(1, -1);
        
        // Agent name and status (top row)
        GameObject topRowObj = new GameObject("TopRow");
        topRowObj.transform.SetParent(cardObj.transform, false);
        RectTransform topRowRect = topRowObj.AddComponent<RectTransform>();
        topRowRect.anchorMin = new Vector2(0, 1);
        topRowRect.anchorMax = new Vector2(1, 1);
        topRowRect.pivot = new Vector2(0, 1);
        topRowRect.anchoredPosition = new Vector2(8, -5);
        topRowRect.sizeDelta = new Vector2(-16, 20);
        
        Text nameText = topRowObj.AddComponent<Text>();
        nameText.text = $"Agent {index}";
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameText.fontSize = 12;
        nameText.fontStyle = FontStyle.Bold;
        nameText.color = agent.agentColor;
        nameText.alignment = TextAnchor.MiddleLeft;
        
        // Status icon and text (next to name)
        GameObject statusObj = new GameObject("Status");
        statusObj.transform.SetParent(topRowObj.transform, false);
        RectTransform statusRect = statusObj.AddComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(1, 0);
        statusRect.anchorMax = new Vector2(1, 1);
        statusRect.pivot = new Vector2(1, 0.5f);
        statusRect.anchoredPosition = Vector2.zero;
        statusRect.sizeDelta = new Vector2(80, 0);
        
        Text statusText = statusObj.AddComponent<Text>();
        statusText.text = "● Idle";
        statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.fontSize = 10;
        statusText.color = new Color(0.6f, 0.6f, 0.7f, 1f);
        statusText.alignment = TextAnchor.MiddleRight;
        
        // Progress bar
        GameObject progressBarObj = new GameObject("ProgressBar");
        progressBarObj.transform.SetParent(cardObj.transform, false);
        RectTransform progressBarRect = progressBarObj.AddComponent<RectTransform>();
        progressBarRect.anchorMin = new Vector2(0, 1);
        progressBarRect.anchorMax = new Vector2(1, 1);
        progressBarRect.pivot = new Vector2(0, 1);
        progressBarRect.anchoredPosition = new Vector2(8, -30);
        progressBarRect.sizeDelta = new Vector2(-16, 8);
        
        Image progressBg = progressBarObj.AddComponent<Image>();
        progressBg.color = new Color(0.1f, 0.1f, 0.2f, 0.8f);
        
        GameObject fillAreaObj = new GameObject("FillArea");
        fillAreaObj.transform.SetParent(progressBarObj.transform, false);
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;
        
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = agent.agentColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        
        Slider progressBar = progressBarObj.AddComponent<Slider>();
        progressBar.fillRect = fillRect;
        progressBar.interactable = false;
        progressBar.minValue = 0f;
        progressBar.maxValue = 1f;
        progressBar.value = 0f;
        
        // Stats text (progress and speed)
        GameObject statsTextObj = new GameObject("StatsText");
        statsTextObj.transform.SetParent(cardObj.transform, false);
        RectTransform statsTextRect = statsTextObj.AddComponent<RectTransform>();
        statsTextRect.anchorMin = new Vector2(0, 1);
        statsTextRect.anchorMax = new Vector2(1, 1);
        statsTextRect.pivot = new Vector2(0, 1);
        statsTextRect.anchoredPosition = new Vector2(8, -42);
        statsTextRect.sizeDelta = new Vector2(-16, 15);
        
        Text progressText = statsTextObj.AddComponent<Text>();
        progressText.text = "Progress: 0% | Speed: 0.0";
        progressText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        progressText.fontSize = 9;
        progressText.color = new Color(0.5f, 0.65f, 0.8f, 0.9f);
        progressText.alignment = TextAnchor.MiddleLeft;
        
        // Buttons (bottom row)
        // Focus button
        GameObject focusButtonObj = new GameObject("FocusButton");
        focusButtonObj.transform.SetParent(cardObj.transform, false);
        RectTransform focusButtonRect = focusButtonObj.AddComponent<RectTransform>();
        focusButtonRect.anchorMin = new Vector2(0, 0);
        focusButtonRect.anchorMax = new Vector2(0, 0);
        focusButtonRect.pivot = new Vector2(0, 0);
        focusButtonRect.anchoredPosition = new Vector2(8, 5);
        focusButtonRect.sizeDelta = new Vector2(50, 25);
        
        Image focusButtonImage = focusButtonObj.AddComponent<Image>();
        focusButtonImage.color = new Color(0.1f, 0.2f, 0.3f, 0.8f);
        
        Button focusButton = focusButtonObj.AddComponent<Button>();
        focusButton.targetGraphic = focusButtonImage;
        
        GameObject focusTextObj = new GameObject("Text");
        focusTextObj.transform.SetParent(focusButtonObj.transform, false);
        RectTransform focusTextRect = focusTextObj.AddComponent<RectTransform>();
        focusTextRect.anchorMin = Vector2.zero;
        focusTextRect.anchorMax = Vector2.one;
        focusTextRect.sizeDelta = Vector2.zero;
        
        Text focusText = focusTextObj.AddComponent<Text>();
        focusText.text = "🔍";
        focusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        focusText.fontSize = 14;
        focusText.color = new Color(0.7f, 0.85f, 1f, 0.9f);
        focusText.alignment = TextAnchor.MiddleCenter;
        
        // Pause/Resume button
        GameObject pauseButtonObj = new GameObject("PauseButton");
        pauseButtonObj.transform.SetParent(cardObj.transform, false);
        RectTransform pauseButtonRect = pauseButtonObj.AddComponent<RectTransform>();
        pauseButtonRect.anchorMin = new Vector2(0, 0);
        pauseButtonRect.anchorMax = new Vector2(0, 0);
        pauseButtonRect.pivot = new Vector2(0, 0);
        pauseButtonRect.anchoredPosition = new Vector2(65, 5);
        pauseButtonRect.sizeDelta = new Vector2(70, 25);
        
        Image pauseButtonImage = pauseButtonObj.AddComponent<Image>();
        pauseButtonImage.color = new Color(0.15f, 0.2f, 0.1f, 0.8f);
        
        Button pauseButton = pauseButtonObj.AddComponent<Button>();
        pauseButton.targetGraphic = pauseButtonImage;
        
        GameObject pauseTextObj = new GameObject("Text");
        pauseTextObj.transform.SetParent(pauseButtonObj.transform, false);
        RectTransform pauseTextRect = pauseTextObj.AddComponent<RectTransform>();
        pauseTextRect.anchorMin = Vector2.zero;
        pauseTextRect.anchorMax = Vector2.one;
        pauseTextRect.sizeDelta = Vector2.zero;
        
        Text pauseText = pauseTextObj.AddComponent<Text>();
        pauseText.text = "⏸ Pause";
        pauseText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        pauseText.fontSize = 9;
        pauseText.fontStyle = FontStyle.Bold;
        pauseText.color = new Color(0.7f, 0.85f, 1f, 0.9f);
        pauseText.alignment = TextAnchor.MiddleCenter;
        
        // Select toggle (radio button)
        GameObject selectToggleObj = new GameObject("SelectToggle");
        selectToggleObj.transform.SetParent(cardObj.transform, false);
        RectTransform selectToggleRect = selectToggleObj.AddComponent<RectTransform>();
        selectToggleRect.anchorMin = new Vector2(1, 0.5f);
        selectToggleRect.anchorMax = new Vector2(1, 0.5f);
        selectToggleRect.pivot = new Vector2(1, 0.5f);
        selectToggleRect.anchoredPosition = new Vector2(-8, 0);
        selectToggleRect.sizeDelta = new Vector2(20, 20);
        
        Image selectBg = selectToggleObj.AddComponent<Image>();
        selectBg.color = new Color(0.1f, 0.15f, 0.25f, 0.8f);
        
        GameObject checkmarkObj = new GameObject("Checkmark");
        checkmarkObj.transform.SetParent(selectToggleObj.transform, false);
        RectTransform checkmarkRect = checkmarkObj.AddComponent<RectTransform>();
        checkmarkRect.anchorMin = Vector2.zero;
        checkmarkRect.anchorMax = Vector2.one;
        checkmarkRect.sizeDelta = Vector2.zero;
        
        Text checkmark = checkmarkObj.AddComponent<Text>();
        checkmark.text = "●";
        checkmark.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        checkmark.fontSize = 16;
        checkmark.color = agent.agentColor;
        checkmark.alignment = TextAnchor.MiddleCenter;
        
        Toggle selectToggle = selectToggleObj.AddComponent<Toggle>();
        selectToggle.targetGraphic = selectBg;
        selectToggle.graphic = checkmark;
        selectToggle.isOn = false;
        
        // Create AgentCard data structure
        AgentCard card = new AgentCard();
        card.cardObject = cardObj;
        card.agent = agent;
        card.selectToggle = selectToggle;
        card.focusButton = focusButton;
        card.pauseResumeButton = pauseButton;
        card.statusText = statusText;
        card.progressText = progressText;
        card.speedText = null; // Not used separately
        card.progressBar = progressBar;
        card.statusIcon = null; // Not used separately
        card.cardBackground = cardBg;
        
        // Connect buttons
        int agentIndex = index; // Capture for closure
        focusButton.onClick.AddListener(() => OnFocusButtonClicked(agentIndex));
        pauseButton.onClick.AddListener(() => OnPauseResumeButtonClicked(agentIndex, pauseText));
        selectToggle.onValueChanged.AddListener((isOn) => {
            if (isOn) OnAgentCardSelected(agentIndex);
        });
        
        agentCards.Add(card);
    }
    
    /// <summary>
    /// Update all agent cards (called at 10 Hz)
    /// </summary>
    void UpdateAllAgentCards()
    {
        if (agentManager == null || !agentManager.isMultiAgentMode) return;
        
        for (int i = 0; i < agentCards.Count; i++)
        {
            if (agentCards[i].agent != null)
            {
                UpdateAgentCard(agentCards[i]);
            }
        }
    }
    
    /// <summary>
    /// Update a single agent card's display
    /// </summary>
    void UpdateAgentCard(AgentCard card)
    {
        PathAgent agent = card.agent;
        
        // Update status text and card color
        switch (agent.status)
        {
            case PathAgent.AgentStatus.Idle:
                card.statusText.text = "● Idle";
                card.statusText.color = new Color(0.5f, 0.5f, 0.6f, 1f);
                card.cardBackground.color = AgentCard.idleColor;
                break;
            case PathAgent.AgentStatus.Active:
                card.statusText.text = "▶ Active";
                card.statusText.color = new Color(0.3f, 0.8f, 0.4f, 1f);
                card.cardBackground.color = AgentCard.activeColor;
                break;
            case PathAgent.AgentStatus.Paused:
                card.statusText.text = "⏸ Paused";
                card.statusText.color = new Color(0.9f, 0.7f, 0.3f, 1f);
                card.cardBackground.color = AgentCard.pausedColor;
                break;
            case PathAgent.AgentStatus.Completed:
                card.statusText.text = "✓ Complete";
                card.statusText.color = new Color(0.4f, 0.6f, 0.9f, 1f);
                card.cardBackground.color = AgentCard.completedColor;
                break;
        }
        
        // Update progress bar and text
        float cycleProgress = agent.currentCycle;
        float positionProgress = 0f;
        if (agentManager.sharedState != null && agent.totalPathLength > 0)
        {
            positionProgress = agent.currentDistance / agent.totalPathLength;
        }
        float totalProgress = (cycleProgress + positionProgress) / Mathf.Max(1, agentManager.sharedState.masterCycles);
        
        card.progressBar.value = totalProgress;
        
        // Update stats text
        float speedMultiplier = agent.speedMultiplier;
        card.progressText.text = $"Progress: {(totalProgress * 100f):F1}% | Speed: {speedMultiplier:F2}x";
    }
    
    /// <summary>
    /// Update global statistics display
    /// </summary>
    void UpdateGlobalStats()
    {
        if (agentManager == null || globalStatsText == null) return;
        
        globalStatsText.text = $"Active: {agentManager.activeAgentCount} | Paused: {agentManager.pausedAgentCount} | Completed: {agentManager.completedAgentCount}\n" +
                               $"Avg Progress: {(agentManager.averageProgress * 100f):F1}%\n" +
                               $"Total Distance: {agentManager.totalDistanceCovered:F1}m";
    }
    
    /// <summary>
    /// Handle focus button click (camera jumps to agent)
    /// </summary>
    void OnFocusButtonClicked(int agentIndex)
    {
        if (agentManager == null) return;
        
        PathAgent agent = agentManager.GetAgent(agentIndex);
        if (agent != null)
        {
            // Focus camera on agent
            CameraController cameraController = FindFirstObjectByType<CameraController>();
            if (cameraController != null)
            {
                cameraController.target = agent.transform;
                cameraController.SetCameraMode(CameraController.CameraMode.SmoothFollow);
                Debug.Log($"Camera focused on Agent {agentIndex}");
            }
        }
    }
    
    /// <summary>
    /// Handle pause/resume button click
    /// </summary>
    void OnPauseResumeButtonClicked(int agentIndex, Text buttonText)
    {
        if (agentManager == null) return;
        
        PathAgent agent = agentManager.GetAgent(agentIndex);
        if (agent != null)
        {
            if (agent.status == PathAgent.AgentStatus.Active)
            {
                agentManager.PauseAgent(agentIndex);
                buttonText.text = "▶ Resume";
            }
            else if (agent.status == PathAgent.AgentStatus.Paused)
            {
                agentManager.ResumeAgent(agentIndex);
                buttonText.text = "⏸ Pause";
            }
        }
    }
    
    /// <summary>
    /// Handle agent card selection (radio button)
    /// </summary>
    void OnAgentCardSelected(int agentIndex)
    {
        if (agentManager == null) return;
        
        // Deselect all other toggles (radio button behavior)
        for (int i = 0; i < agentCards.Count; i++)
        {
            if (i != agentIndex && agentCards[i].selectToggle != null)
            {
                agentCards[i].selectToggle.SetIsOnWithoutNotify(false);
            }
        }
        
        // Select the agent in the manager
        agentManager.SelectAgent(agentIndex);
        
        // Focus camera on selected agent
        OnFocusButtonClicked(agentIndex);
    }
    
    /// <summary>
    /// Handle agent selection from manager (update UI)
    /// </summary>
    void OnAgentSelected(PathAgent agent)
    {
        if (agent == null) return;
        
        // Update toggles to reflect selection
        for (int i = 0; i < agentCards.Count; i++)
        {
            if (agentCards[i].agent == agent)
            {
                agentCards[i].selectToggle.SetIsOnWithoutNotify(true);
            }
            else
            {
                agentCards[i].selectToggle.SetIsOnWithoutNotify(false);
            }
        }
    }
}
