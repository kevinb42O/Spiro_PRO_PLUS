using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages skybox    switching with 5 assignable materials
/// Assign your custom skybox materials in the Inspector
/// </summary>
public class SkyboxManager : MonoBehaviour
{
    [Header("Skybox Materials")]
    [Tooltip("Assign 5 skybox materials in the Inspector")]
    public Material skybox1;
    public Material skybox2;
    public Material skybox3;
    public Material skybox4;
    public Material skybox5;
    
    [Header("Skybox Names (Optional)")]
    [Tooltip("Custom names for dropdown. If empty, uses 'Skybox 1', 'Skybox 2', etc.")]
    public string skybox1Name = "Starfield";
    public string skybox2Name = "Nebula";
    public string skybox3Name = "Space";
    public string skybox4Name = "Cosmic";
    public string skybox5Name = "Galaxy";
    
    private Material[] skyboxMaterials;
    private string[] skyboxNames;
    
    void Awake()
    {
        // Initialize arrays
        InitializeArrays();
        
        // Debug which skyboxes are assigned
        Debug.Log("=== SkyboxManager Initialized ===");
        for (int i = 0; i < skyboxMaterials.Length; i++)
        {
            Debug.Log($"Skybox {i + 1} ({skyboxNames[i]}): {(skyboxMaterials[i] != null ? "ASSIGNED" : "NOT ASSIGNED")}");
        }
        
        // Set default skybox if none is set
        if (RenderSettings.skybox == null && skybox1 != null)
        {
            Debug.Log("Setting initial skybox to slot 1");
            SetSkybox(0);
        }
        else if (RenderSettings.skybox != null)
        {
            Debug.Log($"Current skybox already set: {RenderSettings.skybox.name}");
        }
        else
        {
            Debug.LogWarning("No skybox materials assigned! Please assign materials in the Inspector.");
        }
    }
    
    void InitializeArrays()
    {
        skyboxMaterials = new Material[] { skybox1, skybox2, skybox3, skybox4, skybox5 };
        skyboxNames = new string[] { skybox1Name, skybox2Name, skybox3Name, skybox4Name, skybox5Name };
    }
    
    public void SetSkybox(int index)
    {
        // Ensure arrays are initialized
        if (skyboxMaterials == null)
        {
            InitializeArrays();
        }
        
        Debug.Log($"SetSkybox called with index: {index}");
        
        if (index >= 0 && index < skyboxMaterials.Length)
        {
            if (skyboxMaterials[index] != null)
            {
                RenderSettings.skybox = skyboxMaterials[index];
                DynamicGI.UpdateEnvironment(); // Update lighting
                Debug.Log($"✓ Skybox changed to: {GetSkyboxName(index)} ({skyboxMaterials[index].name})");
            }
            else
            {
                Debug.LogError($"✗ Skybox at index {index} ({GetSkyboxName(index)}) is NULL! Please assign a material in the Inspector.");
            }
        }
        else
        {
            Debug.LogError($"✗ Invalid skybox index: {index}. Must be 0-4.");
        }
    }
    
    public string GetSkyboxName(int index)
    {
        // Ensure arrays are initialized
        if (skyboxNames == null)
        {
            skyboxNames = new string[] { skybox1Name, skybox2Name, skybox3Name, skybox4Name, skybox5Name };
        }
        
        if (index >= 0 && index < skyboxNames.Length && !string.IsNullOrEmpty(skyboxNames[index]))
        {
            return skyboxNames[index];
        }
        return $"Skybox {index + 1}";
    }
    
    public string[] GetAllSkyboxNames()
    {
        // Ensure arrays are initialized
        if (skyboxNames == null)
        {
            skyboxNames = new string[] { skybox1Name, skybox2Name, skybox3Name, skybox4Name, skybox5Name };
        }
        
        string[] names = new string[5];
        for (int i = 0; i < 5; i++)
        {
            names[i] = GetSkyboxName(i);
        }
        return names;
    }
    
    public int GetCurrentSkyboxIndex()
    {
        // Ensure arrays are initialized
        if (skyboxMaterials == null)
        {
            skyboxMaterials = new Material[] { skybox1, skybox2, skybox3, skybox4, skybox5 };
        }
        
        Material currentSkybox = RenderSettings.skybox;
        if (currentSkybox == null) return -1;
        
        for (int i = 0; i < skyboxMaterials.Length; i++)
        {
            if (skyboxMaterials[i] == currentSkybox)
            {
                return i;
            }
        }
        return -1;
    }
}
