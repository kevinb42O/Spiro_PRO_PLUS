using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI Generator for Spirograph controls with modern 2025+ glassmorphism space theme
/// USAGE:
/// 1. Add this component to any GameObject in your scene
/// 2. In the Inspector, check the "generateUI" checkbox
/// 3. The UI will be created automatically with smooth animations and cosmic aesthetics
/// 4. Hook up the sliders/buttons to SpirographRoller, RotateParent, and CameraController manually
/// </summary>
public class SpirographUIManager : MonoBehaviour
{
    [Header("Active Rotor Tracking")]
    [Tooltip("The currently active rotor that UI controls")]
    public SpirographRoller activeRoller = null;
    
    [Header("UI Elements - Auto-populated after generation")]
    public Slider speedSlider;
    public Text speedText;
    public Slider cyclesSlider;
    public Slider rotationSpeedSlider;
    public Slider objectRotationSpeedSlider;
    public Slider penDistanceSlider;
    public Slider lineWidthSlider;
    public Slider lineBrightnessSlider;
    public Button pauseButton;
    public Button resetButton;
    public Button lookAtButton;
    public Button smoothFollowButton;
    public Button autoOrbitButton;
    public Button toggleVisualsButton;
    public Button lineEffectsButton;
    public Button hideUIButton;
    
    [Header("Color Picker Elements")]
    public Slider hueSlider;
    public Slider saturationSlider;
    public Slider valueSlider;
    public GameObject colorPreview;
    public Button[] colorPresetButtons;
    
    [Header("Section Toggles")]
    public Button motionSectionToggle;
    public Button visualsSectionToggle;
    public Button colorSectionToggle;
    public Button environmentSectionToggle;
    public Button cameraSectionToggle;
    
    [Header("Section GameObjects")]
    public GameObject motionSection;
    public GameObject visualsSection;
    public GameObject colorSection;
    public GameObject environmentSection;
    public GameObject cameraSection;
    
    [Header("Skybox Dropdown")]
    public Dropdown skyboxDropdown;
    
    [Header("Pattern Generator")]
    public Dropdown patternDropdown;
    public Button generatePatternButton;
    public Button clearPatternsButton;
    
    [Header("Multi-Agent System")]
    public Toggle multiAgentToggle;
    public Slider agentCountSlider;
    public Text agentCountText;
    public Dropdown agentColorModeDropdown;
    public Dropdown agentSpawnModeDropdown;
    public MultiAgentManager multiAgentManager;
    public SharedPathState sharedPathState;
    
    [Header("UI State")]
    private GameObject controlPanel;
    private bool isUIVisible = true;
    
    [Header("Generate UI")]
    [Tooltip("Check this box to generate UI (will auto-uncheck after generation)")]
    public bool generateUI = false;
    
    void OnValidate()
    {
        if (generateUI)
        {
            generateUI = false;
            #if UNITY_EDITOR
            // Delay execution to next editor update to ensure proper serialization
            UnityEditor.EditorApplication.delayCall += () => {
                if (this != null)
                {
                    GenerateCompleteUI();
                }
            };
            #endif
        }
    }
    
    void Start()
    {
        // Find the control panel if it exists
        if (controlPanel == null)
        {
            GameObject canvas = GameObject.Find("SpirographCanvas");
            if (canvas != null)
            {
                controlPanel = canvas.transform.Find("ControlPanel")?.gameObject;
            }
        }
        
        // Find and setup the hide button
        if (hideUIButton == null)
        {
            GameObject canvas = GameObject.Find("SpirographCanvas");
            if (canvas != null)
            {
                Transform buttonTransform = canvas.transform.Find("HideUIButton");
                if (buttonTransform != null)
                {
                    hideUIButton = buttonTransform.GetComponent<Button>();
                }
            }
        }
        
        if (hideUIButton != null)
        {
            hideUIButton.onClick.AddListener(ToggleUI);
        }
        
        // Reconnect all UI elements at runtime (listeners from Edit mode don't persist)
        ReconnectUIElements();
        
        // Connect Pattern Generator
        ConnectPatternGenerator();
        
        // Connect Multi-Agent System
        ConnectMultiAgentSystem();
        
        // Subscribe to PatternSpawner rotor change events
        SubscribeToPatternSpawner();
        
        // Set initial active rotor
        if (activeRoller == null)
        {
            activeRoller = FindFirstObjectByType<SpirographRoller>();
            if (activeRoller != null)
            {
                Debug.Log($"✓ Initial active rotor set: {activeRoller.gameObject.name}");
            }
        }
    }
    
    /// <summary>
    /// Subscribe to PatternSpawner events to track active rotor changes
    /// </summary>
    void SubscribeToPatternSpawner()
    {
        PatternSpawner spawner = FindFirstObjectByType<PatternSpawner>();
        if (spawner != null)
        {
            spawner.OnActiveRotorChanged += OnActiveRotorChanged;
            Debug.Log("✓ UI Manager subscribed to PatternSpawner rotor change events");
        }
    }
    
    /// <summary>
    /// Called when the active rotor changes (new pattern spawned)
    /// </summary>
    void OnActiveRotorChanged(SpirographRoller newActiveRotor, SpirographRoller oldRotor)
    {
        Debug.Log($"[UI Manager] Active rotor changed! Old: {oldRotor?.gameObject.name ?? "None"}, New: {newActiveRotor?.gameObject.name ?? "None"}");
        
        // Update active rotor reference
        activeRoller = newActiveRotor;
        
        // Reconnect UI sliders to new active rotor
        if (activeRoller != null)
        {
            ConnectSlidersToActiveRotor();
            Debug.Log($"✓ UI now controlling new rotor: {activeRoller.gameObject.name}");
        }
    }
    
    /// <summary>
    /// Connect all UI sliders to the currently active rotor
    /// </summary>
    void ConnectSlidersToActiveRotor()
    {
        if (activeRoller == null) return;
        
        // Speed Slider
        if (speedSlider != null)
        {
            speedSlider.onValueChanged.RemoveAllListeners();
            speedSlider.onValueChanged.AddListener((value) => {
                if (activeRoller != null)
                {
                    activeRoller.speed = value;
                    if (speedText != null)
                    {
                        speedText.text = "Travel Speed: " + value.ToString("F1");
                    }
                    Debug.Log($"Speed slider changed: {value} -> activeRoller.speed is now {activeRoller.speed}");
                }
            });
            speedSlider.value = activeRoller.speed; // Set value AFTER adding listener
            if (speedText != null)
            {
                speedText.text = "Travel Speed: " + activeRoller.speed.ToString("F1");
            }
            Debug.Log($"✓ Speed slider connected to active rotor (current speed: {activeRoller.speed})");
        }
        
        // Rotation Speed Slider
        if (rotationSpeedSlider != null)
        {
            rotationSpeedSlider.onValueChanged.RemoveAllListeners();
            rotationSpeedSlider.value = activeRoller.rotationSpeed;
            rotationSpeedSlider.onValueChanged.AddListener((value) => {
                if (activeRoller != null) activeRoller.rotationSpeed = value;
            });
        }
        
        // Pen Distance Slider
        if (penDistanceSlider != null)
        {
            penDistanceSlider.onValueChanged.RemoveAllListeners();
            penDistanceSlider.value = activeRoller.penDistance;
            penDistanceSlider.onValueChanged.AddListener((value) => {
                if (activeRoller != null) activeRoller.penDistance = value;
            });
        }
        
        // Cycles Slider
        if (cyclesSlider != null)
        {
            cyclesSlider.onValueChanged.RemoveAllListeners();
            cyclesSlider.value = activeRoller.cycles;
            cyclesSlider.onValueChanged.AddListener((value) => {
                if (activeRoller != null) activeRoller.cycles = (int)value;
            });
        }
        
        Debug.Log("✓ All UI sliders connected to active rotor");
    }
    
    void ReconnectUIElements()
    {
        // Reconnect section toggles
        if (motionSectionToggle != null && motionSection != null)
            SetupSectionToggle(motionSectionToggle, motionSection);
        if (visualsSectionToggle != null && visualsSection != null)
            SetupSectionToggle(visualsSectionToggle, visualsSection);
        if (colorSectionToggle != null && colorSection != null)
            SetupSectionToggle(colorSectionToggle, colorSection);
        if (environmentSectionToggle != null && environmentSection != null)
            SetupSectionToggle(environmentSectionToggle, environmentSection);
        if (cameraSectionToggle != null && cameraSection != null)
            SetupSectionToggle(cameraSectionToggle, cameraSection);
        
        // Reconnect camera buttons
        CameraController cameraController = FindFirstObjectByType<CameraController>();
        if (cameraController != null)
        {
            if (lookAtButton != null)
            {
                lookAtButton.onClick.RemoveAllListeners();
                lookAtButton.onClick.AddListener(() => {
                    cameraController.SetCameraMode(CameraController.CameraMode.FreeFly);
                    Debug.Log("Camera Mode: Free Fly");
                });
            }
            
            if (smoothFollowButton != null)
            {
                smoothFollowButton.onClick.RemoveAllListeners();
                smoothFollowButton.onClick.AddListener(() => {
                    cameraController.SetCameraMode(CameraController.CameraMode.SmoothFollow);
                    Debug.Log("Camera Mode: Smooth Follow");
                });
            }
            
            if (autoOrbitButton != null)
            {
                autoOrbitButton.onClick.RemoveAllListeners();
                autoOrbitButton.onClick.AddListener(() => {
                    cameraController.SetCameraMode(CameraController.CameraMode.AutoOrbit);
                    Debug.Log("Camera Mode: Auto Orbit");
                });
            }
        }
        
        // Reconnect HSV sliders
        if (hueSlider != null && saturationSlider != null && valueSlider != null && colorPreview != null)
        {
            hueSlider.onValueChanged.RemoveAllListeners();
            saturationSlider.onValueChanged.RemoveAllListeners();
            valueSlider.onValueChanged.RemoveAllListeners();
            
            System.Action updateColorAndPreview = () => {
                float h = hueSlider.value;
                float s = saturationSlider.value;
                float v = valueSlider.value;
                Color newColor = Color.HSVToRGB(h, s, v);
                
                // Update preview
                Image preview = colorPreview.GetComponent<Image>();
                if (preview != null) preview.color = newColor;
                
                // Change line color on ACTIVE rotor - creates a NEW trail preserving the old one
                if (activeRoller != null)
                {
                    activeRoller.ChangeLineColor(newColor);
                }
            };
            
            hueSlider.onValueChanged.AddListener((val) => {
                updateColorAndPreview();
                Text text = hueSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                if (text != null) text.text = val.ToString("F2");
            });
            
            saturationSlider.onValueChanged.AddListener((val) => {
                updateColorAndPreview();
                Text text = saturationSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                if (text != null) text.text = val.ToString("F2");
            });
            
            valueSlider.onValueChanged.AddListener((val) => {
                updateColorAndPreview();
                Text text = valueSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                if (text != null) text.text = val.ToString("F2");
            });
        }
        
        // Reconnect color   preset buttons
        if (colorPresetButtons != null && colorPresetButtons.Length > 0)
        {
            Color[] presetColors = new Color[] {
                Color.white, Color.black, Color.red, new Color(1f, 0.5f, 0f), Color.yellow, Color.green,
                Color.cyan, Color.blue, new Color(0.5f, 0f, 1f), Color.magenta,
                new Color(1f, 0.4f, 0.7f), new Color(0.5f, 0.3f, 0.1f), new Color(0.8f, 0.8f, 0.8f),
                new Color(0.5f, 0.5f, 0.5f), new Color(0.3f, 0.3f, 0.3f), new Color(1f, 0.8f, 0f)
            };
            
            for (int i = 0; i < colorPresetButtons.Length && i < presetColors.Length; i++)
            {
                if (colorPresetButtons[i] != null)
                {
                    Button btn = colorPresetButtons[i];
                    Color color = presetColors[i];
                    
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log($"🎨 Preset button clicked! Color: {color}");
                        
                        SpirographRoller roller = FindFirstObjectByType<SpirographRoller>();
                        if (roller == null)
                        {
                            Debug.LogError("❌ SpirographRoller not found!");
                            return;
                        }
                        
                        try
                        {
                            // Update HSV sliders
                            float h, s, v;
                            Color.RGBToHSV(color, out h, out s, out v);
                            
                            if (hueSlider != null) 
                            {
                                hueSlider.SetValueWithoutNotify(h);
                                Text text = hueSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                                if (text != null) text.text = h.ToString("F2");
                            }
                            if (saturationSlider != null) 
                            {
                                saturationSlider.SetValueWithoutNotify(s);
                                Text text = saturationSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                                if (text != null) text.text = s.ToString("F2");
                            }
                            if (valueSlider != null) 
                            {
                                valueSlider.SetValueWithoutNotify(v);
                                Text text = valueSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                                if (text != null) text.text = v.ToString("F2");
                            }
                            
                            // Change color
                            roller.ChangeLineColor(color);
                            
                            // Update preview
                            if (colorPreview != null)
                            {
                                Image preview = colorPreview.GetComponent<Image>();
                                if (preview != null) preview.color = color;
                            }
                            
                            Debug.Log($"✅ Color changed to {color}");
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"❌ Error: {e.Message}");
                        }
                    });
                }
            }
            
            Debug.Log($"✓ Reconnected {colorPresetButtons.Length} color preset buttons!");
        }
        
        // Reconnect skybox dropdown
        if (skyboxDropdown != null)
        {
            skyboxDropdown.onValueChanged.RemoveAllListeners();
            
            SkyboxManager skyboxManager = FindFirstObjectByType<SkyboxManager>();
            if (skyboxManager == null)
            {
                // Create SkyboxManager if it doesn't exist
                GameObject managerObj = new GameObject("SkyboxManager");
                skyboxManager = managerObj.AddComponent<SkyboxManager>();
                Debug.Log("✓ Created SkyboxManager");
            }
            
            // Connect listener - changes skybox IMMEDIATELY when dropdown value changes
            skyboxDropdown.onValueChanged.AddListener((index) => {
                Debug.Log($"🌌 Skybox dropdown changed to index: {index}");
                skyboxManager.SetSkybox(index);
            });
            
            Debug.Log("✓ Skybox dropdown listener reconnected - will change skybox immediately!");
        }
        else
        {
            Debug.LogWarning("⚠ No color preset buttons found to reconnect!");
        }
        
        // Reconnect remaining sliders to active rotor
        if (activeRoller != null)
        {
            // Line Width Slider
            if (lineWidthSlider != null)
            {
                lineWidthSlider.onValueChanged.RemoveAllListeners();
                lineWidthSlider.onValueChanged.AddListener((value) => {
                    if (activeRoller != null)
                    {
                        activeRoller.lineWidth = value;
                    }
                });
            }
            
            // Line Brightness Slider
            if (lineBrightnessSlider != null)
            {
                lineBrightnessSlider.onValueChanged.RemoveAllListeners();
                lineBrightnessSlider.onValueChanged.AddListener((value) => {
                    if (activeRoller != null)
                    {
                        activeRoller.lineBrightness = value;
                    }
                });
            }
            
            // Pen Distance Slider (already in ConnectSlidersToActiveRotor)
            // Cycles Slider (already in ConnectSlidersToActiveRotor)
            
            // Object Rotation Speed Slider
            if (objectRotationSpeedSlider != null)
            {
                objectRotationSpeedSlider.onValueChanged.RemoveAllListeners();
                objectRotationSpeedSlider.onValueChanged.AddListener((value) => {
                    RotateParent rotateParent = FindFirstObjectByType<RotateParent>();
                    if (rotateParent != null)
                    {
                        // Use reflection to set the private rotationSpeedMultiplier field
                        var field = typeof(RotateParent).GetField("rotationSpeedMultiplier", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (field != null)
                        {
                            field.SetValue(rotateParent, value);
                        }
                        
                        // Update the slider text if it exists
                        Text sliderText = objectRotationSpeedSlider.GetComponentInChildren<Text>();
                        if (sliderText != null)
                        {
                            sliderText.text = "Object Rotation Speed: " + value.ToString("F1");
                        }
                    }
                });
            }
        }
        
        // Reconnect pause button
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(() => {
                if (activeRoller != null)
                {
                    activeRoller.TogglePause();
                    Text btnText = pauseButton.GetComponentInChildren<Text>();
                    if (btnText != null)
                    {
                        btnText.text = activeRoller.IsPaused() ? "▶ RESUME" : "⏸ PAUSE";
                    }
                    Debug.Log($"Rotor {(activeRoller.IsPaused() ? "Paused" : "Resumed")}");
                }
            });
        }
        
        // Reconnect reset button
        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(() => {
                if (activeRoller != null)
                {
                    activeRoller.ResetSpirograph();
                    Debug.Log("Rotor position reset");
                }
            });
        }
        
        // Reconnect toggle visuals button
        if (toggleVisualsButton != null)
        {
            toggleVisualsButton.onClick.RemoveAllListeners();
            toggleVisualsButton.onClick.AddListener(() => {
                if (activeRoller != null)
                {
                    activeRoller.ToggleVisuals();
                    Debug.Log("Rotor visuals toggled");
                }
            });
        }
        
        // Reconnect line effects button
        if (lineEffectsButton != null)
        {
            lineEffectsButton.onClick.RemoveAllListeners();
            lineEffectsButton.onClick.AddListener(() => {
                if (activeRoller != null)
                {
                    // Cycle through line effects using the lineEffectMode enum
                    int currentMode = (int)activeRoller.lineEffectMode;
                    int nextMode = (currentMode + 1) % 8; // 8 effect types: None, Glow, Rainbow, etc.
                    activeRoller.lineEffectMode = (SpirographRoller.LineEffectMode)nextMode;
                    Debug.Log($"Line effect changed to: {activeRoller.lineEffectMode}");
                }
            });
        }
        
        Debug.Log("✓ UI elements reconnected at runtime!");
    }
    
    void Update()
    {
        // Toggle UI visibility with Enter key using new Input System
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame))
        {
            ToggleUI();
        }
    }
    
    void ToggleUI()
    {
        isUIVisible = !isUIVisible;
        
        if (controlPanel != null)
        {
            StartCoroutine(AnimateUIToggle(controlPanel, isUIVisible));
        }
        
        // Update button text with icon
        if (hideUIButton != null)
        {
            Text buttonText = hideUIButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = isUIVisible ? "⊗ HIDE" : "⊕ SHOW";
            }
        }
        
        Debug.Log($"UI {(isUIVisible ? "Shown" : "Hidden")} (Press Enter to toggle)");
    }
    
    IEnumerator AnimateUIToggle(GameObject panel, bool show)
    {
        // IMPORTANT: Must activate panel BEFORE animating if showing
        if (show && !panel.activeSelf)
        {
            panel.SetActive(true);
        }
        
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }
        
        // If showing, start from hidden state
        if (show)
        {
            canvasGroup.alpha = 0f;
            panel.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
        }
        
        float duration = 0.3f;
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        float targetAlpha = show ? 1f : 0f;
        
        Vector3 startScale = panel.transform.localScale;
        Vector3 targetScale = show ? Vector3.one : new Vector3(0.95f, 0.95f, 1f);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Smooth  easing
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, smoothT);
            panel.transform.localScale = Vector3.Lerp(startScale, targetScale, smoothT);
            
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
        panel.transform.localScale = targetScale;
        canvasGroup.interactable = show;
        canvasGroup.blocksRaycasts = show;
        
        // Only deactivate if hiding
        if (!show)
        {
            panel.SetActive(false);
        }
    }
    
    [ContextMenu("Generate Complete UI")]
    void GenerateCompleteUI()
    {
        // Create EventSystem
        if (EventSystem.current == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
            Debug.Log("✓ Created EventSystem with InputSystemUIInputModule");
        }
        
        // Create Canvas
        GameObject canvasObj = new GameObject("SpirographCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create Hide UI Button (Top-Right Corner - ALWAYS VISIBLE) - Modern glassmorphic style
        hideUIButton = CreateModernButton(canvas.transform, "HideUIButton", new Vector2(-15, -15), new Vector2(90, 40), "⊗ HIDE");
        RectTransform hideButtonRect = hideUIButton.GetComponent<RectTransform>();
        hideButtonRect.anchorMin = new Vector2(1, 1); // Top-right anchor
        hideButtonRect.anchorMax = new Vector2(1, 1);
        hideButtonRect.pivot = new Vector2(1, 1);
        
        // Glassmorphic style for hide button
        Image hideButtonImage = hideUIButton.GetComponent<Image>();
        hideButtonImage.color = new Color(0.05f, 0.05f, 0.15f, 0.7f); // Deep space glass
        
        // Add subtle glow outline
        Outline hideOutline = hideUIButton.gameObject.AddComponent<Outline>();
        hideOutline.effectColor = new Color(0.4f, 0.6f, 1f, 0.5f); // Cyan glow
        hideOutline.effectDistance = new Vector2(1, -1);
        
        Text hideButtonText = hideUIButton.GetComponentInChildren<Text>();
        hideButtonText.fontSize = 14;
        hideButtonText.fontStyle = FontStyle.Bold;
        hideButtonText.color = new Color(0.8f, 0.9f, 1f, 0.95f); // Soft cyan-white
        
        // Create Panel Background - Modern Glassmorphism with cosmic theme - NOW SCROLLABLE!
        GameObject panel = new GameObject("ControlPanel");
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 0.5f);
        panelRect.anchoredPosition = new Vector2(15, 0);
        panelRect.sizeDelta = new Vector2(340, -80); // Full height minus padding (80 = 15 top + 15 bottom + 50 for title)
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.02f, 0.02f, 0.08f, 0.75f);
        
        // Add Canvas Group for smooth transitions
        CanvasGroup panelGroup = panel.AddComponent<CanvasGroup>();
        panelGroup.alpha = 1f;
        
        // Add subtle outer glow
        Shadow panelGlow = panel.AddComponent<Shadow>();
        panelGlow.effectColor = new Color(0.2f, 0.4f, 0.8f, 0.3f);
        panelGlow.effectDistance = new Vector2(0, 0);
        panelGlow.useGraphicAlpha = true;
        
        // Add border accent
        Outline panelOutline = panel.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0.3f, 0.5f, 0.9f, 0.25f);
        panelOutline.effectDistance = new Vector2(2, -2);
        
        // Add ScrollRect for scrolling
        ScrollRect panelScroll = panel.AddComponent<ScrollRect>();
        panelScroll.horizontal = false;
        panelScroll.vertical = true;
        panelScroll.scrollSensitivity = 20f;
        panelScroll.movementType = ScrollRect.MovementType.Clamped;
        panelScroll.inertia = true;
        panelScroll.decelerationRate = 0.135f;
        
        // Create Viewport for ScrollRect (fills the panel with padding)
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(panel.transform, false);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.pivot = new Vector2(0.5f, 0.5f);
        viewportRect.offsetMin = new Vector2(10, 10); // Padding
        viewportRect.offsetMax = new Vector2(-10, -10); // Padding
        
        // Add mask to viewport
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = Color.white; // Fully opaque white (mask hides it with showMaskGraphic=false)
        Mask viewportMask = viewport.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false; // Don't render the image, but use it for masking
        
        panelScroll.viewport = viewportRect;
        
        // Create Content container (this will hold all our UI elements)
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = new Vector2(0, 0);
        contentRect.sizeDelta = new Vector2(-20, 2000); // Width accounts for padding, height will be adjusted
        
        // Add VerticalLayoutGroup to automatically stack elements and collapse gaps
        VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = false;
        contentLayout.childForceExpandHeight = false;
        contentLayout.spacing = 10f; // Space between sections
        contentLayout.padding = new RectOffset(0, 0, 20, 20); // Top and bottom padding
        
        // Add ContentSizeFitter to auto-adjust height based on content
        ContentSizeFitter contentFitter = content.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        panelScroll.content = contentRect;
        
        // Store reference to control panel
        controlPanel = panel;
        
        // Now parent all UI elements to 'content' instead of 'panel'
        Transform uiParent = content.transform;
        
        float yPos = -20; // More top padding
        
        // Add panel title
        GameObject titleObj = new GameObject("PanelTitle");
        titleObj.transform.SetParent(uiParent, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        // Let VerticalLayoutGroup control positioning
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.sizeDelta = new Vector2(0, 35);
        
        // Add LayoutElement for title
        LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 35;
        titleLayout.flexibleHeight = 0;
        
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "✦ SPIROGRAPH CONTROLS";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 18;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = new Color(0.7f, 0.85f, 1f, 0.9f); // Bright cyan
        titleText.alignment = TextAnchor.MiddleCenter;
        
        // Add subtle glow to title
        Shadow titleShadow = titleObj.AddComponent<Shadow>();
        titleShadow.effectColor = new Color(0.3f, 0.6f, 1f, 0.5f);
        titleShadow.effectDistance = new Vector2(0, 0);
        
        yPos -= 55;
        
        // ============================================================
        // MOTION SECTION
        // ============================================================
        motionSectionToggle = CreateSectionHeader(uiParent, "MotionHeader", new Vector2(15, yPos), "⚡ MOTION & SPEED", true);
        yPos -= 45;
        
        motionSection = CreateSection(uiParent, "MotionSection", new Vector2(15, yPos));
        float motionYPos = -10;
        
        // Speed Slider
        speedSlider = CreateModernSlider(motionSection.transform, "SpeedSlider", new Vector2(0, motionYPos), 0f, 750f, 
            0f, out speedText, "Travel Speed", "0.0");
        motionYPos -= 70;
        
        // Cycles Slider
        Text cyclesText;
        cyclesSlider = CreateModernSlider(motionSection.transform, "CyclesSlider", new Vector2(0, motionYPos), 1f, 500f, 
            10f, out cyclesText, "Cycles", "10");
        cyclesSlider.wholeNumbers = true;
        motionYPos -= 70;
        
        // Rotation Speed Slider
        Text rotationSpeedText;
        rotationSpeedSlider = CreateModernSlider(motionSection.transform, "RotationSpeedSlider", new Vector2(0, motionYPos), 0f, 1f, 
            0.5f, out rotationSpeedText, "Rotation Speed", "0.5");
        motionYPos -= 70;
        
        // Object Rotation Speed Slider
        Text objectRotationSpeedText;
        objectRotationSpeedSlider = CreateModernSlider(motionSection.transform, "ObjectRotationSpeedSlider", new Vector2(0, motionYPos), 0f, 100f, 
            1f, out objectRotationSpeedText, "Object Rotation", "1.0");
        motionYPos -= 70;
        
        // Pen Distance Slider
        Text penDistanceText;
        penDistanceSlider = CreateModernSlider(motionSection.transform, "PenDistanceSlider", new Vector2(0, motionYPos), 0f, 5f, 
            0.3f, out penDistanceText, "Rotor Radius", "0.30x");
        motionYPos -= 75;
        
        // Set motion section height
        RectTransform motionRect = motionSection.GetComponent<RectTransform>();
        float motionHeight = Mathf.Abs(motionYPos) + 10;
        motionRect.sizeDelta = new Vector2(0, motionHeight);
        LayoutElement motionLayout = motionSection.GetComponent<LayoutElement>();
        if (motionLayout != null) motionLayout.preferredHeight = motionHeight;
        
        // ============================================================
        // VISUALS SECTION
        // ============================================================
        visualsSectionToggle = CreateSectionHeader(uiParent, "VisualsHeader", new Vector2(15, yPos), "🎨 VISUAL EFFECTS", true);
        yPos -= 45;
        
        visualsSection = CreateSection(uiParent, "VisualsSection", new Vector2(15, yPos));
        float visualsYPos = -10;
        
        // Line Width Slider
        Text lineWidthText;
        lineWidthSlider = CreateModernSlider(visualsSection.transform, "LineWidthSlider", new Vector2(0, visualsYPos), 0.01f, 2f, 
            0.3f, out lineWidthText, "Line Width", "0.30");
        visualsYPos -= 70;
        
        // Line Brightness Slider
        Text lineBrightnessText;
        lineBrightnessSlider = CreateModernSlider(visualsSection.transform, "LineBrightnessSlider", new Vector2(0, visualsYPos), 0f, 1f, 
            1f, out lineBrightnessText, "Line Brightness", "1.00");
        visualsYPos -= 75;
        
        // Pause/Reset Buttons
        pauseButton = CreateModernButton(visualsSection.transform, "PauseButton", new Vector2(0, visualsYPos), new Vector2(140, 38), "⏸ PAUSE");
        resetButton = CreateModernButton(visualsSection.transform, "ResetButton", new Vector2(150, visualsYPos), new Vector2(140, 38), "↻ RESET");
        visualsYPos -= 50;
        
        // Toggle Visuals Button
        toggleVisualsButton = CreateModernButton(visualsSection.transform, "ToggleVisualsButton", new Vector2(0, visualsYPos), new Vector2(290, 38), "👁 SHOW/HIDE");
        visualsYPos -= 50;
        
        // Line Effects Cycle Button
        lineEffectsButton = CreateModernButton(visualsSection.transform, "LineEffectsButton", new Vector2(0, visualsYPos), new Vector2(290, 38), "✨ LINE FX: Normal");
        Image effectsImg = lineEffectsButton.GetComponent<Image>();
        effectsImg.color = new Color(0.15f, 0.08f, 0.20f, 0.8f);
        visualsYPos -= 55;
        
        // Set visuals section height
        RectTransform visualsRect = visualsSection.GetComponent<RectTransform>();
        float visualsHeight = Mathf.Abs(visualsYPos) + 10;
        visualsRect.sizeDelta = new Vector2(0, visualsHeight);
        LayoutElement visualsLayout = visualsSection.GetComponent<LayoutElement>();
        if (visualsLayout != null) visualsLayout.preferredHeight = visualsHeight;
        
        // ============================================================
        // COLOR PICKER SECTION
        // ============================================================
        colorSectionToggle = CreateSectionHeader(uiParent, "ColorHeader", new Vector2(15, yPos), "🌈 COLOR PICKER", true);
        yPos -= 45;
        
        colorSection = CreateSection(uiParent, "ColorSection", new Vector2(15, yPos));
        float colorYPos = -10;
        
        // Color Preview Box
        GameObject previewObj = new GameObject("ColorPreview");
        previewObj.transform.SetParent(colorSection.transform, false);
        RectTransform previewRect = previewObj.AddComponent<RectTransform>();
        previewRect.anchorMin = new Vector2(0, 1);
        previewRect.anchorMax = new Vector2(0, 1);
        previewRect.pivot = new Vector2(0, 1);
        previewRect.anchoredPosition = new Vector2(0, colorYPos);
        previewRect.sizeDelta = new Vector2(290, 50);
        Image previewImage = previewObj.AddComponent<Image>();
        previewImage.color = Color.cyan;
        Outline previewOutline = previewObj.AddComponent<Outline>();
        previewOutline.effectColor = new Color(1f, 1f, 1f, 0.5f);
        previewOutline.effectDistance = new Vector2(2, -2);
        colorPreview = previewObj;
        colorYPos -= 60;
        
        // HSV Sliders
        Text hueText;
        hueSlider = CreateModernSlider(colorSection.transform, "HueSlider", new Vector2(0, colorYPos), 0f, 1f, 
            0.5f, out hueText, "Hue", "0.50");
        colorYPos -= 70;
        
        Text satText;
        saturationSlider = CreateModernSlider(colorSection.transform, "SaturationSlider", new Vector2(0, colorYPos), 0f, 1f, 
            0.8f, out satText, "Saturation", "0.80");
        colorYPos -= 70;
        
        Text valText;
        valueSlider = CreateModernSlider(colorSection.transform, "ValueSlider", new Vector2(0, colorYPos), 0f, 1f, 
            1f, out valText, "Brightness", "1.00");
        colorYPos -= 75;
        
        // Color Presets (2 rows of 6)
        AddColorPresetLabel(colorSection.transform, new Vector2(0, colorYPos), "Quick Colors:");
        colorYPos -= 25;
        
        colorPresetButtons = new Button[16];
        Color[] presetColors = new Color[] {
            Color.white, Color.black, Color.red, new Color(1f, 0.5f, 0f), Color.yellow, Color.green,
            Color.cyan, Color.blue, new Color(0.5f, 0f, 1f), Color.magenta,
            new Color(1f, 0.4f, 0.7f), new Color(0.5f, 0.3f, 0.1f), new Color(0.8f, 0.8f, 0.8f),
            new Color(0.5f, 0.5f, 0.5f), new Color(0.3f, 0.3f, 0.3f), new Color(1f, 0.8f, 0f)
        };
        
        for (int i = 0; i < 16; i++)
        {
            int row = i / 8;
            int col = i % 8;
            float xPos = col * 36f;
            float yPosPreset = colorYPos - (row * 32f);
            colorPresetButtons[i] = CreateColorPresetButton(colorSection.transform, $"Preset{i}", 
                new Vector2(xPos, yPosPreset), presetColors[i]);
        }
        colorYPos -= 72;
        
        // Set color section height
        RectTransform colorRect = colorSection.GetComponent<RectTransform>();
        float colorHeight = Mathf.Abs(colorYPos) + 10;
        colorRect.sizeDelta = new Vector2(0, colorHeight);
        LayoutElement colorLayout = colorSection.GetComponent<LayoutElement>();
        if (colorLayout != null) colorLayout.preferredHeight = colorHeight;
        
        // ============================================================
        // ENVIRONMENT SECTION
        // ============================================================
        environmentSectionToggle = CreateSectionHeader(uiParent, "EnvironmentHeader", new Vector2(15, yPos), "🌌 ENVIRONMENT", false);
        yPos -= 45;
        
        environmentSection = CreateSection(uiParent, "EnvironmentSection", new Vector2(15, yPos));
        environmentSection.SetActive(false); // Collapsed by default
        float envYPos = -10;
        
        // Skybox Label
        GameObject skyboxLabelObj = new GameObject("SkyboxLabel");
        skyboxLabelObj.transform.SetParent(environmentSection.transform, false);
        RectTransform skyboxLabelRect = skyboxLabelObj.AddComponent<RectTransform>();
        skyboxLabelRect.anchorMin = new Vector2(0, 1);
        skyboxLabelRect.anchorMax = new Vector2(0, 1);
        skyboxLabelRect.pivot = new Vector2(0, 1);
        skyboxLabelRect.anchoredPosition = new Vector2(0, envYPos);
        skyboxLabelRect.sizeDelta = new Vector2(290, 20);
        Text skyboxLabel = skyboxLabelObj.AddComponent<Text>();
        skyboxLabel.text = "Skybox Theme:";
        skyboxLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        skyboxLabel.fontSize = 12;
        skyboxLabel.fontStyle = FontStyle.Bold;
        skyboxLabel.color = new Color(0.7f, 0.85f, 1f, 0.9f);
        skyboxLabel.alignment = TextAnchor.MiddleLeft;
        envYPos -= 28;
        
        // Skybox Dropdown
        skyboxDropdown = CreateModernDropdown(environmentSection.transform, "SkyboxDropdown", new Vector2(0, envYPos));
        envYPos -= 80;
        
        // Pattern Generator Label
        GameObject patternLabelObj = new GameObject("PatternGeneratorLabel");
        patternLabelObj.transform.SetParent(environmentSection.transform, false);
        RectTransform patternLabelRect = patternLabelObj.AddComponent<RectTransform>();
        patternLabelRect.anchorMin = new Vector2(0, 1);
        patternLabelRect.anchorMax = new Vector2(0, 1);
        patternLabelRect.pivot = new Vector2(0, 1);
        patternLabelRect.anchoredPosition = new Vector2(0, envYPos);
        patternLabelRect.sizeDelta = new Vector2(290, 20);
        Text patternLabel = patternLabelObj.AddComponent<Text>();
        patternLabel.text = "Pattern Generator:";
        patternLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        patternLabel.fontSize = 12;
        patternLabel.fontStyle = FontStyle.Bold;
        patternLabel.color = new Color(0.7f, 0.85f, 1f, 0.9f);
        patternLabel.alignment = TextAnchor.MiddleLeft;
        envYPos -= 28;
        
        // Pattern Dropdown
        patternDropdown = CreatePatternDropdown(environmentSection.transform, "PatternDropdown", new Vector2(0, envYPos));
        envYPos -= 50;
        
        // Generate and Clear Buttons
        generatePatternButton = CreateModernButton(environmentSection.transform, "GeneratePatternButton", new Vector2(0, envYPos), new Vector2(140, 38), "✦ GENERATE");
        Image generateImg = generatePatternButton.GetComponent<Image>();
        generateImg.color = new Color(0.15f, 0.25f, 0.08f, 0.8f); // Green tint
        
        clearPatternsButton = CreateModernButton(environmentSection.transform, "ClearPatternsButton", new Vector2(150, envYPos), new Vector2(140, 38), "✖ CLEAR ALL");
        Image clearImg = clearPatternsButton.GetComponent<Image>();
        clearImg.color = new Color(0.25f, 0.08f, 0.08f, 0.8f); // Red tint
        envYPos -= 65;
        
        // ============================================================
        // MULTI-AGENT SYSTEM CONTROLS
        // ============================================================
        
        // Multi-Agent Mode Section Label
        GameObject multiAgentLabelObj = new GameObject("MultiAgentLabel");
        multiAgentLabelObj.transform.SetParent(environmentSection.transform, false);
        RectTransform multiAgentLabelRect = multiAgentLabelObj.AddComponent<RectTransform>();
        multiAgentLabelRect.anchorMin = new Vector2(0, 1);
        multiAgentLabelRect.anchorMax = new Vector2(0, 1);
        multiAgentLabelRect.pivot = new Vector2(0, 1);
        multiAgentLabelRect.anchoredPosition = new Vector2(0, envYPos);
        multiAgentLabelRect.sizeDelta = new Vector2(290, 20);
        Text multiAgentLabel = multiAgentLabelObj.AddComponent<Text>();
        multiAgentLabel.text = "Multi-Agent System:";
        multiAgentLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        multiAgentLabel.fontSize = 12;
        multiAgentLabel.fontStyle = FontStyle.Bold;
        multiAgentLabel.color = new Color(0.7f, 0.85f, 1f, 0.9f);
        multiAgentLabel.alignment = TextAnchor.MiddleLeft;
        envYPos -= 28;
        
        // Multi-Agent Toggle
        multiAgentToggle = CreateModernToggle(environmentSection.transform, "MultiAgentToggle", new Vector2(0, envYPos), "Enable Multi-Agent Mode");
        envYPos -= 45;
        
        // Agent Count Slider (initially hidden)
        GameObject agentCountContainer = new GameObject("AgentCountContainer");
        agentCountContainer.transform.SetParent(environmentSection.transform, false);
        RectTransform agentCountContainerRect = agentCountContainer.AddComponent<RectTransform>();
        agentCountContainerRect.anchorMin = new Vector2(0, 1);
        agentCountContainerRect.anchorMax = new Vector2(0, 1);
        agentCountContainerRect.pivot = new Vector2(0, 1);
        agentCountContainerRect.anchoredPosition = new Vector2(0, envYPos);
        agentCountContainerRect.sizeDelta = new Vector2(290, 70);
        agentCountContainer.SetActive(false); // Hidden until multi-agent enabled
        
        agentCountSlider = CreateModernSlider(agentCountContainer.transform, "AgentCountSlider", new Vector2(0, 0), 1f, 16f, 
            4f, out agentCountText, "Agent Count", "4");
        agentCountSlider.wholeNumbers = true;
        envYPos -= 75;
        
        // Agent Color Mode Dropdown (initially hidden)
        GameObject colorModeContainer = new GameObject("ColorModeContainer");
        colorModeContainer.transform.SetParent(environmentSection.transform, false);
        RectTransform colorModeContainerRect = colorModeContainer.AddComponent<RectTransform>();
        colorModeContainerRect.anchorMin = new Vector2(0, 1);
        colorModeContainerRect.anchorMax = new Vector2(0, 1);
        colorModeContainerRect.pivot = new Vector2(0, 1);
        colorModeContainerRect.anchoredPosition = new Vector2(0, envYPos);
        colorModeContainerRect.sizeDelta = new Vector2(290, 70);
        colorModeContainer.SetActive(false); // Hidden until multi-agent enabled
        
        GameObject colorModeLabelObj = new GameObject("ColorModeLabel");
        colorModeLabelObj.transform.SetParent(colorModeContainer.transform, false);
        RectTransform colorModeLabelRect = colorModeLabelObj.AddComponent<RectTransform>();
        colorModeLabelRect.anchorMin = new Vector2(0, 1);
        colorModeLabelRect.anchorMax = new Vector2(0, 1);
        colorModeLabelRect.pivot = new Vector2(0, 1);
        colorModeLabelRect.anchoredPosition = new Vector2(0, 0);
        colorModeLabelRect.sizeDelta = new Vector2(290, 20);
        Text colorModeLabel = colorModeLabelObj.AddComponent<Text>();
        colorModeLabel.text = "Agent Color Mode:";
        colorModeLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        colorModeLabel.fontSize = 11;
        colorModeLabel.fontStyle = FontStyle.Bold;
        colorModeLabel.color = new Color(0.6f, 0.75f, 0.9f, 0.85f);
        colorModeLabel.alignment = TextAnchor.MiddleLeft;
        
        agentColorModeDropdown = CreateCompactDropdown(colorModeContainer.transform, "AgentColorModeDropdown", new Vector2(0, -25), 
            new string[] { "Master", "Rainbow", "Individual", "Custom" });
        envYPos -= 75;
        
        // Agent Spawn Mode Dropdown (initially hidden)
        GameObject spawnModeContainer = new GameObject("SpawnModeContainer");
        spawnModeContainer.transform.SetParent(environmentSection.transform, false);
        RectTransform spawnModeContainerRect = spawnModeContainer.AddComponent<RectTransform>();
        spawnModeContainerRect.anchorMin = new Vector2(0, 1);
        spawnModeContainerRect.anchorMax = new Vector2(0, 1);
        spawnModeContainerRect.pivot = new Vector2(0, 1);
        spawnModeContainerRect.anchoredPosition = new Vector2(0, envYPos);
        spawnModeContainerRect.sizeDelta = new Vector2(290, 70);
        spawnModeContainer.SetActive(false); // Hidden until multi-agent enabled
        
        GameObject spawnModeLabelObj = new GameObject("SpawnModeLabel");
        spawnModeLabelObj.transform.SetParent(spawnModeContainer.transform, false);
        RectTransform spawnModeLabelRect = spawnModeLabelObj.AddComponent<RectTransform>();
        spawnModeLabelRect.anchorMin = new Vector2(0, 1);
        spawnModeLabelRect.anchorMax = new Vector2(0, 1);
        spawnModeLabelRect.pivot = new Vector2(0, 1);
        spawnModeLabelRect.anchoredPosition = new Vector2(0, 0);
        spawnModeLabelRect.sizeDelta = new Vector2(290, 20);
        Text spawnModeLabel = spawnModeLabelObj.AddComponent<Text>();
        spawnModeLabel.text = "Spawn Mode:";
        spawnModeLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        spawnModeLabel.fontSize = 11;
        spawnModeLabel.fontStyle = FontStyle.Bold;
        spawnModeLabel.color = new Color(0.6f, 0.75f, 0.9f, 0.85f);
        spawnModeLabel.alignment = TextAnchor.MiddleLeft;
        
        agentSpawnModeDropdown = CreateCompactDropdown(spawnModeContainer.transform, "AgentSpawnModeDropdown", new Vector2(0, -25), 
            new string[] { "Simultaneous", "Sequential", "Staggered", "Competitive" });
        envYPos -= 80;
        
        // Set environment section height
        RectTransform envRect = environmentSection.GetComponent<RectTransform>();
        float envHeight = Mathf.Abs(envYPos) + 10;
        envRect.sizeDelta = new Vector2(0, envHeight);
        LayoutElement envLayout = environmentSection.GetComponent<LayoutElement>();
        if (envLayout != null) envLayout.preferredHeight = envHeight;
        
        // ============================================================
        // CAMERA SECTION
        // ============================================================
        cameraSectionToggle = CreateSectionHeader(uiParent, "CameraHeader", new Vector2(15, yPos), "📷 CAMERA CONTROLS", false);
        yPos -= 45;
        
        cameraSection = CreateSection(uiParent, "CameraSection", new Vector2(15, yPos));
        cameraSection.SetActive(false); // Collapsed by default
        float cameraYPos = -10;
        
        // Camera Mode Buttons - IMPORTANT: These are assigned to public fields
        lookAtButton = CreateModernButton(cameraSection.transform, "LookAtButton", new Vector2(0, cameraYPos), new Vector2(140, 38), "✈ Free Fly");
        smoothFollowButton = CreateModernButton(cameraSection.transform, "SmoothFollowButton", new Vector2(150, cameraYPos), new Vector2(140, 38), "◎ Follow");
        cameraYPos -= 50;
        
        autoOrbitButton = CreateModernButton(cameraSection.transform, "AutoOrbitButton", new Vector2(0, cameraYPos), new Vector2(290, 38), "🎬 AUTO ORBIT");
        Image orbitImg = autoOrbitButton.GetComponent<Image>();
        orbitImg.color = new Color(0.15f, 0.05f, 0.25f, 0.8f);
        cameraYPos -= 55;
        
        #if UNITY_EDITOR
        // Force serialization of button references
        if (lookAtButton != null) UnityEditor.EditorUtility.SetDirty(lookAtButton.gameObject);
        if (smoothFollowButton != null) UnityEditor.EditorUtility.SetDirty(smoothFollowButton.gameObject);
        if (autoOrbitButton != null) UnityEditor.EditorUtility.SetDirty(autoOrbitButton.gameObject);
        #endif
        
        // Connect camera buttons to CameraController
        CameraController cameraController = FindFirstObjectByType<CameraController>();
        if (cameraController != null)
        {
            lookAtButton.onClick.AddListener(() => {
                cameraController.SetCameraMode(CameraController.CameraMode.FreeFly);
                Debug.Log("Camera Mode: Free Fly");
            });
            
            smoothFollowButton.onClick.AddListener(() => {
                cameraController.SetCameraMode(CameraController.CameraMode.SmoothFollow);
                Debug.Log("Camera Mode: Smooth Follow");
            });
            
            autoOrbitButton.onClick.AddListener(() => {
                cameraController.SetCameraMode(CameraController.CameraMode.AutoOrbit);
                Debug.Log("Camera Mode: Auto Orbit");
            });
            
            Debug.Log("✓ Camera buttons connected to CameraController!");
        }
        else
        {
            Debug.LogWarning("⚠ CameraController not found in scene! Camera buttons will not function.");
        }
        
        // Set camera section height
        RectTransform cameraRect = cameraSection.GetComponent<RectTransform>();
        float cameraHeight = Mathf.Abs(cameraYPos) + 10;
        cameraRect.sizeDelta = new Vector2(0, cameraHeight);
        LayoutElement cameraLayout = cameraSection.GetComponent<LayoutElement>();
        if (cameraLayout != null) cameraLayout.preferredHeight = cameraHeight;
        
        // Setup section toggle functionality
        SetupSectionToggle(motionSectionToggle, motionSection);
        SetupSectionToggle(visualsSectionToggle, visualsSection);
        SetupSectionToggle(colorSectionToggle, colorSection);
        SetupSectionToggle(environmentSectionToggle, environmentSection);
        SetupSectionToggle(cameraSectionToggle, cameraSection);
        
        // Setup HSV sliders to update preview AND create new trail with new color
        if (hueSlider != null && saturationSlider != null && valueSlider != null && colorPreview != null)
        {
            System.Action updateColorAndPreview = () => {
                float h = hueSlider.value;
                float s = saturationSlider.value;
                float v = valueSlider.value;
                Color newColor = Color.HSVToRGB(h, s, v);
                
                // Update preview
                Image preview = colorPreview.GetComponent<Image>();
                if (preview != null) preview.color = newColor;
                
                // Change line color on ACTIVE rotor - creates a NEW trail preserving the old one
                if (activeRoller != null)
                {
                    activeRoller.ChangeLineColor(newColor);
                }
            };
            
            hueSlider.onValueChanged.AddListener((val) => {
                updateColorAndPreview();
                Text text = hueSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                if (text != null) text.text = val.ToString("F2");
            });
            
            saturationSlider.onValueChanged.AddListener((val) => {
                updateColorAndPreview();
                Text text = saturationSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                if (text != null) text.text = val.ToString("F2");
            });
            
            valueSlider.onValueChanged.AddListener((val) => {
                updateColorAndPreview();
                Text text = valueSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                if (text != null) text.text = val.ToString("F2");
            });
        }
        
        // Instructions - Modern style
        GameObject instructions = new GameObject("Instructions");
        instructions.transform.SetParent(uiParent, false);
        RectTransform instructRect = instructions.AddComponent<RectTransform>();
        // Let VerticalLayoutGroup control positioning
        instructRect.anchorMin = new Vector2(0, 1);
        instructRect.anchorMax = new Vector2(1, 1);
        instructRect.pivot = new Vector2(0.5f, 1);
        instructRect.sizeDelta = new Vector2(0, 110);
        
        // Add LayoutElement for instructions
        LayoutElement instructLayout = instructions.AddComponent<LayoutElement>();
        instructLayout.preferredHeight = 110;
        instructLayout.flexibleHeight = 0;
        
        // Add subtle background for instructions
        Image instructBg = instructions.AddComponent<Image>();
        instructBg.color = new Color(0.05f, 0.05f, 0.15f, 0.4f);
        
        GameObject instructTextObj = new GameObject("InstructionsText");
        instructTextObj.transform.SetParent(instructions.transform, false);
        RectTransform instructTextRect = instructTextObj.AddComponent<RectTransform>();
        instructTextRect.anchorMin = Vector2.zero;
        instructTextRect.anchorMax = Vector2.one;
        instructTextRect.offsetMin = new Vector2(8, 8);
        instructTextRect.offsetMax = new Vector2(-8, -8);
        
        Text instructText = instructTextObj.AddComponent<Text>();
        instructText.text = "💡 Click section headers (▼/▶) to expand/collapse\n\n⌨ WASD/ZQSD: Move • Shift: Sprint\n🖱 Right Click: Look • Scroll: Zoom\n\n🎨 Click color swatches for instant colors\n🌌 Choose skybox from Environment section";
        instructText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        instructText.fontSize = 10;
        instructText.color = new Color(0.6f, 0.75f, 0.9f, 0.85f);
        instructText.alignment = TextAnchor.UpperLeft;
        instructText.lineSpacing = 1.15f;
        
        // ContentSizeFitter will automatically adjust content height based on LayoutElements
        
        // Add hint text for Hide UI button - Modern style
        GameObject hintText = new GameObject("HideUIHint");
        hintText.transform.SetParent(canvas.transform, false);
        RectTransform hintRect = hintText.AddComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(1, 1);
        hintRect.anchorMax = new Vector2(1, 1);
        hintRect.pivot = new Vector2(1, 1);
        hintRect.anchoredPosition = new Vector2(-15, -60);
        hintRect.sizeDelta = new Vector2(180, 22);
        Text hint = hintText.AddComponent<Text>();
        hint.text = "⏎ ENTER to toggle";
        hint.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hint.fontSize = 10;
        hint.color = new Color(0.5f, 0.7f, 1f, 0.5f);
        hint.alignment = TextAnchor.MiddleRight;
        hint.fontStyle = FontStyle.Italic;
        
        // Add glow effect
        Shadow hintGlow = hintText.AddComponent<Shadow>();
        hintGlow.effectColor = new Color(0.3f, 0.5f, 1f, 0.3f);
        hintGlow.effectDistance = new Vector2(0, 0);
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.Selection.activeGameObject = canvasObj;
        #endif
        
        Debug.Log("✓ Generated complete UI! Canvas selected in Hierarchy.");
        Debug.Log("✓ HIDE UI BUTTON: Top-right corner (always visible) - Press ENTER to toggle!");
        Debug.Log("✓ You can now customize colors, sizes, and positions.");
        Debug.Log("✓ Don't forget to assign 4 materials to SpirographRoller for the material buttons!");
        Debug.Log("✓ UI automatically connects to SpirographRoller, RotateParent, and CameraController.");
        
        // Log button assignments for verification
        Debug.Log($"✓ Camera Buttons Created: LookAt={lookAtButton != null}, SmoothFollow={smoothFollowButton != null}, AutoOrbit={autoOrbitButton != null}");
        if (lookAtButton != null) Debug.Log($"  → Look At Button: {lookAtButton.name}");
        if (smoothFollowButton != null) Debug.Log($"  → Smooth Follow Button: {smoothFollowButton.name}");
        if (autoOrbitButton != null) Debug.Log($"  → Auto Orbit Button: {autoOrbitButton.name}");
    }
    
    Slider CreateModernSlider(Transform parent, string name, Vector2 position, float min, float max, float value, out Text valueLabel, string labelText, string valueText)
    {
        // Container
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent, false);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 1);
        sliderRect.anchorMax = new Vector2(0, 1);
        sliderRect.pivot = new Vector2(0, 1);
        sliderRect.anchoredPosition = position;
        sliderRect.sizeDelta = new Vector2(290, 12); // Half height (was 24)
        
        // Label (left aligned)
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(sliderObj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 1);
        labelRect.anchorMax = new Vector2(0, 1);
        labelRect.pivot = new Vector2(0, 1);
        labelRect.anchoredPosition = new Vector2(0, 26);
        labelRect.sizeDelta = new Vector2(180, 20);
        Text label = labelObj.AddComponent<Text>();
        label.text = labelText;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 12;
        label.fontStyle = FontStyle.Bold;
        label.color = new Color(0.7f, 0.85f, 1f, 0.9f); // Cyan tint
        label.alignment = TextAnchor.MiddleLeft;
        
        // Value Label (right aligned)
        GameObject valueLabelObj = new GameObject("ValueLabel");
        valueLabelObj.transform.SetParent(sliderObj.transform, false);
        RectTransform valueLabelRect = valueLabelObj.AddComponent<RectTransform>();
        valueLabelRect.anchorMin = new Vector2(1, 1);
        valueLabelRect.anchorMax = new Vector2(1, 1);
        valueLabelRect.pivot = new Vector2(1, 1);
        valueLabelRect.anchoredPosition = new Vector2(0, 26);
        valueLabelRect.sizeDelta = new Vector2(100, 20);
        valueLabel = valueLabelObj.AddComponent<Text>();
        valueLabel.text = valueText;
        valueLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        valueLabel.fontSize = 11;
        valueLabel.color = new Color(0.5f, 0.7f, 1f, 0.8f); // Softer cyan
        valueLabel.alignment = TextAnchor.MiddleRight;
        
        // Background (glassmorphic)
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.15f, 0.25f, 0.4f); // Deep space glass
        
        // Add subtle outline
        Outline bgOutline = bg.AddComponent<Outline>();
        bgOutline.effectColor = new Color(0.2f, 0.4f, 0.7f, 0.3f);
        bgOutline.effectDistance = new Vector2(1, -1);
        
        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(2, 2); // Smaller padding for thinner slider
        fillAreaRect.offsetMax = new Vector2(-10, -2);
        
        // Fill (gradient cyan-blue)
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.3f, 0.6f, 1f, 0.6f); // Bright cyan
        
        // Add glow to fill
        Shadow fillGlow = fill.AddComponent<Shadow>();
        fillGlow.effectColor = new Color(0.4f, 0.7f, 1f, 0.5f);
        fillGlow.effectDistance = new Vector2(0, 0);
        
        // Handle Area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(6, 0); // Adjusted for thinner slider
        handleAreaRect.offsetMax = new Vector2(-6, 0);
        
        // Handle (modern circular design)
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(10, 10); // Smaller handle (was 16x16)
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(0.9f, 0.95f, 1f, 1f); // Bright white-cyan
        
        // Add handle glow
        Shadow handleGlow = handle.AddComponent<Shadow>();
        handleGlow.effectColor = new Color(0.4f, 0.7f, 1f, 0.8f);
        handleGlow.effectDistance = new Vector2(0, 0);
        
        // Slider
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;
        
        return slider;
    }
    
    Button CreateSectionHeader(Transform parent, string name, Vector2 position, string headerText, bool startExpanded)
    {
        GameObject headerObj = new GameObject(name);
        headerObj.transform.SetParent(parent, false);
        RectTransform headerRect = headerObj.AddComponent<RectTransform>();
        // Let VerticalLayoutGroup control positioning
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.sizeDelta = new Vector2(0, 35);
        
        // Add Image FIRST (required for Button)
        Image headerImage = headerObj.AddComponent<Image>();
        headerImage.color = new Color(0.12f, 0.15f, 0.25f, 0.9f);
        headerImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        headerImage.type = Image.Type.Sliced;
        
        // Add Button and set target graphic
        Button button = headerObj.AddComponent<Button>();
        button.targetGraphic = headerImage;
        button.transition = Selectable.Transition.ColorTint;
        
        // Add LayoutElement so it works with VerticalLayoutGroup
        LayoutElement headerLayout = headerObj.AddComponent<LayoutElement>();
        headerLayout.preferredHeight = 35;
        headerLayout.flexibleHeight = 0;
        
        Outline headerOutline = headerObj.AddComponent<Outline>();
        headerOutline.effectColor = new Color(0.4f, 0.6f, 1f, 0.4f);
        headerOutline.effectDistance = new Vector2(1, -1);
        
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.2f, 1f);
        colors.pressedColor = new Color(0.9f, 0.9f, 1f, 1f);
        colors.colorMultiplier = 1f;
        button.colors = colors;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(headerObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        Text text = textObj.AddComponent<Text>();
        text.text = (startExpanded ? "▼ " : "▶ ") + headerText;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 13;
        text.fontStyle = FontStyle.Bold;
        text.color = new Color(0.8f, 0.95f, 1f, 1f);
        text.alignment = TextAnchor.MiddleLeft;
        
        RectTransform textRectPadding = text.GetComponent<RectTransform>();
        textRectPadding.offsetMin = new Vector2(10, 0);
        textRectPadding.offsetMax = new Vector2(-10, 0);
        
        return button;
    }
    
    GameObject CreateSection(Transform parent, string name, Vector2 position)
    {
        GameObject section = new GameObject(name);
        section.transform.SetParent(parent, false);
        RectTransform sectionRect = section.AddComponent<RectTransform>();
        // Let VerticalLayoutGroup control positioning
        sectionRect.anchorMin = new Vector2(0, 1);
        sectionRect.anchorMax = new Vector2(1, 1);
        sectionRect.pivot = new Vector2(0.5f, 1);
        sectionRect.sizeDelta = new Vector2(0, 100); // Temp, will be adjusted
        
        Image sectionBg = section.AddComponent<Image>();
        sectionBg.color = new Color(0.03f, 0.03f, 0.1f, 0.5f);
        
        // Add LayoutElement so it works with VerticalLayoutGroup
        LayoutElement sectionLayout = section.AddComponent<LayoutElement>();
        sectionLayout.preferredHeight = 100; // Will be updated per section
        sectionLayout.flexibleHeight = 0;
        
        return section;
    }
    
    void SetupSectionToggle(Button toggleButton, GameObject section)
    {
        if (toggleButton == null || section == null) return;
        
        // Get references (height will be read dynamically later)
        RectTransform sectionRect = section.GetComponent<RectTransform>();
        LayoutElement sectionLayout = section.GetComponent<LayoutElement>();
        
        // Remove old listeners to prevent duplicates
        toggleButton.onClick.RemoveAllListeners();
        
        toggleButton.onClick.AddListener(() => {
            bool willBeActive = !section.activeSelf;
            
            // Get the actual height from the LayoutElement (this is the correct height)
            float fullHeight = sectionLayout != null ? sectionLayout.preferredHeight : sectionRect.sizeDelta.y;
            
            // If collapsing, we just set to 0
            // If expanding, we restore the full height
            if (sectionLayout != null)
            {
                // When expanding, make sure we have the correct height stored
                if (willBeActive && fullHeight <= 0)
                {
                    // Height wasn't stored properly, get it from sizeDelta
                    fullHeight = sectionRect.sizeDelta.y;
                    if (fullHeight <= 0) fullHeight = 100; // Fallback
                }
                
                sectionLayout.preferredHeight = willBeActive ? fullHeight : 0;
                // Mark layout as dirty to force recalculation
                LayoutRebuilder.MarkLayoutForRebuild(sectionLayout.GetComponent<RectTransform>());
            }
            
            // Then change active state
            section.SetActive(willBeActive);
            
            // Update arrow icon
            Text buttonText = toggleButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                string[] parts = buttonText.text.Split(' ');
                if (parts.Length > 1)
                {
                    string arrow = willBeActive ? "▼" : "▶";
                    buttonText.text = arrow + " " + string.Join(" ", parts, 1, parts.Length - 1);
                }
            }
            
            // Force complete layout rebuild (multiple passes for reliability)
            RectTransform contentRect = section.transform.parent.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            Canvas.ForceUpdateCanvases();
            
            // Second pass to ensure everything updates
            UnityEngine.Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            
            Debug.Log($"Section {section.name} toggled: {(willBeActive ? "EXPANDED" : "COLLAPSED")} - Height: {fullHeight}");
        });
    }
    
    void AddColorPresetLabel(Transform parent, Vector2 position, string labelText)
    {
        GameObject labelObj = new GameObject("PresetLabel");
        labelObj.transform.SetParent(parent, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 1);
        labelRect.anchorMax = new Vector2(0, 1);
        labelRect.pivot = new Vector2(0, 1);
        labelRect.anchoredPosition = position;
        labelRect.sizeDelta = new Vector2(290, 20);
        Text label = labelObj.AddComponent<Text>();
        label.text = labelText;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 11;
        label.fontStyle = FontStyle.Bold;
        label.color = new Color(0.6f, 0.75f, 0.9f, 0.85f);
        label.alignment = TextAnchor.MiddleLeft;
    }
    
    Button CreateColorPresetButton(Transform parent, string name, Vector2 position, Color color)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0, 1);
        buttonRect.anchorMax = new Vector2(0, 1);
        buttonRect.pivot = new Vector2(0, 1);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(32, 28);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = color;
        
        Outline buttonOutline = buttonObj.AddComponent<Outline>();
        buttonOutline.effectColor = new Color(1f, 1f, 1f, 0.5f);
        buttonOutline.effectDistance = new Vector2(1, -1);
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.colorMultiplier = 1f;
        button.colors = colors;
        
        // Connect to ACTIVE SpirographRoller
        button.onClick.AddListener(() => {
            Debug.Log($"🎨 Color preset button clicked! Color: {color}");
            
            if (activeRoller == null)
            {
                Debug.LogError("❌ Active SpirographRoller not found! Cannot change color.");
                return;
            }
            
            Debug.Log($"✓ Found active SpirographRoller, changing color...");
            
            try
            {
                // Update HSV sliders first
                float h, s, v;
                Color.RGBToHSV(color, out h, out s, out v);
                
                if (hueSlider != null) 
                {
                    hueSlider.SetValueWithoutNotify(h);
                    Text text = hueSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                    if (text != null) text.text = h.ToString("F2");
                }
                if (saturationSlider != null) 
                {
                    saturationSlider.SetValueWithoutNotify(s);
                    Text text = saturationSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                    if (text != null) text.text = s.ToString("F2");
                }
                if (valueSlider != null) 
                {
                    valueSlider.SetValueWithoutNotify(v);
                    Text text = valueSlider.transform.Find("ValueLabel")?.GetComponent<Text>();
                    if (text != null) text.text = v.ToString("F2");
                }
                
                // Change line color on ACTIVE rotor - this creates a NEW trail with the new color
                // while preserving the old trail in its current color
                activeRoller.ChangeLineColor(color);
                
                // Update preview
                if (colorPreview != null)
                {
                    Image preview = colorPreview.GetComponent<Image>();
                    if (preview != null) preview.color = color;
                }
                
                Debug.Log($"✓ Color changed successfully to {color}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error changing color: {e.Message}\n{e.StackTrace}");
            }
        });
        
        return button;
    }
    
    Dropdown CreateModernDropdown(Transform parent, string name, Vector2 position)
    {
        // Main Dropdown GameObject
        GameObject dropdownObj = new GameObject(name);
        dropdownObj.transform.SetParent(parent, false);
        RectTransform dropdownRect = dropdownObj.AddComponent<RectTransform>();
        dropdownRect.anchorMin = new Vector2(0, 1);
        dropdownRect.anchorMax = new Vector2(0, 1);
        dropdownRect.pivot = new Vector2(0, 1);
        dropdownRect.anchoredPosition = position;
        dropdownRect.sizeDelta = new Vector2(290, 38);
        
        // Background
        Image dropdownBg = dropdownObj.AddComponent<Image>();
        dropdownBg.color = new Color(0.08f, 0.12f, 0.22f, 0.7f);
        dropdownBg.raycastTarget = true;
        
        Outline dropdownOutline = dropdownObj.AddComponent<Outline>();
        dropdownOutline.effectColor = new Color(0.3f, 0.5f, 0.8f, 0.4f);
        dropdownOutline.effectDistance = new Vector2(1, -1);
        
        // Dropdown Component
        Dropdown dropdown = dropdownObj.AddComponent<Dropdown>();
        dropdown.targetGraphic = dropdownBg;
        
        // Label (shows current selection)
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(10, 2);
        labelRect.offsetMax = new Vector2(-30, -2);
        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = "Starfield";
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 13;
        labelText.fontStyle = FontStyle.Bold;
        labelText.color = new Color(0.85f, 0.95f, 1f, 0.95f);
        labelText.alignment = TextAnchor.MiddleLeft;
        dropdown.captionText = labelText;
        
        // Arrow
        GameObject arrowObj = new GameObject("Arrow");
        arrowObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform arrowRect = arrowObj.AddComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(1, 0);
        arrowRect.anchorMax = new Vector2(1, 1);
        arrowRect.offsetMin = new Vector2(-25, 0);
        arrowRect.offsetMax = new Vector2(-5, 0);
        Text arrowText = arrowObj.AddComponent<Text>();
        arrowText.text = "▼";
        arrowText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        arrowText.fontSize = 12;
        arrowText.color = new Color(0.6f, 0.8f, 1f, 0.8f);
        arrowText.alignment = TextAnchor.MiddleCenter;
        
        // Template (the dropdown list that appears) - POSITIONED TO THE RIGHT
        GameObject templateObj = new GameObject("Template");
        templateObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform templateRect = templateObj.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(1, 0.5f); // Anchor to right middle of dropdown
        templateRect.anchorMax = new Vector2(1, 0.5f);
        templateRect.pivot = new Vector2(0, 0.5f); // Pivot on left middle so it extends right
        templateRect.anchoredPosition = new Vector2(10, 0); // 10 pixels to the right
        templateRect.sizeDelta = new Vector2(250, 200); // Fixed width, taller for better visibility
        
        Image templateBg = templateObj.AddComponent<Image>();
        templateBg.color = new Color(0.05f, 0.08f, 0.15f, 0.95f);
        templateBg.raycastTarget = true;
        
        // Add Canvas component to template to break out of parent's clipping
        Canvas templateCanvas = templateObj.AddComponent<Canvas>();
        templateCanvas.overrideSorting = true;
        templateCanvas.sortingOrder = 1000; // Render on top of everything
        
        // Add CanvasGroup to ensure it renders properly
        CanvasGroup templateGroup = templateObj.AddComponent<CanvasGroup>();
        templateGroup.blocksRaycasts = true;
        
        templateObj.AddComponent<GraphicRaycaster>();
        
        Outline templateOutline = templateObj.AddComponent<Outline>();
        templateOutline.effectColor = new Color(0.3f, 0.5f, 0.8f, 0.5f);
        templateOutline.effectDistance = new Vector2(2, -2);
        
        ScrollRect scrollRect = templateObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 10;
        
        // Viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(templateObj.transform, false);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        
        Image viewportMask = viewportObj.AddComponent<Image>();
        viewportMask.color = Color.white;
        Mask mask = viewportObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        
        scrollRect.viewport = viewportRect;
        
        // Content
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 150);
        
        scrollRect.content = contentRect;
        
        // Item (template for each option)
        GameObject itemObj = new GameObject("Item");
        itemObj.transform.SetParent(contentObj.transform, false);
        RectTransform itemRect = itemObj.AddComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 1);
        itemRect.anchorMax = new Vector2(1, 1);
        itemRect.pivot = new Vector2(0.5f, 1);
        itemRect.anchoredPosition = Vector2.zero;
        itemRect.sizeDelta = new Vector2(0, 30);
        
        Toggle itemToggle = itemObj.AddComponent<Toggle>();
        itemToggle.isOn = false;
        itemToggle.interactable = true;
        
        Image itemBg = itemObj.AddComponent<Image>();
        itemBg.color = new Color(0.1f, 0.15f, 0.25f, 1f);
        itemBg.raycastTarget = true;
        itemToggle.targetGraphic = itemBg;
        
        ColorBlock toggleColors = itemToggle.colors;
        toggleColors.normalColor = new Color(1f, 1f, 1f, 1f);
        toggleColors.highlightedColor = new Color(0.8f, 0.95f, 1f, 1f);
        toggleColors.pressedColor = new Color(0.6f, 0.8f, 1f, 1f);
        toggleColors.selectedColor = new Color(0.4f, 0.7f, 1f, 1f);
        itemToggle.colors = toggleColors;
        
        // Item Label
        GameObject itemLabelObj = new GameObject("Item Label");
        itemLabelObj.transform.SetParent(itemObj.transform, false);
        RectTransform itemLabelRect = itemLabelObj.AddComponent<RectTransform>();
        itemLabelRect.anchorMin = Vector2.zero;
        itemLabelRect.anchorMax = Vector2.one;
        itemLabelRect.offsetMin = new Vector2(10, 1);
        itemLabelRect.offsetMax = new Vector2(-10, -1);
        Text itemLabelText = itemLabelObj.AddComponent<Text>();
        itemLabelText.text = "Option";
        itemLabelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        itemLabelText.fontSize = 12;
        itemLabelText.color = new Color(0.85f, 0.95f, 1f, 0.95f);
        itemLabelText.alignment = TextAnchor.MiddleLeft;
        dropdown.itemText = itemLabelText;
        
        dropdown.template = templateRect;
        templateObj.SetActive(false);
        
        // Populate with skybox names and connect to SkyboxManager
        SkyboxManager skyboxManager = FindFirstObjectByType<SkyboxManager>();
        if (skyboxManager == null)
        {
            // Create SkyboxManager if it doesn't exist
            GameObject managerObj = new GameObject("SkyboxManager");
            skyboxManager = managerObj.AddComponent<SkyboxManager>();
        }
        
        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>(skyboxManager.GetAllSkyboxNames()));
        
        // Set current value
        int currentIndex = skyboxManager.GetCurrentSkyboxIndex();
        if (currentIndex >= 0)
        {
            dropdown.value = currentIndex;
        }
        
        // Connect listener
        dropdown.onValueChanged.AddListener((index) => {
            skyboxManager.SetSkybox(index);
        });
        
        return dropdown;
    }
    
    Button CreateModernButton(Transform parent, string name, Vector2 position, Vector2 size, string buttonText)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0, 1);
        buttonRect.anchorMax = new Vector2(0, 1);
        buttonRect.pivot = new Vector2(0, 1);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = size;
        
        // Glassmorphic background
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.08f, 0.12f, 0.22f, 0.7f); // Deep space glass
        
        // Add subtle outline
        Outline buttonOutline = buttonObj.AddComponent<Outline>();
        buttonOutline.effectColor = new Color(0.3f, 0.5f, 0.8f, 0.4f); // Cyan glow
        buttonOutline.effectDistance = new Vector2(1, -1);
        
        // Add hover glow effect
        Shadow buttonGlow = buttonObj.AddComponent<Shadow>();
        buttonGlow.effectColor = new Color(0.2f, 0.4f, 0.8f, 0.3f);
        buttonGlow.effectDistance = new Vector2(0, 0);
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        
        // Setup hover colors
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 1f);
        colors.highlightedColor = new Color(0.85f, 0.95f, 1f, 1f); // Bright on hover
        colors.pressedColor = new Color(0.6f, 0.8f, 1f, 1f); // Cyan on press
        colors.selectedColor = new Color(0.85f, 0.95f, 1f, 1f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        colors.colorMultiplier = 1.2f;
        colors.fadeDuration = 0.15f;
        button.colors = colors;
        
        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        Text text = textObj.AddComponent<Text>();
        text.text = buttonText;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 12;
        text.fontStyle = FontStyle.Bold;
        text.color = new Color(0.85f, 0.95f, 1f, 0.95f); // Bright cyan-white
        text.alignment = TextAnchor.MiddleCenter;
        
        // Add text shadow for depth
        Shadow textShadow = textObj.AddComponent<Shadow>();
        textShadow.effectColor = new Color(0, 0, 0, 0.5f);
        textShadow.effectDistance = new Vector2(1, -1);
        
        return button;
    }
    
    /// <summary>
    /// Create dropdown populated with all GeometricPatternGenerator shape types
    /// </summary>
    Dropdown CreatePatternDropdown(Transform parent, string name, Vector2 position)
    {
        GameObject dropdownObj = new GameObject(name);
        dropdownObj.transform.SetParent(parent, false);
        RectTransform dropdownRect = dropdownObj.AddComponent<RectTransform>();
        dropdownRect.anchorMin = new Vector2(0, 1);
        dropdownRect.anchorMax = new Vector2(0, 1);
        dropdownRect.pivot = new Vector2(0, 1);
        dropdownRect.anchoredPosition = position;
        dropdownRect.sizeDelta = new Vector2(290, 38);
        
        Image dropdownBg = dropdownObj.AddComponent<Image>();
        dropdownBg.color = new Color(0.08f, 0.12f, 0.22f, 0.7f);
        dropdownBg.raycastTarget = true;
        
        Outline dropdownOutline = dropdownObj.AddComponent<Outline>();
        dropdownOutline.effectColor = new Color(0.3f, 0.5f, 0.8f, 0.4f);
        dropdownOutline.effectDistance = new Vector2(1, -1);
        
        Dropdown dropdown = dropdownObj.AddComponent<Dropdown>();
        dropdown.targetGraphic = dropdownBg;
        
        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(10, 2);
        labelRect.offsetMax = new Vector2(-30, -2);
        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = "StarBurst";
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 13;
        labelText.fontStyle = FontStyle.Bold;
        labelText.color = new Color(0.85f, 0.95f, 1f, 0.95f);
        labelText.alignment = TextAnchor.MiddleLeft;
        dropdown.captionText = labelText;
        
        // Arrow
        GameObject arrowObj = new GameObject("Arrow");
        arrowObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform arrowRect = arrowObj.AddComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(1, 0);
        arrowRect.anchorMax = new Vector2(1, 1);
        arrowRect.offsetMin = new Vector2(-25, 0);
        arrowRect.offsetMax = new Vector2(-5, 0);
        Text arrowText = arrowObj.AddComponent<Text>();
        arrowText.text = "▼";
        arrowText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        arrowText.fontSize = 12;
        arrowText.color = new Color(0.6f, 0.8f, 1f, 0.8f);
        arrowText.alignment = TextAnchor.MiddleCenter;
        
        // Template - POSITIONED TO THE RIGHT
        GameObject templateObj = new GameObject("Template");
        templateObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform templateRect = templateObj.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(1, 0.5f); // Anchor to right middle of dropdown
        templateRect.anchorMax = new Vector2(1, 0.5f);
        templateRect.pivot = new Vector2(0, 0.5f); // Pivot on left middle so it extends right
        templateRect.anchoredPosition = new Vector2(10, 0); // 10 pixels to the right
        templateRect.sizeDelta = new Vector2(250, 300); // Fixed width, extra tall for many options
        
        Image templateBg = templateObj.AddComponent<Image>();
        templateBg.color = new Color(0.05f, 0.08f, 0.15f, 0.95f);
        templateBg.raycastTarget = true;
        
        // Add Canvas component to break out of parent's clipping
        Canvas templateCanvas = templateObj.AddComponent<Canvas>();
        templateCanvas.overrideSorting = true;
        templateCanvas.sortingOrder = 1000; // Render on top of everything
        
        // Add CanvasGroup to ensure it renders properly
        CanvasGroup templateGroup = templateObj.AddComponent<CanvasGroup>();
        templateGroup.blocksRaycasts = true;
        
        templateObj.AddComponent<GraphicRaycaster>();
        
        Outline templateOutline = templateObj.AddComponent<Outline>();
        templateOutline.effectColor = new Color(0.3f, 0.5f, 0.8f, 0.5f);
        templateOutline.effectDistance = new Vector2(2, -2);
        
        ScrollRect scrollRect = templateObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 10;
        
        // Viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(templateObj.transform, false);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        
        Image viewportMask = viewportObj.AddComponent<Image>();
        viewportMask.color = Color.white;
        Mask mask = viewportObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        
        scrollRect.viewport = viewportRect;
        
        // Content
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 200);
        
        scrollRect.content = contentRect;
        
        // Item
        GameObject itemObj = new GameObject("Item");
        itemObj.transform.SetParent(contentObj.transform, false);
        RectTransform itemRect = itemObj.AddComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 1);
        itemRect.anchorMax = new Vector2(1, 1);
        itemRect.pivot = new Vector2(0.5f, 1);
        itemRect.anchoredPosition = Vector2.zero;
        itemRect.sizeDelta = new Vector2(0, 30);
        
        Toggle itemToggle = itemObj.AddComponent<Toggle>();
        itemToggle.isOn = false;
        itemToggle.interactable = true;
        
        Image itemBg = itemObj.AddComponent<Image>();
        itemBg.color = new Color(0.1f, 0.15f, 0.25f, 1f);
        itemBg.raycastTarget = true;
        itemToggle.targetGraphic = itemBg;
        
        ColorBlock toggleColors = itemToggle.colors;
        toggleColors.normalColor = new Color(1f, 1f, 1f, 1f);
        toggleColors.highlightedColor = new Color(0.8f, 0.95f, 1f, 1f);
        toggleColors.pressedColor = new Color(0.6f, 0.8f, 1f, 1f);
        toggleColors.selectedColor = new Color(0.4f, 0.7f, 1f, 1f);
        itemToggle.colors = toggleColors;
        
        // Item Label
        GameObject itemLabelObj = new GameObject("Item Label");
        itemLabelObj.transform.SetParent(itemObj.transform, false);
        RectTransform itemLabelRect = itemLabelObj.AddComponent<RectTransform>();
        itemLabelRect.anchorMin = Vector2.zero;
        itemLabelRect.anchorMax = Vector2.one;
        itemLabelRect.offsetMin = new Vector2(10, 1);
        itemLabelRect.offsetMax = new Vector2(-10, -1);
        Text itemLabelText = itemLabelObj.AddComponent<Text>();
        itemLabelText.text = "Option";
        itemLabelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        itemLabelText.fontSize = 12;
        itemLabelText.color = new Color(0.85f, 0.95f, 1f, 0.95f);
        itemLabelText.alignment = TextAnchor.MiddleLeft;
        dropdown.itemText = itemLabelText;
        
        dropdown.template = templateRect;
        templateObj.SetActive(false);
        
        // Populate with ALL shape types from GeometricPatternGenerator
        dropdown.ClearOptions();
        List<string> shapeNames = new List<string>();
        foreach (GeometricPatternGenerator.ShapeType shape in System.Enum.GetValues(typeof(GeometricPatternGenerator.ShapeType)))
        {
            shapeNames.Add(shape.ToString());
        }
        dropdown.AddOptions(shapeNames);
        
        return dropdown;
    }
    
    /// <summary>
    /// Connect pattern generator buttons to PatternSpawner
    /// </summary>
    void ConnectPatternGenerator()
    {
        // Find or create PatternSpawner
        PatternSpawner spawner = FindFirstObjectByType<PatternSpawner>();
        if (spawner == null)
        {
            GameObject spawnerObj = new GameObject("PatternSpawner");
            spawner = spawnerObj.AddComponent<PatternSpawner>();
            Debug.Log("✓ Created PatternSpawner in scene");
        }
        
        // Connect Generate Button
        if (generatePatternButton != null && patternDropdown != null)
        {
            generatePatternButton.onClick.RemoveAllListeners();
            generatePatternButton.onClick.AddListener(() => {
                // Get selected shape from dropdown
                string selectedShape = patternDropdown.options[patternDropdown.value].text;
                GeometricPatternGenerator.ShapeType shapeType = (GeometricPatternGenerator.ShapeType)System.Enum.Parse(
                    typeof(GeometricPatternGenerator.ShapeType), selectedShape);
                
                // Spawn pattern
                GameObject newPattern = spawner.SpawnPattern(shapeType);
                Debug.Log($"✓ Generated {selectedShape} pattern at runtime!");
            });
        }
        
        // Connect Clear Button
        if (clearPatternsButton != null)
        {
            clearPatternsButton.onClick.RemoveAllListeners();
            clearPatternsButton.onClick.AddListener(() => {
                spawner.ClearAllPatterns();
                Debug.Log("✓ Cleared all generated patterns");
            });
        }
        
        Debug.Log("✓ Pattern generator UI connected!");
    }
    
    /// <summary>
    /// Create a modern toggle (checkbox) control
    /// </summary>
    Toggle CreateModernToggle(Transform parent, string name, Vector2 position, string labelText)
    {
        // Container
        GameObject toggleObj = new GameObject(name);
        toggleObj.transform.SetParent(parent, false);
        RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(0, 1);
        toggleRect.anchorMax = new Vector2(0, 1);
        toggleRect.pivot = new Vector2(0, 1);
        toggleRect.anchoredPosition = position;
        toggleRect.sizeDelta = new Vector2(290, 35);
        
        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(toggleObj.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = new Vector2(0, 1);
        bgRect.pivot = new Vector2(0, 0.5f);
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta = new Vector2(30, 0);
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.15f, 0.25f, 0.6f);
        
        Outline bgOutline = bgObj.AddComponent<Outline>();
        bgOutline.effectColor = new Color(0.3f, 0.5f, 0.8f, 0.4f);
        bgOutline.effectDistance = new Vector2(1, -1);
        
        // Checkmark
        GameObject checkmarkObj = new GameObject("Checkmark");
        checkmarkObj.transform.SetParent(bgObj.transform, false);
        RectTransform checkmarkRect = checkmarkObj.AddComponent<RectTransform>();
        checkmarkRect.anchorMin = Vector2.zero;
        checkmarkRect.anchorMax = Vector2.one;
        checkmarkRect.sizeDelta = Vector2.zero;
        
        Text checkmark = checkmarkObj.AddComponent<Text>();
        checkmark.text = "✓";
        checkmark.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        checkmark.fontSize = 22;
        checkmark.fontStyle = FontStyle.Bold;
        checkmark.color = new Color(0.3f, 0.8f, 1f, 1f);
        checkmark.alignment = TextAnchor.MiddleCenter;
        
        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(toggleObj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.pivot = new Vector2(0, 0.5f);
        labelRect.anchoredPosition = new Vector2(38, 0);
        labelRect.sizeDelta = new Vector2(-38, 0);
        
        Text label = labelObj.AddComponent<Text>();
        label.text = labelText;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 12;
        label.fontStyle = FontStyle.Bold;
        label.color = new Color(0.7f, 0.85f, 1f, 0.9f);
        label.alignment = TextAnchor.MiddleLeft;
        
        // Toggle component
        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.targetGraphic = bgImage;
        toggle.graphic = checkmark;
        toggle.isOn = false;
        
        ColorBlock colors = toggle.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.2f, 1f);
        colors.pressedColor = new Color(0.9f, 0.9f, 1f, 1f);
        colors.selectedColor = new Color(1.1f, 1.1f, 1.2f, 1f);
        toggle.colors = colors;
        
        return toggle;
    }
    
    /// <summary>
    /// Create a compact dropdown (smaller than the default modern dropdown)
    /// </summary>
    Dropdown CreateCompactDropdown(Transform parent, string name, Vector2 position, string[] options)
    {
        GameObject dropdownObj = new GameObject(name);
        dropdownObj.transform.SetParent(parent, false);
        RectTransform dropdownRect = dropdownObj.AddComponent<RectTransform>();
        dropdownRect.anchorMin = new Vector2(0, 1);
        dropdownRect.anchorMax = new Vector2(0, 1);
        dropdownRect.pivot = new Vector2(0, 1);
        dropdownRect.anchoredPosition = position;
        dropdownRect.sizeDelta = new Vector2(290, 32);
        
        Image dropdownBg = dropdownObj.AddComponent<Image>();
        dropdownBg.color = new Color(0.08f, 0.12f, 0.22f, 0.7f);
        dropdownBg.raycastTarget = true;
        
        Outline dropdownOutline = dropdownObj.AddComponent<Outline>();
        dropdownOutline.effectColor = new Color(0.3f, 0.5f, 0.8f, 0.4f);
        dropdownOutline.effectDistance = new Vector2(1, -1);
        
        Dropdown dropdown = dropdownObj.AddComponent<Dropdown>();
        dropdown.targetGraphic = dropdownBg;
        
        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(8, 2);
        labelRect.offsetMax = new Vector2(-25, -2);
        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = options[0];
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 11;
        labelText.fontStyle = FontStyle.Bold;
        labelText.color = new Color(0.85f, 0.95f, 1f, 0.95f);
        labelText.alignment = TextAnchor.MiddleLeft;
        dropdown.captionText = labelText;
        
        // Arrow
        GameObject arrowObj = new GameObject("Arrow");
        arrowObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform arrowRect = arrowObj.AddComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(1, 0);
        arrowRect.anchorMax = new Vector2(1, 1);
        arrowRect.offsetMin = new Vector2(-20, 0);
        arrowRect.offsetMax = new Vector2(-4, 0);
        Text arrowText = arrowObj.AddComponent<Text>();
        arrowText.text = "▼";
        arrowText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        arrowText.fontSize = 10;
        arrowText.color = new Color(0.6f, 0.8f, 1f, 0.8f);
        arrowText.alignment = TextAnchor.MiddleCenter;
        
        // Template (same as modern dropdown but positioned to the right)
        GameObject templateObj = new GameObject("Template");
        templateObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform templateRect = templateObj.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(1, 0.5f);
        templateRect.anchorMax = new Vector2(1, 0.5f);
        templateRect.pivot = new Vector2(0, 0.5f);
        templateRect.anchoredPosition = new Vector2(10, 0);
        templateRect.sizeDelta = new Vector2(200, 150);
        
        Image templateBg = templateObj.AddComponent<Image>();
        templateBg.color = new Color(0.05f, 0.08f, 0.15f, 0.95f);
        templateBg.raycastTarget = true;
        
        Canvas templateCanvas = templateObj.AddComponent<Canvas>();
        templateCanvas.overrideSorting = true;
        templateCanvas.sortingOrder = 1000;
        
        CanvasGroup templateGroup = templateObj.AddComponent<CanvasGroup>();
        templateGroup.blocksRaycasts = true;
        
        templateObj.AddComponent<GraphicRaycaster>();
        
        Outline templateOutline = templateObj.AddComponent<Outline>();
        templateOutline.effectColor = new Color(0.3f, 0.5f, 0.8f, 0.5f);
        templateOutline.effectDistance = new Vector2(2, -2);
        
        ScrollRect scrollRect = templateObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 10;
        
        // Viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(templateObj.transform, false);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        
        Image viewportMask = viewportObj.AddComponent<Image>();
        viewportMask.color = Color.white;
        Mask mask = viewportObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        
        scrollRect.viewport = viewportRect;
        
        // Content
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 100);
        
        scrollRect.content = contentRect;
        
        // Item
        GameObject itemObj = new GameObject("Item");
        itemObj.transform.SetParent(contentObj.transform, false);
        RectTransform itemRect = itemObj.AddComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 1);
        itemRect.anchorMax = new Vector2(1, 1);
        itemRect.pivot = new Vector2(0.5f, 1);
        itemRect.anchoredPosition = Vector2.zero;
        itemRect.sizeDelta = new Vector2(0, 25);
        
        Toggle itemToggle = itemObj.AddComponent<Toggle>();
        itemToggle.isOn = false;
        itemToggle.interactable = true;
        
        Image itemBg = itemObj.AddComponent<Image>();
        itemBg.color = new Color(0.1f, 0.15f, 0.25f, 1f);
        itemBg.raycastTarget = true;
        itemToggle.targetGraphic = itemBg;
        
        ColorBlock toggleColors = itemToggle.colors;
        toggleColors.normalColor = new Color(1f, 1f, 1f, 1f);
        toggleColors.highlightedColor = new Color(0.8f, 0.95f, 1f, 1f);
        toggleColors.pressedColor = new Color(0.6f, 0.8f, 1f, 1f);
        toggleColors.selectedColor = new Color(0.4f, 0.7f, 1f, 1f);
        itemToggle.colors = toggleColors;
        
        // Item Label
        GameObject itemLabelObj = new GameObject("Item Label");
        itemLabelObj.transform.SetParent(itemObj.transform, false);
        RectTransform itemLabelRect = itemLabelObj.AddComponent<RectTransform>();
        itemLabelRect.anchorMin = Vector2.zero;
        itemLabelRect.anchorMax = Vector2.one;
        itemLabelRect.offsetMin = new Vector2(8, 1);
        itemLabelRect.offsetMax = new Vector2(-8, -1);
        Text itemLabelText = itemLabelObj.AddComponent<Text>();
        itemLabelText.text = "Option";
        itemLabelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        itemLabelText.fontSize = 11;
        itemLabelText.color = new Color(0.85f, 0.95f, 1f, 0.95f);
        itemLabelText.alignment = TextAnchor.MiddleLeft;
        dropdown.itemText = itemLabelText;
        
        dropdown.template = templateRect;
        templateObj.SetActive(false);
        
        // Populate options
        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>(options));
        
        return dropdown;
    }
    
    /// <summary>
    /// Connect multi-agent UI to MultiAgentManager
    /// </summary>
    void ConnectMultiAgentSystem()
    {
        // Find or create MultiAgentManager
        multiAgentManager = FindFirstObjectByType<MultiAgentManager>();
        if (multiAgentManager == null)
        {
            GameObject managerObj = new GameObject("MultiAgentManager");
            multiAgentManager = managerObj.AddComponent<MultiAgentManager>();
            Debug.Log("✓ Created MultiAgentManager");
        }
        
        // Find or create SharedPathState
        sharedPathState = FindFirstObjectByType<SharedPathState>();
        if (sharedPathState == null)
        {
            GameObject stateObj = new GameObject("SharedPathState");
            sharedPathState = stateObj.AddComponent<SharedPathState>();
            Debug.Log("✓ Created SharedPathState");
        }
        
        // Link manager to shared state
        multiAgentManager.sharedState = sharedPathState;
        
        // Connect Multi-Agent Toggle
        if (multiAgentToggle != null)
        {
            multiAgentToggle.onValueChanged.RemoveAllListeners();
            multiAgentToggle.onValueChanged.AddListener((isOn) => {
                OnMultiAgentModeToggled(isOn);
            });
        }
        
        // Connect Agent Count Slider
        if (agentCountSlider != null)
        {
            agentCountSlider.onValueChanged.RemoveAllListeners();
            agentCountSlider.onValueChanged.AddListener((value) => {
                int count = (int)value;
                if (agentCountText != null)
                {
                    agentCountText.text = $"Agent Count: {count}";
                }
                if (multiAgentManager != null && multiAgentManager.isMultiAgentMode)
                {
                    multiAgentManager.SetAgentCount(count);
                }
            });
        }
        
        // Connect Color Mode Dropdown
        if (agentColorModeDropdown != null)
        {
            agentColorModeDropdown.onValueChanged.RemoveAllListeners();
            agentColorModeDropdown.onValueChanged.AddListener((index) => {
                if (multiAgentManager != null && multiAgentManager.isMultiAgentMode)
                {
                    multiAgentManager.SetColorMode((MultiAgentManager.AgentColorMode)index);
                    Debug.Log($"Agent color mode: {(MultiAgentManager.AgentColorMode)index}");
                }
            });
        }
        
        // Connect Spawn Mode Dropdown
        if (agentSpawnModeDropdown != null)
        {
            agentSpawnModeDropdown.onValueChanged.RemoveAllListeners();
            agentSpawnModeDropdown.onValueChanged.AddListener((index) => {
                if (multiAgentManager != null && multiAgentManager.isMultiAgentMode)
                {
                    multiAgentManager.SetSpawnMode((MultiAgentManager.AgentSpawnMode)index);
                    Debug.Log($"Agent spawn mode: {(MultiAgentManager.AgentSpawnMode)index}");
                }
            });
        }
        
        // When multi-agent mode is enabled, connect sliders to SharedPathState
        // This will be handled in OnMultiAgentModeToggled
        
        Debug.Log("✓ Multi-agent system UI connected!");
    }
    
    /// <summary>
    /// Handle multi-agent mode toggle
    /// </summary>
    void OnMultiAgentModeToggled(bool isEnabled)
    {
        Debug.Log($"Multi-Agent Mode: {(isEnabled ? "ENABLED" : "DISABLED")}");
        
        // Show/hide multi-agent controls
        GameObject agentCountContainer = GameObject.Find("AgentCountContainer");
        GameObject colorModeContainer = GameObject.Find("ColorModeContainer");
        GameObject spawnModeContainer = GameObject.Find("SpawnModeContainer");
        
        if (agentCountContainer != null) agentCountContainer.SetActive(isEnabled);
        if (colorModeContainer != null) colorModeContainer.SetActive(isEnabled);
        if (spawnModeContainer != null) spawnModeContainer.SetActive(isEnabled);
        
        if (isEnabled)
        {
            // Enable multi-agent mode
            if (multiAgentManager != null)
            {
                // Setup shared path state with current pattern's path
                PatternSpawner spawner = FindFirstObjectByType<PatternSpawner>();
                if (spawner != null && spawner.activeRoller != null && spawner.activeRoller.pathPoints != null)
                {
                    sharedPathState.pathPoints = spawner.activeRoller.pathPoints;
                    
                    // Copy current settings from active rotor to shared state
                    sharedPathState.masterSpeed = spawner.activeRoller.speed;
                    sharedPathState.masterRotationSpeed = spawner.activeRoller.rotationSpeed;
                    sharedPathState.masterPenDistance = spawner.activeRoller.penDistance;
                    sharedPathState.masterCycles = spawner.activeRoller.cycles;
                    sharedPathState.masterLineWidth = spawner.activeRoller.lineWidth;
                    sharedPathState.masterLineBrightness = spawner.activeRoller.lineBrightness;
                    sharedPathState.masterLineColor = spawner.activeRoller.currentLineColor;
                    
                    Debug.Log("✓ Copied active rotor settings to SharedPathState");
                }
                
                // Set agent count from slider
                if (agentCountSlider != null)
                {
                    multiAgentManager.agentCount = (int)agentCountSlider.value;
                }
                
                // Set color mode from dropdown
                if (agentColorModeDropdown != null)
                {
                    multiAgentManager.colorMode = (MultiAgentManager.AgentColorMode)agentColorModeDropdown.value;
                }
                
                // Set spawn mode from dropdown
                if (agentSpawnModeDropdown != null)
                {
                    multiAgentManager.spawnMode = (MultiAgentManager.AgentSpawnMode)agentSpawnModeDropdown.value;
                }
                
                multiAgentManager.EnableMultiAgentMode();
            }
            
            // Redirect sliders to control SharedPathState instead of activeRoller
            ConnectSlidersToSharedState();
        }
        else
        {
            // Disable multi-agent mode
            if (multiAgentManager != null)
            {
                multiAgentManager.DisableMultiAgentMode();
            }
            
            // Redirect sliders back to activeRoller
            ConnectSlidersToActiveRotor();
        }
    }
    
    /// <summary>
    /// Connect UI sliders to SharedPathState (for multi-agent mode)
    /// </summary>
    void ConnectSlidersToSharedState()
    {
        if (sharedPathState == null) return;
        
        // Speed Slider
        if (speedSlider != null)
        {
            speedSlider.onValueChanged.RemoveAllListeners();
            speedSlider.value = sharedPathState.masterSpeed;
            speedSlider.onValueChanged.AddListener((value) => {
                sharedPathState.masterSpeed = value;
                if (speedText != null)
                {
                    speedText.text = "Travel Speed: " + value.ToString("F1") + " (ALL)";
                }
            });
            // Update text to show it's controlling all agents
            if (speedText != null)
            {
                speedText.text = "Travel Speed: " + sharedPathState.masterSpeed.ToString("F1") + " (ALL)";
            }
        }
        
        // Rotation Speed Slider
        if (rotationSpeedSlider != null)
        {
            rotationSpeedSlider.onValueChanged.RemoveAllListeners();
            rotationSpeedSlider.value = sharedPathState.masterRotationSpeed;
            rotationSpeedSlider.onValueChanged.AddListener((value) => {
                sharedPathState.masterRotationSpeed = value;
            });
        }
        
        // Pen Distance Slider
        if (penDistanceSlider != null)
        {
            penDistanceSlider.onValueChanged.RemoveAllListeners();
            penDistanceSlider.value = sharedPathState.masterPenDistance;
            penDistanceSlider.onValueChanged.AddListener((value) => {
                sharedPathState.masterPenDistance = value;
            });
        }
        
        // Cycles Slider
        if (cyclesSlider != null)
        {
            cyclesSlider.onValueChanged.RemoveAllListeners();
            cyclesSlider.value = sharedPathState.masterCycles;
            cyclesSlider.onValueChanged.AddListener((value) => {
                sharedPathState.masterCycles = (int)value;
            });
        }
        
        // Line Width Slider
        if (lineWidthSlider != null)
        {
            lineWidthSlider.onValueChanged.RemoveAllListeners();
            lineWidthSlider.value = sharedPathState.masterLineWidth;
            lineWidthSlider.onValueChanged.AddListener((value) => {
                sharedPathState.masterLineWidth = value;
                // Update all agents
                if (multiAgentManager != null)
                {
                    foreach (PathAgent agent in multiAgentManager.agents)
                    {
                        if (agent != null) agent.UpdateLineWidth(value);
                    }
                }
            });
        }
        
        // Line Brightness Slider
        if (lineBrightnessSlider != null)
        {
            lineBrightnessSlider.onValueChanged.RemoveAllListeners();
            lineBrightnessSlider.value = sharedPathState.masterLineBrightness;
            lineBrightnessSlider.onValueChanged.AddListener((value) => {
                sharedPathState.masterLineBrightness = value;
            });
        }
        
        Debug.Log("✓ Sliders now controlling SharedPathState (Multi-Agent Mode)");
    }
}
