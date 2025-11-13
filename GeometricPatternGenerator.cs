using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates beautiful closed geometric patterns where the last point connects to the first
/// </summary>
public class GeometricPatternGenerator : MonoBehaviour
{
    [Header("Pattern Settings")]
    [SerializeField] private ShapeType shapeType = ShapeType.StarBurst;
    [SerializeField] private int numberOfPoints = 64;
    [SerializeField] private float radius = 5f;
    [SerializeField] private float innerRadius = 2.5f; // For stars and complex shapes
    
    [Header("Modulation")]
    [SerializeField] private int frequencyA = 3; // Outer frequency
    [SerializeField] private int frequencyB = 7; // Inner frequency
    [SerializeField] private float phase = 0f;
    [SerializeField] private float amplitude = 1f;
    
    [Header("3D Settings")]
    [SerializeField] private bool generate3D = false;
    [SerializeField] private float heightAmplitude = 2f;
    [SerializeField] private int heightFrequency = 3;
    
    [Header("Text Settings")]
    [SerializeField] private string textToGenerate = "HELLO";
    [SerializeField] private float letterSpacing = 3f;
    [SerializeField] private float letterScale = 2f;
    [SerializeField] private int pointsPerLetter = 12;
    
    [Header("Visualization")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private bool drawConnections = true;
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0.8f, 1f);
    [SerializeField] private float gizmoSize = 0.15f;

    [Header("Parent Object")]
    [SerializeField] private bool createParentObject = true;
    [SerializeField] private string parentName = "GeometricShape";
    [SerializeField] private bool keepPreviousGenerations = true; // NEW: Keep old patterns

    [Header("Performance Optimization")]
    [Tooltip("Use object pooling for point GameObjects (recommended for frequent regeneration)")]
    [SerializeField] private bool useObjectPooling = false;
    [Tooltip("Batch point creation (faster for large patterns)")]
    [SerializeField] private bool batchCreation = true;

    private List<GameObject> generatedObjects = new List<GameObject>();
    private Transform parentTransform;
    private static int generationCounter = 0; // Track how many patterns created
    
    // Object pool for performance
    private Queue<GameObject> objectPool = new Queue<GameObject>();
    private const int POOL_INITIAL_SIZE = 1000;

    public enum ShapeType
    {
        StarBurst,          // Beautiful star pattern
        RoseCurve,          // Rose/flower mathematical curve
        Hypotrochoid,       // Like a spirograph!
        Epitrochoid,        // Inverse spirograph
        Lissajous,          // Harmonic oscillation pattern
        SuperEllipse,       // Rounded star/square
        HeartShape,         // Heart curve
        ButterflyShape,     // Butterfly curve
        LemniscateInfinity, // Infinity symbol (figure-8)
        Cardioid,           // Heart-like curve
        Deltoid,            // Three-cusped shape
        ReuleauxTriangle,   // Constant width triangle
        Astroid,            // Four-cusped star
        MultiFoil,          // Multi-petal flower
        DNADoubleHelix,     // 3D DNA double helix structure
        TextWord,           // Type any word!
        MonaLisa,           // Ultra-detailed Mona Lisa portrait!
        MetaLogo,           // Meta (Facebook) infinity logo
        SmileyFace,         // Classic happy smiley face :)
        McDonaldsLogo,      // McDonald's Golden Arches + text - I'm lovin' it!
        WuTangLogo,         // Wu-Tang Clan iconic W logo - Protect Ya Neck!
        MiddleFinger,       // The universal gesture 🖕
        CannabisLeaf,       // Perfect cannabis leaf with stem and all details 🌿
        CountIQ             // COUNT IQ in the coolest font imaginable! 🔥
    }

    private void Start()
    {
        GeneratePattern();
    }

    [ContextMenu("Generate Pattern")]
    public void GeneratePattern()
    {
        // Only clear if we're NOT keeping previous generations
        if (!keepPreviousGenerations)
        {
            ClearPattern();
        }
        else
        {
            // Clear only the current working list, not the previous objects
            generatedObjects.Clear();
        }
        
        if (createParentObject)
        {
            // Create unique parent name with counter
            generationCounter++;
            string uniqueName = $"{parentName}_{generationCounter}_{shapeType}";
            GameObject parent = new GameObject(uniqueName);
            parent.transform.position = transform.position;
            parentTransform = parent.transform;
        }
        else
        {
            parentTransform = transform;
        }

        switch (shapeType)
        {
            case ShapeType.StarBurst:
                GenerateStarBurst();
                break;
            case ShapeType.RoseCurve:
                GenerateRoseCurve();
                break;
            case ShapeType.Hypotrochoid:
                GenerateHypotrochoid();
                break;
            case ShapeType.Epitrochoid:
                GenerateEpitrochoid();
                break;
            case ShapeType.Lissajous:
                GenerateLissajous();
                break;
            case ShapeType.SuperEllipse:
                GenerateSuperEllipse();
                break;
            case ShapeType.HeartShape:
                GenerateHeart();
                break;
            case ShapeType.ButterflyShape:
                GenerateButterfly();
                break;
            case ShapeType.LemniscateInfinity:
                GenerateLemniscate();
                break;
            case ShapeType.Cardioid:
                GenerateCardioid();
                break;
            case ShapeType.Deltoid:
                GenerateDeltoid();
                break;
            case ShapeType.ReuleauxTriangle:
                GenerateReuleauxTriangle();
                break;
            case ShapeType.Astroid:
                GenerateAstroid();
                break;
            case ShapeType.MultiFoil:
                GenerateMultiFoil();
                break;
            case ShapeType.DNADoubleHelix:
                GenerateDNAHelix();
                break;
            case ShapeType.TextWord:
                GenerateText();
                break;
            case ShapeType.MonaLisa:
                GenerateMonaLisa();
                break;
            case ShapeType.MetaLogo:
                GenerateMetaLogo();
                break;
            case ShapeType.SmileyFace:
                GenerateSmileyFace();
                break;
            case ShapeType.McDonaldsLogo:
                GenerateMcDonaldsLogo();
                break;
            case ShapeType.WuTangLogo:
                GenerateWuTangLogo();
                break;
            case ShapeType.MiddleFinger:
                GenerateMiddleFinger();
                break;
            case ShapeType.CannabisLeaf:
                GenerateCannabisLeaf();
                break;
            case ShapeType.CountIQ:
                GenerateCountIQ();
                break;
        }
    }

    [ContextMenu("Clear Pattern")]
    public void ClearPattern()
    {
        foreach (GameObject obj in generatedObjects)
        {
            if (obj != null)
            {
                ReturnToPool(obj);
            }
        }
        generatedObjects.Clear();

        if (parentTransform != null && parentTransform != transform)
        {
            DestroyImmediate(parentTransform.gameObject);
            parentTransform = null;
        }
    }

    [ContextMenu("Clear All Patterns")]
    public void ClearAllPatterns()
    {
        // Find all generated parent objects and destroy them
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains(parentName))
            {
                DestroyImmediate(obj);
            }
        }
        
        generatedObjects.Clear();
        parentTransform = null;
        generationCounter = 0;
        Debug.Log("All patterns cleared!");
    }

    // === CLOSED SHAPE GENERATORS - All patterns loop back to start ===

    private void GenerateStarBurst()
    {
        // Alternating inner/outer radius creates a star
        for (int i = 0; i < numberOfPoints; i++)
        {
            float angle = (i / (float)numberOfPoints) * 2f * Mathf.PI + phase;
            float r = (i % 2 == 0) ? radius : innerRadius;
            
            Vector3 position = new Vector3(
                r * Mathf.Cos(angle),
                r * Mathf.Sin(angle),
                generate3D ? heightAmplitude * Mathf.Sin(heightFrequency * angle) : 0f
            );
            CreatePoint(position, i);
        }
    }

    private void GenerateRoseCurve()
    {
        // Rose curve: r = cos(k*θ) or r = sin(k*θ)
        // Creates flower-like petals
        float k = frequencyA; // Number of petals (if odd) or 2*k petals (if even)
        
        for (int i = 0; i < numberOfPoints; i++)
        {
            float angle = (i / (float)numberOfPoints) * 2f * Mathf.PI + phase;
            float r = radius * Mathf.Abs(Mathf.Cos(k * angle)) * amplitude;
            
            Vector3 position = new Vector3(
                r * Mathf.Cos(angle),
                r * Mathf.Sin(angle),
                generate3D ? heightAmplitude * Mathf.Sin(heightFrequency * angle) : 0f
            );
            CreatePoint(position, i);
        }
    }

    private void GenerateHypotrochoid()
    {
        // Like a spirograph - inner circle rolling inside outer circle
        float R = radius;              // Fixed circle radius
        float r = innerRadius;         // Rolling circle radius
        float d = amplitude * innerRadius; // Distance from center of rolling circle
        
        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = (i / (float)numberOfPoints) * 2f * Mathf.PI * frequencyA + phase;
            float x = (R - r) * Mathf.Cos(t) + d * Mathf.Cos((R - r) / r * t);
            float y = (R - r) * Mathf.Sin(t) - d * Mathf.Sin((R - r) / r * t);
            
            Vector3 position = new Vector3(
                x, y,
                generate3D ? heightAmplitude * Mathf.Sin(heightFrequency * t) : 0f
            );
            CreatePoint(position, i);
        }
    }

    private void GenerateEpitrochoid()
    {
        // Inner circle rolling around the outside of outer circle
        float R = radius;              
        float r = innerRadius;         
        float d = amplitude * innerRadius;
        
        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = (i / (float)numberOfPoints) * 2f * Mathf.PI * frequencyA + phase;
            float x = (R + r) * Mathf.Cos(t) - d * Mathf.Cos((R + r) / r * t);
            float y = (R + r) * Mathf.Sin(t) - d * Mathf.Sin((R + r) / r * t);
            
            Vector3 position = new Vector3(
                x, y,
                generate3D ? heightAmplitude * Mathf.Sin(heightFrequency * t) : 0f
            );
            CreatePoint(position, i);
        }
    }

    private void GenerateLissajous()
    {
        // Harmonic oscillation - creates figure-8 or complex loops
        float A = radius;
        float B = radius;
        float a = frequencyA;
        float b = frequencyB;
        
        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = (i / (float)numberOfPoints) * 2f * Mathf.PI + phase;
            float x = A * Mathf.Sin(a * t + phase);
            float y = B * Mathf.Sin(b * t);
            
            Vector3 position = new Vector3(
                x, y,
                generate3D ? heightAmplitude * Mathf.Sin(heightFrequency * t) : 0f
            );
            CreatePoint(position, i);
        }
    }

    private void GenerateSuperEllipse()
    {
        // Rounded square/star shape
        float n = amplitude * 2f; // Exponent controls roundness
        
        for (int i = 0; i < numberOfPoints; i++)
        {
            float angle = (i / (float)numberOfPoints) * 2f * Mathf.PI + phase;
            float cosT = Mathf.Cos(angle);
            float sinT = Mathf.Sin(angle);
            
            float r = radius * Mathf.Pow(
                Mathf.Pow(Mathf.Abs(cosT), n) + Mathf.Pow(Mathf.Abs(sinT), n),
                -1f / n
            );
            
            Vector3 position = new Vector3(
                r * cosT,
                r * sinT,
                generate3D ? heightAmplitude * Mathf.Sin(heightFrequency * angle) : 0f
            );
            CreatePoint(position, i);
        }
    }

    private void GenerateHeart()
    {
        // Parametric heart curve
        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = (i / (float)numberOfPoints) * 2f * Mathf.PI + phase;
            float x = radius * 16f * Mathf.Pow(Mathf.Sin(t), 3f);
            float y = radius * (13f * Mathf.Cos(t) - 5f * Mathf.Cos(2f * t) - 
                      2f * Mathf.Cos(3f * t) - Mathf.Cos(4f * t));
            
            Vector3 position = new Vector3(
                x * 0.1f, y * 0.1f,
                generate3D ? heightAmplitude * Mathf.Sin(heightFrequency * t) : 0f
            );
            CreatePoint(position, i);
        }
    }

    private void GenerateButterfly()
    {
        // Butterfly curve
        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = (i / (float)numberOfPoints) * 12f * Mathf.PI + phase;
            float r = radius * (Mathf.Exp(Mathf.Cos(t)) - 2f * Mathf.Cos(4f * t) + 
                      Mathf.Pow(Mathf.Sin(t / 12f), 5f));
            
            Vector3 position = new Vector3(
                r * Mathf.Sin(t) * 0.3f,
                r * Mathf.Cos(t) * 0.3f,
                generate3D ? heightAmplitude * Mathf.Sin(heightFrequency * t) : 0f
            );
            CreatePoint(position, i);
        }
    }

    private void GenerateLemniscate()
    {
        // Figure-8 / Infinity symbol
        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = (i / (float)numberOfPoints) * 2f * Mathf.PI + phase;
            float denominator = 1f + Mathf.Sin(t) * Mathf.Sin(t);
            float x = radius * Mathf.Cos(t) / denominator;
            float y = radius * Mathf.Sin(t) * Mathf.Cos(t) / denominator;
            
            Vector3 position = new Vector3(
                x, y,
                generate3D ? heightAmplitude * Mathf.Sin(heightFrequency * t) : 0f
            );
            CreatePoint(position, i);
        }
    }

    private void GenerateCardioid()
    {
        // Heart-shaped curve (different from heart)
        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = (i / (float)numberOfPoints) * 2f * Mathf.PI + phase;
            float r = radius * (1f - Mathf.Cos(t));
            
            Vector3 position = new Vector3(
                r * Mathf.Cos(t),
                r * Mathf.Sin(t),
                generate3D ? heightAmplitude * Mathf.Sin(heightFrequency * t) : 0f
            );
            CreatePoint(position, i);
        }
    }

    private void GenerateDeltoid()
    {
        // Three-cusped hypocycloid
        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = (i / (float)numberOfPoints) * 2f * Mathf.PI + phase;
            float x = radius * (2f * Mathf.Cos(t) + Mathf.Cos(2f * t));
            float y = radius * (2f * Mathf.Sin(t) - Mathf.Sin(2f * t));
            
            Vector3 position = new Vector3(
                x * 0.33f, y * 0.33f,
                generate3D ? heightAmplitude * Mathf.Sin(heightFrequency * t) : 0f
            );
            CreatePoint(position, i);
        }
    }

    private void GenerateReuleauxTriangle()
    {
        // Rounded triangle with constant width
        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = (i / (float)numberOfPoints) * 2f * Mathf.PI + phase;
            int section = Mathf.FloorToInt((t / (2f * Mathf.PI)) * 3f) % 3;
            float sectionAngle = (t - section * 2f * Mathf.PI / 3f);
            
            float x = radius * Mathf.Cos(section * 2f * Mathf.PI / 3f + sectionAngle);
            float y = radius * Mathf.Sin(section * 2f * Mathf.PI / 3f + sectionAngle);
            
            Vector3 position = new Vector3(
                x, y,
                generate3D ? heightAmplitude * Mathf.Sin(heightFrequency * t) : 0f
            );
            CreatePoint(position, i);
        }
    }

    private void GenerateAstroid()
    {
        // Four-cusped star
        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = (i / (float)numberOfPoints) * 2f * Mathf.PI + phase;
            float x = radius * Mathf.Pow(Mathf.Cos(t), 3f);
            float y = radius * Mathf.Pow(Mathf.Sin(t), 3f);
            
            Vector3 position = new Vector3(
                x, y,
                generate3D ? heightAmplitude * Mathf.Sin(heightFrequency * t) : 0f
            );
            CreatePoint(position, i);
        }
    }

    private void GenerateMultiFoil()
    {
        // Multi-petal flower using multiple frequencies
        int petals = frequencyA;
        
        for (int i = 0; i < numberOfPoints; i++)
        {
            float angle = (i / (float)numberOfPoints) * 2f * Mathf.PI + phase;
            float r = radius * (1f + amplitude * 0.5f * Mathf.Cos(petals * angle));
            
            Vector3 position = new Vector3(
                r * Mathf.Cos(angle),
                r * Mathf.Sin(angle),
                generate3D ? heightAmplitude * Mathf.Sin(heightFrequency * angle) : 0f
            );
            CreatePoint(position, i);
        }
    }

    private void GenerateDNAHelix()
    {
        // DNA Double Helix - Two intertwined helices with base pairs
        // Creates both strands and connecting base pairs
        
        int pointsPerStrand = numberOfPoints / 2;
        float helixRadius = radius * 0.5f;
        float helixHeight = heightAmplitude * 2f;
        float twists = frequencyA; // Number of complete rotations
        float basePairFrequency = frequencyB; // How often to add base pairs
        
        // Generate first strand (one helix)
        for (int i = 0; i < pointsPerStrand; i++)
        {
            float t = i / (float)(pointsPerStrand - 1);
            float angle = t * twists * 2f * Mathf.PI + phase;
            float height = (t - 0.5f) * helixHeight;
            
            Vector3 position = new Vector3(
                helixRadius * Mathf.Cos(angle),
                height,
                helixRadius * Mathf.Sin(angle)
            );
            CreatePoint(position, i);
        }
        
        // Generate second strand (opposite helix, 180 degrees offset)
        for (int i = 0; i < pointsPerStrand && (pointsPerStrand + i) < numberOfPoints; i++)
        {
            float t = i / (float)(pointsPerStrand - 1);
            float angle = t * twists * 2f * Mathf.PI + phase + Mathf.PI; // 180° offset
            float height = (t - 0.5f) * helixHeight;
            
            Vector3 position = new Vector3(
                helixRadius * Mathf.Cos(angle),
                height,
                helixRadius * Mathf.Sin(angle)
            );
            CreatePoint(position, pointsPerStrand + i);
        }
    }

    private void GenerateText()
    {
        if (string.IsNullOrEmpty(textToGenerate))
        {
            Debug.LogWarning("No text entered! Please type a word in the inspector.");
            return;
        }

        string text = textToGenerate.ToUpper();
        float currentXOffset = 0f;
        int totalPointIndex = 0;

        foreach (char c in text)
        {
            if (c == ' ')
            {
                currentXOffset += letterSpacing * 1.5f; // Extra space for actual spaces
                continue;
            }

            Vector3[] letterPoints = GetLetterPoints(c);
            
            if (letterPoints != null && letterPoints.Length > 0)
            {
                foreach (Vector3 point in letterPoints)
                {
                    Vector3 scaledPoint = point * letterScale;
                    Vector3 position = new Vector3(
                        scaledPoint.x + currentXOffset,
                        scaledPoint.y,
                        generate3D ? scaledPoint.z : 0f
                    );
                    CreatePoint(position, totalPointIndex++);
                }
                currentXOffset += letterSpacing;
            }
        }
    }

    private void GenerateMonaLisa()
    {
        // High-Quality Mona Lisa - ~700 points, clean composition
        
        float scale = letterScale;
        int pointIndex = 0;
        
        // ============== BACKGROUND ==============
        
        // Left mountains (lower horizon)
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.05f, 0.70f, 0), new Vector3(0.10f, 0.74f, 0),
            new Vector3(0.15f, 0.71f, 0), new Vector3(0.20f, 0.67f, 0)
        }, ref pointIndex, scale, 15);
        
        // Right mountains (higher)
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.78f, 0.73f, 0), new Vector3(0.84f, 0.77f, 0),
            new Vector3(0.90f, 0.72f, 0), new Vector3(0.95f, 0.68f, 0)
        }, ref pointIndex, scale, 15);
        
        // Bridge
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.80f, 0.58f, 0), new Vector3(0.84f, 0.565f, 0),
            new Vector3(0.88f, 0.58f, 0)
        }, ref pointIndex, scale, 12);
        
        // ============== BODY & CLOTHING ==============
        
        // Dress neckline
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.25f, 0.48f, 0), new Vector3(0.35f, 0.52f, 0),
            new Vector3(0.42f, 0.545f, 0), new Vector3(0.50f, 0.545f, 0),
            new Vector3(0.58f, 0.545f, 0), new Vector3(0.65f, 0.52f, 0),
            new Vector3(0.75f, 0.48f, 0)
        }, ref pointIndex, scale, 25);
        
        // Left shoulder
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.25f, 0.48f, 0), new Vector3(0.27f, 0.42f, 0),
            new Vector3(0.29f, 0.36f, 0)
        }, ref pointIndex, scale, 15);
        
        // Right shoulder
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.75f, 0.48f, 0), new Vector3(0.73f, 0.42f, 0),
            new Vector3(0.71f, 0.36f, 0)
        }, ref pointIndex, scale, 15);
        
        // ============== HANDS ==============
        
        // Right hand (top left)
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.32f, 0.28f, 0), new Vector3(0.36f, 0.27f, 0),
            new Vector3(0.40f, 0.26f, 0), new Vector3(0.44f, 0.25f, 0)
        }, ref pointIndex, scale, 18);
        
        // Left hand (bottom right)
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.54f, 0.27f, 0), new Vector3(0.58f, 0.26f, 0),
            new Vector3(0.62f, 0.25f, 0)
        }, ref pointIndex, scale, 15);
        
        // Armrest
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.30f, 0.30f, 0), new Vector3(0.50f, 0.295f, 0),
            new Vector3(0.70f, 0.30f, 0)
        }, ref pointIndex, scale, 15);
        
        // ============== HAIR ==============
        
        // Center part
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.50f, 0.90f, 0), new Vector3(0.50f, 0.85f, 0)
        }, ref pointIndex, scale, 10);
        
        // Left hair - Outer
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.45f, 0.88f, 0), new Vector3(0.38f, 0.84f, 0),
            new Vector3(0.33f, 0.78f, 0), new Vector3(0.31f, 0.70f, 0),
            new Vector3(0.30f, 0.62f, 0), new Vector3(0.31f, 0.54f, 0)
        }, ref pointIndex, scale, 30);
        
        // Left hair - Inner
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.48f, 0.85f, 0), new Vector3(0.44f, 0.80f, 0),
            new Vector3(0.41f, 0.74f, 0), new Vector3(0.40f, 0.68f, 0)
        }, ref pointIndex, scale, 20);
        
        // Right hair - Outer
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.55f, 0.88f, 0), new Vector3(0.62f, 0.84f, 0),
            new Vector3(0.67f, 0.78f, 0), new Vector3(0.69f, 0.70f, 0),
            new Vector3(0.70f, 0.62f, 0), new Vector3(0.69f, 0.54f, 0)
        }, ref pointIndex, scale, 30);
        
        // Right hair - Inner
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.52f, 0.85f, 0), new Vector3(0.56f, 0.80f, 0),
            new Vector3(0.59f, 0.74f, 0), new Vector3(0.60f, 0.68f, 0)
        }, ref pointIndex, scale, 20);
        
        // ============== FACE ==============
        
        // Face outline - Right side
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.52f, 0.84f, 0), new Vector3(0.58f, 0.81f, 0),
            new Vector3(0.63f, 0.76f, 0), new Vector3(0.66f, 0.69f, 0),
            new Vector3(0.665f, 0.62f, 0), new Vector3(0.64f, 0.56f, 0),
            new Vector3(0.58f, 0.54f, 0), new Vector3(0.50f, 0.528f, 0)
        }, ref pointIndex, scale, 40);
        
        // Face outline - Left side
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.48f, 0.84f, 0), new Vector3(0.42f, 0.81f, 0),
            new Vector3(0.37f, 0.76f, 0), new Vector3(0.34f, 0.69f, 0),
            new Vector3(0.335f, 0.62f, 0), new Vector3(0.36f, 0.56f, 0),
            new Vector3(0.42f, 0.54f, 0), new Vector3(0.50f, 0.528f, 0)
        }, ref pointIndex, scale, 40);
        
        // ============== EYES ==============
        
        // Left eye - Upper lid
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.40f, 0.73f, 0), new Vector3(0.425f, 0.738f, 0),
            new Vector3(0.45f, 0.738f, 0), new Vector3(0.47f, 0.73f, 0)
        }, ref pointIndex, scale, 18);
        
        // Left eye - Lower lid
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.40f, 0.73f, 0), new Vector3(0.425f, 0.725f, 0),
            new Vector3(0.45f, 0.725f, 0), new Vector3(0.47f, 0.73f, 0)
        }, ref pointIndex, scale, 18);
        
        // Left iris & pupil
        AddCircle(new Vector3(0.435f, 0.731f, 0), 0.008f, ref pointIndex, scale, 12);
        AddCircle(new Vector3(0.435f, 0.731f, 0), 0.004f, ref pointIndex, scale, 8);
        
        // Right eye - Upper lid
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.53f, 0.73f, 0), new Vector3(0.555f, 0.738f, 0),
            new Vector3(0.58f, 0.738f, 0), new Vector3(0.60f, 0.73f, 0)
        }, ref pointIndex, scale, 18);
        
        // Right eye - Lower lid
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.53f, 0.73f, 0), new Vector3(0.555f, 0.725f, 0),
            new Vector3(0.58f, 0.725f, 0), new Vector3(0.60f, 0.73f, 0)
        }, ref pointIndex, scale, 18);
        
        // Right iris & pupil
        AddCircle(new Vector3(0.565f, 0.731f, 0), 0.008f, ref pointIndex, scale, 12);
        AddCircle(new Vector3(0.565f, 0.731f, 0), 0.004f, ref pointIndex, scale, 8);
        
        // ============== EYEBROWS ==============
        
        // Left eyebrow
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.39f, 0.757f, 0), new Vector3(0.42f, 0.763f, 0),
            new Vector3(0.44f, 0.764f, 0), new Vector3(0.47f, 0.760f, 0)
        }, ref pointIndex, scale, 18);
        
        // Right eyebrow
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.53f, 0.760f, 0), new Vector3(0.56f, 0.764f, 0),
            new Vector3(0.58f, 0.763f, 0), new Vector3(0.61f, 0.757f, 0)
        }, ref pointIndex, scale, 18);
        
        // ============== NOSE ==============
        
        // Nose bridge
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.50f, 0.725f, 0), new Vector3(0.50f, 0.695f, 0),
            new Vector3(0.50f, 0.665f, 0)
        }, ref pointIndex, scale, 15);
        
        // Nose tip & nostrils
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.48f, 0.655f, 0), new Vector3(0.50f, 0.650f, 0),
            new Vector3(0.52f, 0.655f, 0)
        }, ref pointIndex, scale, 12);
        
        // Left nostril
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.48f, 0.655f, 0), new Vector3(0.485f, 0.653f, 0),
            new Vector3(0.49f, 0.655f, 0)
        }, ref pointIndex, scale, 8);
        
        // Right nostril
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.51f, 0.655f, 0), new Vector3(0.515f, 0.653f, 0),
            new Vector3(0.52f, 0.655f, 0)
        }, ref pointIndex, scale, 8);
        
        // ============== THE FAMOUS SMILE ==============
        
        // Upper lip
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.42f, 0.620f, 0), new Vector3(0.45f, 0.618f, 0),
            new Vector3(0.48f, 0.617f, 0), new Vector3(0.50f, 0.6165f, 0),
            new Vector3(0.52f, 0.617f, 0), new Vector3(0.55f, 0.618f, 0),
            new Vector3(0.58f, 0.620f, 0)
        }, ref pointIndex, scale, 30);
        
        // Lower lip
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.42f, 0.620f, 0), new Vector3(0.45f, 0.615f, 0),
            new Vector3(0.48f, 0.613f, 0), new Vector3(0.50f, 0.612f, 0),
            new Vector3(0.52f, 0.613f, 0), new Vector3(0.55f, 0.615f, 0),
            new Vector3(0.58f, 0.620f, 0)
        }, ref pointIndex, scale, 30);
        
        // Philtrum (nose to lip)
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.50f, 0.650f, 0), new Vector3(0.50f, 0.635f, 0),
            new Vector3(0.50f, 0.620f, 0)
        }, ref pointIndex, scale, 12);
        
        // ============== NECK & CHIN ==============
        
        // Chin
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.44f, 0.535f, 0), new Vector3(0.50f, 0.528f, 0),
            new Vector3(0.56f, 0.535f, 0)
        }, ref pointIndex, scale, 15);
        
        // Neck - Left
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.42f, 0.545f, 0), new Vector3(0.40f, 0.535f, 0)
        }, ref pointIndex, scale, 10);
        
        // Neck - Right
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.58f, 0.545f, 0), new Vector3(0.60f, 0.535f, 0)
        }, ref pointIndex, scale, 10);
        
        Debug.Log($"✨ Mona Lisa created with {pointIndex} perfectly placed points! ✨");
    }

    // Helper method for detailed curves
    private void AddDetailedCurve(Vector3[] controlPoints, ref int startIndex, float scale, int smoothness)
    {
        Vector3[] curve = GenerateSmoothCurve(controlPoints, smoothness);
        AddPoints(curve, ref startIndex, scale);
    }

    // Helper method to create circles
    private void AddCircle(Vector3 center, float radius, ref int startIndex, float scale, int segments)
    {
        for (int i = 0; i < segments; i++)
        {
            float angle = (i / (float)segments) * 2f * Mathf.PI;
            Vector3 point = center + new Vector3(
                radius * Mathf.Cos(angle),
                radius * Mathf.Sin(angle),
                0f
            );
            Vector3 centeredPoint = (point - new Vector3(0.5f, 0.5f, 0)) * scale * 10f;
            CreatePoint(centeredPoint, startIndex++);
        }
    }

    private Vector3[] GenerateSmoothCurve(Vector3[] controlPoints, int smoothness)
    {
        // Creates smooth curves between control points using interpolation
        if (controlPoints.Length < 2) return controlPoints;
        
        List<Vector3> smoothPoints = new List<Vector3>();
        
        for (int i = 0; i < controlPoints.Length - 1; i++)
        {
            Vector3 start = controlPoints[i];
            Vector3 end = controlPoints[i + 1];
            
            for (int j = 0; j <= smoothness; j++)
            {
                float t = j / (float)smoothness;
                Vector3 point = Vector3.Lerp(start, end, t);
                smoothPoints.Add(point);
            }
        }
        
        return smoothPoints.ToArray();
    }

    private void AddPoints(Vector3[] points, ref int startIndex, float scale)
    {
        foreach (Vector3 point in points)
        {
            // Center the portrait and scale it
            Vector3 centeredPoint = (point - new Vector3(0.5f, 0.5f, 0)) * scale * 10f;
            CreatePoint(centeredPoint, startIndex++);
        }
    }

    private Vector3[] GetLetterPoints(char letter)
    {
        // Define letters using point coordinates (normalized to 0-1 space)
        switch (letter)
        {
            case 'A':
                return new Vector3[] {
                    new Vector3(0, 0, 0), new Vector3(0.5f, 1, 0), new Vector3(1, 0, 0),
                    new Vector3(0.25f, 0.5f, 0), new Vector3(0.75f, 0.5f, 0)
                };
            case 'B':
                return new Vector3[] {
                    new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0.7f, 0.9f, 0),
                    new Vector3(0.8f, 0.75f, 0), new Vector3(0.7f, 0.5f, 0), new Vector3(0, 0.5f, 0),
                    new Vector3(0.75f, 0.4f, 0), new Vector3(0.85f, 0.2f, 0), new Vector3(0.7f, 0, 0), new Vector3(0, 0, 0)
                };
            case 'C':
                return new Vector3[] {
                    new Vector3(1, 0.9f, 0), new Vector3(0.7f, 1, 0), new Vector3(0.3f, 1, 0),
                    new Vector3(0, 0.7f, 0), new Vector3(0, 0.3f, 0), new Vector3(0.3f, 0, 0),
                    new Vector3(0.7f, 0, 0), new Vector3(1, 0.1f, 0)
                };
            case 'D':
                return new Vector3[] {
                    new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0.6f, 1, 0),
                    new Vector3(1, 0.7f, 0), new Vector3(1, 0.3f, 0), new Vector3(0.6f, 0, 0), new Vector3(0, 0, 0)
                };
            case 'E':
                return new Vector3[] {
                    new Vector3(1, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 1, 0),
                    new Vector3(1, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 0.5f, 0),
                    new Vector3(0.8f, 0.5f, 0)
                };
            case 'F':
                return new Vector3[] {
                    new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0),
                    new Vector3(0, 1, 0), new Vector3(0, 0.5f, 0), new Vector3(0.8f, 0.5f, 0)
                };
            case 'G':
                return new Vector3[] {
                    new Vector3(1, 0.9f, 0), new Vector3(0.7f, 1, 0), new Vector3(0.3f, 1, 0),
                    new Vector3(0, 0.7f, 0), new Vector3(0, 0.3f, 0), new Vector3(0.3f, 0, 0),
                    new Vector3(0.7f, 0, 0), new Vector3(1, 0.3f, 0), new Vector3(1, 0.5f, 0),
                    new Vector3(0.6f, 0.5f, 0)
                };
            case 'H':
                return new Vector3[] {
                    new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0.5f, 0),
                    new Vector3(1, 0.5f, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0)
                };
            case 'I':
                return new Vector3[] {
                    new Vector3(0.2f, 1, 0), new Vector3(0.8f, 1, 0), new Vector3(0.5f, 1, 0),
                    new Vector3(0.5f, 0, 0), new Vector3(0.2f, 0, 0), new Vector3(0.8f, 0, 0)
                };
            case 'J':
                return new Vector3[] {
                    new Vector3(1, 1, 0), new Vector3(1, 0.3f, 0), new Vector3(0.7f, 0, 0),
                    new Vector3(0.3f, 0, 0), new Vector3(0, 0.3f, 0)
                };
            case 'K':
                return new Vector3[] {
                    new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0.5f, 0),
                    new Vector3(1, 1, 0), new Vector3(0, 0.5f, 0), new Vector3(1, 0, 0)
                };
            case 'L':
                return new Vector3[] {
                    new Vector3(0, 1, 0), new Vector3(0, 0, 0), new Vector3(1, 0, 0)
                };
            case 'M':
                return new Vector3[] {
                    new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0.5f, 0.5f, 0),
                    new Vector3(1, 1, 0), new Vector3(1, 0, 0)
                };
            case 'N':
                return new Vector3[] {
                    new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0)
                };
            case 'O':
                return new Vector3[] {
                    new Vector3(0.3f, 1, 0), new Vector3(0.7f, 1, 0), new Vector3(1, 0.7f, 0),
                    new Vector3(1, 0.3f, 0), new Vector3(0.7f, 0, 0), new Vector3(0.3f, 0, 0),
                    new Vector3(0, 0.3f, 0), new Vector3(0, 0.7f, 0), new Vector3(0.3f, 1, 0)
                };
            case 'P':
                return new Vector3[] {
                    new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0.7f, 1, 0),
                    new Vector3(1, 0.8f, 0), new Vector3(1, 0.6f, 0), new Vector3(0.7f, 0.5f, 0), new Vector3(0, 0.5f, 0)
                };
            case 'Q':
                return new Vector3[] {
                    new Vector3(0.3f, 1, 0), new Vector3(0.7f, 1, 0), new Vector3(1, 0.7f, 0),
                    new Vector3(1, 0.3f, 0), new Vector3(0.7f, 0, 0), new Vector3(0.3f, 0, 0),
                    new Vector3(0, 0.3f, 0), new Vector3(0, 0.7f, 0), new Vector3(0.3f, 1, 0),
                    new Vector3(0.6f, 0.3f, 0), new Vector3(1.1f, -0.1f, 0)
                };
            case 'R':
                return new Vector3[] {
                    new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0.7f, 1, 0),
                    new Vector3(1, 0.8f, 0), new Vector3(1, 0.6f, 0), new Vector3(0.7f, 0.5f, 0),
                    new Vector3(0, 0.5f, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(1, 0, 0)
                };
            case 'S':
                return new Vector3[] {
                    new Vector3(1, 0.9f, 0), new Vector3(0.7f, 1, 0), new Vector3(0.3f, 1, 0),
                    new Vector3(0, 0.8f, 0), new Vector3(0.3f, 0.5f, 0), new Vector3(0.7f, 0.5f, 0),
                    new Vector3(1, 0.2f, 0), new Vector3(0.7f, 0, 0), new Vector3(0.3f, 0, 0), new Vector3(0, 0.1f, 0)
                };
            case 'T':
                return new Vector3[] {
                    new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(0.5f, 1, 0), new Vector3(0.5f, 0, 0)
                };
            case 'U':
                return new Vector3[] {
                    new Vector3(0, 1, 0), new Vector3(0, 0.3f, 0), new Vector3(0.3f, 0, 0),
                    new Vector3(0.7f, 0, 0), new Vector3(1, 0.3f, 0), new Vector3(1, 1, 0)
                };
            case 'V':
                return new Vector3[] {
                    new Vector3(0, 1, 0), new Vector3(0.5f, 0, 0), new Vector3(1, 1, 0)
                };
            case 'W':
                return new Vector3[] {
                    new Vector3(0, 1, 0), new Vector3(0.25f, 0, 0), new Vector3(0.5f, 0.5f, 0),
                    new Vector3(0.75f, 0, 0), new Vector3(1, 1, 0)
                };
            case 'X':
                return new Vector3[] {
                    new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector3(0.5f, 0.5f, 0),
                    new Vector3(0, 0, 0), new Vector3(1, 1, 0)
                };
            case 'Y':
                return new Vector3[] {
                    new Vector3(0, 1, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(1, 1, 0),
                    new Vector3(0.5f, 0.5f, 0), new Vector3(0.5f, 0, 0)
                };
            case 'Z':
                return new Vector3[] {
                    new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(0, 0, 0), new Vector3(1, 0, 0)
                };
            case '!':
                return new Vector3[] {
                    new Vector3(0.5f, 1, 0), new Vector3(0.5f, 0.3f, 0), new Vector3(0.5f, 0.1f, 0), new Vector3(0.5f, 0, 0)
                };
            case '?':
                return new Vector3[] {
                    new Vector3(0.2f, 0.8f, 0), new Vector3(0.5f, 1, 0), new Vector3(0.8f, 0.8f, 0),
                    new Vector3(0.8f, 0.6f, 0), new Vector3(0.5f, 0.4f, 0), new Vector3(0.5f, 0.1f, 0), new Vector3(0.5f, 0, 0)
                };
            case '0':
                return new Vector3[] {
                    new Vector3(0.3f, 1, 0), new Vector3(0.7f, 1, 0), new Vector3(1, 0.7f, 0),
                    new Vector3(1, 0.3f, 0), new Vector3(0.7f, 0, 0), new Vector3(0.3f, 0, 0),
                    new Vector3(0, 0.3f, 0), new Vector3(0, 0.7f, 0), new Vector3(0.3f, 1, 0),
                    new Vector3(0.8f, 0.2f, 0)
                };
            case '1':
                return new Vector3[] {
                    new Vector3(0.3f, 0.8f, 0), new Vector3(0.5f, 1, 0), new Vector3(0.5f, 0, 0),
                    new Vector3(0.2f, 0, 0), new Vector3(0.8f, 0, 0)
                };
            case '2':
                return new Vector3[] {
                    new Vector3(0, 0.8f, 0), new Vector3(0.3f, 1, 0), new Vector3(0.7f, 1, 0),
                    new Vector3(1, 0.7f, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(0, 0, 0), new Vector3(1, 0, 0)
                };
            case '3':
                return new Vector3[] {
                    new Vector3(0, 0.9f, 0), new Vector3(0.7f, 1, 0), new Vector3(1, 0.7f, 0),
                    new Vector3(0.6f, 0.5f, 0), new Vector3(1, 0.3f, 0), new Vector3(0.7f, 0, 0), new Vector3(0, 0.1f, 0)
                };
            default:
                return new Vector3[] { new Vector3(0.5f, 0.5f, 0) }; // Unknown char - single point
        }
    }

    private void CreatePoint(Vector3 localPosition, int index)
    {
        GameObject point;
        
        if (useObjectPooling && objectPool.Count > 0)
        {
            // Reuse from pool
            point = objectPool.Dequeue();
            point.name = $"Point_{index}";
            point.SetActive(true);
        }
        else
        {
            // Create new
            point = new GameObject($"Point_{index}");
        }
        
        point.transform.SetParent(parentTransform);
        point.transform.localPosition = localPosition;
        generatedObjects.Add(point);
    }
    
    private void InitializeObjectPool()
    {
        // Pre-create pool objects for performance
        if (!useObjectPooling) return;
        
        for (int i = 0; i < POOL_INITIAL_SIZE; i++)
        {
            GameObject obj = new GameObject("PooledPoint");
            obj.SetActive(false);
            objectPool.Enqueue(obj);
        }
    }
    
    private void ReturnToPool(GameObject obj)
    {
        if (!useObjectPooling)
        {
            DestroyImmediate(obj);
            return;
        }
        
        obj.SetActive(false);
        obj.transform.SetParent(null);
        objectPool.Enqueue(obj);
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || generatedObjects.Count == 0) return;

        Gizmos.color = gizmoColor;
        
        // Draw points
        foreach (GameObject obj in generatedObjects)
        {
            if (obj != null)
            {
                Gizmos.DrawWireSphere(obj.transform.position, gizmoSize);
            }
        }

        if (drawConnections && generatedObjects.Count > 1)
        {
            // Special handling for DNA Double Helix
            if (shapeType == ShapeType.DNADoubleHelix)
            {
                DrawDNAConnections();
            }
            else if (shapeType == ShapeType.TextWord || shapeType == ShapeType.MonaLisa)
            {
                // Just connect consecutive points for text and portraits (no loop closure)
                for (int i = 0; i < generatedObjects.Count - 1; i++)
                {
                    if (generatedObjects[i] != null && generatedObjects[i + 1] != null)
                    {
                        Gizmos.DrawLine(generatedObjects[i].transform.position, 
                                       generatedObjects[i + 1].transform.position);
                    }
                }
            }
            else
            {
                // Draw connections between consecutive points
                for (int i = 0; i < generatedObjects.Count - 1; i++)
                {
                    if (generatedObjects[i] != null && generatedObjects[i + 1] != null)
                    {
                        Gizmos.DrawLine(generatedObjects[i].transform.position, 
                                       generatedObjects[i + 1].transform.position);
                    }
                }
                
                // CLOSE THE LOOP - Connect last point to first point
                if (generatedObjects[generatedObjects.Count - 1] != null && 
                    generatedObjects[0] != null)
                {
                    Gizmos.color = Color.Lerp(gizmoColor, Color.white, 0.5f); // Highlight closing line
                    Gizmos.DrawLine(generatedObjects[generatedObjects.Count - 1].transform.position, 
                                   generatedObjects[0].transform.position);
                }
            }
        }
    }

    private void DrawDNAConnections()
    {
        int pointsPerStrand = generatedObjects.Count / 2;
        
        // Draw first strand
        Gizmos.color = gizmoColor;
        for (int i = 0; i < pointsPerStrand - 1; i++)
        {
            if (generatedObjects[i] != null && generatedObjects[i + 1] != null)
            {
                Gizmos.DrawLine(generatedObjects[i].transform.position, 
                               generatedObjects[i + 1].transform.position);
            }
        }
        
        // Draw second strand
        Gizmos.color = Color.Lerp(gizmoColor, Color.yellow, 0.3f);
        for (int i = pointsPerStrand; i < generatedObjects.Count - 1; i++)
        {
            if (generatedObjects[i] != null && generatedObjects[i + 1] != null)
            {
                Gizmos.DrawLine(generatedObjects[i].transform.position, 
                               generatedObjects[i + 1].transform.position);
            }
        }
        
        // Draw base pairs (connecting rungs between the two strands)
        Gizmos.color = Color.Lerp(gizmoColor, Color.red, 0.5f);
        int basePairStep = Mathf.Max(1, pointsPerStrand / frequencyB);
        for (int i = 0; i < pointsPerStrand; i += basePairStep)
        {
            if (i < pointsPerStrand && (pointsPerStrand + i) < generatedObjects.Count)
            {
                if (generatedObjects[i] != null && generatedObjects[pointsPerStrand + i] != null)
                {
                    Gizmos.DrawLine(generatedObjects[i].transform.position, 
                                   generatedObjects[pointsPerStrand + i].transform.position);
                }
            }
        }
    }

    private void GenerateMetaLogo()
    {
        // Meta's infinity logo - continuous infinity symbol with modern styling
        float scale = radius;
        int pointIndex = 0;
        int pointsPerLoop = numberOfPoints / 2;
        
        // Left loop of the infinity symbol
        for (int i = 0; i < pointsPerLoop; i++)
        {
            float t = (i / (float)pointsPerLoop) * 2f * Mathf.PI;
            
            // Parametric infinity curve - left side
            float x = -scale * Mathf.Cos(t) / (1 + Mathf.Sin(t) * Mathf.Sin(t));
            float y = scale * Mathf.Sin(t) * Mathf.Cos(t) / (1 + Mathf.Sin(t) * Mathf.Sin(t));
            
            Vector3 position = new Vector3(x - scale * 0.5f, y, 0f);
            
            if (generate3D)
            {
                position.z = Mathf.Sin(t * heightFrequency) * heightAmplitude;
            }
            
            CreatePoint(position, pointIndex++);
        }
        
        // Right loop of the infinity symbol
        for (int i = 0; i < pointsPerLoop; i++)
        {
            float t = (i / (float)pointsPerLoop) * 2f * Mathf.PI;
            
            // Parametric infinity curve - right side (mirrored)
            float x = scale * Mathf.Cos(t) / (1 + Mathf.Sin(t) * Mathf.Sin(t));
            float y = scale * Mathf.Sin(t) * Mathf.Cos(t) / (1 + Mathf.Sin(t) * Mathf.Sin(t));
            
            Vector3 position = new Vector3(x + scale * 0.5f, y, 0f);
            
            if (generate3D)
            {
                position.z = Mathf.Sin(t * heightFrequency + Mathf.PI) * heightAmplitude;
            }
            
            CreatePoint(position, pointIndex++);
        }
        
        Debug.Log($"Meta Logo generated with {pointIndex} points!");
    }

    private void GenerateSmileyFace()
    {
        // Classic smiley face - ONE CONTINUOUS LINE forming the whole face!
        // Path: Face outline → left eye → back to face → right eye → back to face → smile → close
        float scale = radius;
        int pointIndex = 0;
        
        float eyeRadius = scale * 0.15f;
        float eyeOffsetX = scale * 0.3f;
        float eyeOffsetY = scale * 0.25f;
        
        // PART 1: Face circle (RIGHT SIDE - from top going clockwise to left eye position)
        int facePoints1 = (int)(numberOfPoints * 0.15f);
        for (int i = 0; i <= facePoints1; i++)
        {
            float angle = (i / (float)facePoints1) * Mathf.PI * 0.6f; // Top to left side
            float x = scale * Mathf.Cos(angle);
            float y = scale * Mathf.Sin(angle);
            CreatePoint(new Vector3(x, y, 0f), pointIndex++);
        }
        
        // PART 2: Connect to LEFT EYE (move inward to eye position)
        for (int i = 0; i <= 5; i++)
        {
            float t = i / 5f;
            float startX = scale * Mathf.Cos(Mathf.PI * 0.6f);
            float startY = scale * Mathf.Sin(Mathf.PI * 0.6f);
            float x = Mathf.Lerp(startX, -eyeOffsetX + eyeRadius, t);
            float y = Mathf.Lerp(startY, eyeOffsetY, t);
            CreatePoint(new Vector3(x, y, 0f), pointIndex++);
        }
        
        // PART 3: LEFT EYE (full circle)
        int eyePoints = (int)(numberOfPoints * 0.08f);
        for (int i = 0; i < eyePoints; i++)
        {
            float angle = (i / (float)eyePoints) * 2f * Mathf.PI;
            float x = -eyeOffsetX + eyeRadius * Mathf.Cos(angle);
            float y = eyeOffsetY + eyeRadius * Mathf.Sin(angle);
            CreatePoint(new Vector3(x, y, 0f), pointIndex++);
        }
        
        // PART 4: Connect back to face (left eye to face arc)
        for (int i = 0; i <= 5; i++)
        {
            float t = i / 5f;
            float x = Mathf.Lerp(-eyeOffsetX + eyeRadius, scale * Mathf.Cos(Mathf.PI * 0.85f), t);
            float y = Mathf.Lerp(eyeOffsetY, scale * Mathf.Sin(Mathf.PI * 0.85f), t);
            CreatePoint(new Vector3(x, y, 0f), pointIndex++);
        }
        
        // PART 5: Face circle (LEFT SIDE arc to right eye position)
        int facePoints2 = (int)(numberOfPoints * 0.1f);
        for (int i = 0; i <= facePoints2; i++)
        {
            float angle = Mathf.PI * 0.85f + (i / (float)facePoints2) * Mathf.PI * 0.45f;
            float x = scale * Mathf.Cos(angle);
            float y = scale * Mathf.Sin(angle);
            CreatePoint(new Vector3(x, y, 0f), pointIndex++);
        }
        
        // PART 6: Connect to RIGHT EYE
        for (int i = 0; i <= 5; i++)
        {
            float t = i / 5f;
            float startX = scale * Mathf.Cos(Mathf.PI * 1.3f);
            float startY = scale * Mathf.Sin(Mathf.PI * 1.3f);
            float x = Mathf.Lerp(startX, eyeOffsetX - eyeRadius, t);
            float y = Mathf.Lerp(startY, eyeOffsetY, t);
            CreatePoint(new Vector3(x, y, 0f), pointIndex++);
        }
        
        // PART 7: RIGHT EYE (full circle)
        for (int i = 0; i < eyePoints; i++)
        {
            float angle = (i / (float)eyePoints) * 2f * Mathf.PI;
            float x = eyeOffsetX + eyeRadius * Mathf.Cos(angle);
            float y = eyeOffsetY + eyeRadius * Mathf.Sin(angle);
            CreatePoint(new Vector3(x, y, 0f), pointIndex++);
        }
        
        // PART 8: Connect back to face (right eye to face)
        for (int i = 0; i <= 5; i++)
        {
            float t = i / 5f;
            float x = Mathf.Lerp(eyeOffsetX - eyeRadius, scale * Mathf.Cos(Mathf.PI * 1.5f), t);
            float y = Mathf.Lerp(eyeOffsetY, scale * Mathf.Sin(Mathf.PI * 1.5f), t);
            CreatePoint(new Vector3(x, y, 0f), pointIndex++);
        }
        
        // PART 9: Face circle (BOTTOM arc to smile)
        int facePoints3 = (int)(numberOfPoints * 0.12f);
        for (int i = 0; i <= facePoints3; i++)
        {
            float angle = Mathf.PI * 1.5f + (i / (float)facePoints3) * Mathf.PI * 0.3f;
            float x = scale * Mathf.Cos(angle);
            float y = scale * Mathf.Sin(angle);
            CreatePoint(new Vector3(x, y, 0f), pointIndex++);
        }
        
        // PART 10: Connect to SMILE (move inward)
        float smileRadius = scale * 0.6f;
        for (int i = 0; i <= 5; i++)
        {
            float t = i / 5f;
            float startX = scale * Mathf.Cos(Mathf.PI * 1.8f);
            float startY = scale * Mathf.Sin(Mathf.PI * 1.8f);
            float smileStart = 200f * Mathf.Deg2Rad;
            float x = Mathf.Lerp(startX, smileRadius * Mathf.Cos(smileStart), t);
            float y = Mathf.Lerp(startY, smileRadius * Mathf.Sin(smileStart) - scale * 0.2f, t);
            CreatePoint(new Vector3(x, y, 0f), pointIndex++);
        }
        
        // PART 11: THE SMILE (arc)
        int smilePoints = (int)(numberOfPoints * 0.15f);
        for (int i = 0; i < smilePoints; i++)
        {
            float t = i / (float)(smilePoints - 1);
            float angle = Mathf.Lerp(200f * Mathf.Deg2Rad, 340f * Mathf.Deg2Rad, t);
            float x = smileRadius * Mathf.Cos(angle);
            float y = smileRadius * Mathf.Sin(angle) - scale * 0.2f;
            CreatePoint(new Vector3(x, y, 0f), pointIndex++);
        }
        
        // PART 12: Connect smile back to face and close
        for (int i = 0; i <= 5; i++)
        {
            float t = i / 5f;
            float smileEnd = 340f * Mathf.Deg2Rad;
            float x = Mathf.Lerp(smileRadius * Mathf.Cos(smileEnd), scale * Mathf.Cos(smileEnd), t);
            float y = Mathf.Lerp(smileRadius * Mathf.Sin(smileEnd) - scale * 0.2f, scale * Mathf.Sin(smileEnd), t);
            CreatePoint(new Vector3(x, y, 0f), pointIndex++);
        }
        
        // PART 13: Complete the face circle back to start
        int facePoints4 = (int)(numberOfPoints * 0.08f);
        for (int i = 0; i <= facePoints4; i++)
        {
            float angle = 340f * Mathf.Deg2Rad + (i / (float)facePoints4) * (Mathf.PI * 2f - 340f * Mathf.Deg2Rad);
            float x = scale * Mathf.Cos(angle);
            float y = scale * Mathf.Sin(angle);
            CreatePoint(new Vector3(x, y, 0f), pointIndex++);
        }
        
        Debug.Log($"😊 Smiley Face: {pointIndex} points in ONE continuous drawing! :)");
    }

    private void GenerateMcDonaldsLogo()
    {
        // ===== MCDONALD'S GOLDEN ARCHES - CORRECT PROPORTIONS =====
        // Key: WIDE base, LOWER height, arches CLOSE TOGETHER at top
        
        float scale = radius;
        int pointIndex = 0;
        
        // Real McDonald's proportions: Wide and low!
        float archBaseWidth = 2.5f * scale;    // Width of each arch
        float archHeight = 3.2f * scale;       // Much shorter! (was 4.5)
        float archThickness = 0.45f * scale;   // Thickness
        float archSeparation = 0.15f * scale;  // VERY close together! (was 0.8)
        
        int points = 45; // Smooth curves
        
        // ===== LEFT ARCH =====
        
        // OUTER edge of left arch
        for (int i = 0; i <= points; i++)
        {
            float t = i / (float)points;
            float x = -archSeparation / 2f - archBaseWidth * (1f - t);
            // Wider parabola - flatter curve
            float y = archHeight * Mathf.Sqrt(1f - Mathf.Pow(1f - t, 2));
            
            CreatePoint(new Vector3(x, y, 0f), pointIndex++);
        }
        
        // INNER edge of left arch
        for (int i = points; i >= 0; i--)
        {
            float t = i / (float)points;
            float x = -archSeparation / 2f - (archBaseWidth - archThickness) * (1f - t);
            float y = (archHeight - archThickness * 0.7f) * Mathf.Sqrt(1f - Mathf.Pow(1f - t, 2));
            if (y < 0) y = 0;
            
            CreatePoint(new Vector3(x, y, 0f), pointIndex++);
        }
        
        // Close left arch at base
        CreatePoint(new Vector3(-archSeparation / 2f - archBaseWidth, 0f, 0f), pointIndex++);
        
        
        // ===== RIGHT ARCH =====
        
        // OUTER edge of right arch
        for (int i = 0; i <= points; i++)
        {
            float t = i / (float)points;
            float x = archSeparation / 2f + archBaseWidth * t;
            float y = archHeight * Mathf.Sqrt(1f - Mathf.Pow(t, 2));
            
            CreatePoint(new Vector3(x, y, 0f), pointIndex++);
        }
        
        // INNER edge of right arch
        for (int i = points; i >= 0; i--)
        {
            float t = i / (float)points;
            float x = archSeparation / 2f + (archBaseWidth - archThickness) * t;
            float y = (archHeight - archThickness * 0.7f) * Mathf.Sqrt(1f - Mathf.Pow(t, 2));
            if (y < 0) y = 0;
            
            CreatePoint(new Vector3(x, y, 0f), pointIndex++);
        }
        
        // Close right arch at base
        CreatePoint(new Vector3(archSeparation / 2f + archBaseWidth, 0f, 0f), pointIndex++);
        
        Debug.Log($"🍔 McDonald's Golden Arches: {pointIndex} points! Getting closer! 🟡");
    }

    private void GenerateWuTangLogo()
    {
        // Wu-Tang Clan's iconic "W" logo
        // Sharp, bold, angular design with the distinctive bat-wing style
        
        float scale = radius;
        int pointIndex = 0;
        
        // The Wu-Tang W has a very distinctive shape:
        // - Wide outer wings
        // - Sharp angular points
        // - Center trident/bat style
        // - Bold, aggressive styling
        
        // ===== LEFT OUTER WING =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(-5.0f * scale, 3.0f * scale, 0f),      // Top left outer
            new Vector3(-4.8f * scale, 2.5f * scale, 0f),
            new Vector3(-4.5f * scale, 1.8f * scale, 0f),
            new Vector3(-4.0f * scale, 0.8f * scale, 0f),
            new Vector3(-3.5f * scale, -0.5f * scale, 0f),
            new Vector3(-3.2f * scale, -1.5f * scale, 0f),
            new Vector3(-3.0f * scale, -3.0f * scale, 0f)      // Bottom left outer point
        }, ref pointIndex, scale, 30);
        
        // Left inner edge (creating thickness)
        AddDetailedCurve(new Vector3[] {
            new Vector3(-3.0f * scale, -3.0f * scale, 0f),
            new Vector3(-2.5f * scale, -1.8f * scale, 0f),
            new Vector3(-2.2f * scale, -0.5f * scale, 0f),
            new Vector3(-2.0f * scale, 0.5f * scale, 0f),
            new Vector3(-1.8f * scale, 1.5f * scale, 0f)
        }, ref pointIndex, scale, 25);
        
        // ===== LEFT CENTER VALLEY (dip between left and center) =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(-1.8f * scale, 1.5f * scale, 0f),
            new Vector3(-1.5f * scale, 0.3f * scale, 0f),
            new Vector3(-1.2f * scale, -1.0f * scale, 0f),
            new Vector3(-1.0f * scale, -2.0f * scale, 0f)      // Left valley bottom
        }, ref pointIndex, scale, 20);
        
        // ===== CENTER SPIKE (middle prong of the trident) =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(-1.0f * scale, -2.0f * scale, 0f),
            new Vector3(-0.7f * scale, -0.8f * scale, 0f),
            new Vector3(-0.4f * scale, 0.8f * scale, 0f),
            new Vector3(-0.2f * scale, 2.0f * scale, 0f),
            new Vector3(0.0f, 3.5f * scale, 0f),               // Center spike peak
            new Vector3(0.2f * scale, 2.0f * scale, 0f),
            new Vector3(0.4f * scale, 0.8f * scale, 0f),
            new Vector3(0.7f * scale, -0.8f * scale, 0f),
            new Vector3(1.0f * scale, -2.0f * scale, 0f)       // Right valley bottom
        }, ref pointIndex, scale, 35);
        
        // ===== RIGHT CENTER VALLEY (dip between center and right) =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(1.0f * scale, -2.0f * scale, 0f),
            new Vector3(1.2f * scale, -1.0f * scale, 0f),
            new Vector3(1.5f * scale, 0.3f * scale, 0f),
            new Vector3(1.8f * scale, 1.5f * scale, 0f)
        }, ref pointIndex, scale, 20);
        
        // Right inner edge
        AddDetailedCurve(new Vector3[] {
            new Vector3(1.8f * scale, 1.5f * scale, 0f),
            new Vector3(2.0f * scale, 0.5f * scale, 0f),
            new Vector3(2.2f * scale, -0.5f * scale, 0f),
            new Vector3(2.5f * scale, -1.8f * scale, 0f),
            new Vector3(3.0f * scale, -3.0f * scale, 0f)       // Bottom right outer point
        }, ref pointIndex, scale, 25);
        
        // ===== RIGHT OUTER WING =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(3.0f * scale, -3.0f * scale, 0f),
            new Vector3(3.2f * scale, -1.5f * scale, 0f),
            new Vector3(3.5f * scale, -0.5f * scale, 0f),
            new Vector3(4.0f * scale, 0.8f * scale, 0f),
            new Vector3(4.5f * scale, 1.8f * scale, 0f),
            new Vector3(4.8f * scale, 2.5f * scale, 0f),
            new Vector3(5.0f * scale, 3.0f * scale, 0f)        // Top right outer
        }, ref pointIndex, scale, 30);
        
        // ===== TOP CONNECTING LINE (closes the shape) =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(5.0f * scale, 3.0f * scale, 0f),
            new Vector3(3.5f * scale, 3.2f * scale, 0f),
            new Vector3(2.0f * scale, 3.3f * scale, 0f),
            new Vector3(0.0f, 3.8f * scale, 0f),
            new Vector3(-2.0f * scale, 3.3f * scale, 0f),
            new Vector3(-3.5f * scale, 3.2f * scale, 0f),
            new Vector3(-5.0f * scale, 3.0f * scale, 0f)       // Back to start
        }, ref pointIndex, scale, 30);
        
        Debug.Log($"Wu-Tang Logo generated with {pointIndex} points! Wu-Tang Clan ain't nothin' to f*** wit! 🎤");
    }

    private void GenerateMiddleFinger()
    {
        // Middle finger extended, other fingers clenched in fist
        // Front view (pointing at camera)
        
        float scale = radius;
        int pointIndex = 0;
        
        // ===== WRIST/FOREARM BASE =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(-1.5f * scale, -4.0f * scale, 0f),     // Left wrist
            new Vector3(-1.4f * scale, -3.5f * scale, 0f),
            new Vector3(-1.3f * scale, -3.0f * scale, 0f)
        }, ref pointIndex, scale, 15);
        
        // ===== LEFT SIDE OF FIST (pinky side) =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(-1.3f * scale, -3.0f * scale, 0f),
            new Vector3(-1.5f * scale, -2.5f * scale, 0f),
            new Vector3(-1.6f * scale, -2.0f * scale, 0f),
            new Vector3(-1.7f * scale, -1.5f * scale, 0f),
            new Vector3(-1.7f * scale, -1.0f * scale, 0f),     // Side of fist
            new Vector3(-1.6f * scale, -0.5f * scale, 0f)
        }, ref pointIndex, scale, 25);
        
        // ===== PINKY (clenched) =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(-1.6f * scale, -0.5f * scale, 0f),
            new Vector3(-1.5f * scale, -0.2f * scale, 0f),
            new Vector3(-1.3f * scale, 0.0f, 0f),              // Pinky knuckle
            new Vector3(-1.2f * scale, -0.1f * scale, 0f),
            new Vector3(-1.1f * scale, -0.3f * scale, 0f)
        }, ref pointIndex, scale, 18);
        
        // ===== RING FINGER (clenched) =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(-1.1f * scale, -0.3f * scale, 0f),
            new Vector3(-0.9f * scale, -0.1f * scale, 0f),
            new Vector3(-0.7f * scale, 0.1f * scale, 0f),      // Ring knuckle
            new Vector3(-0.6f * scale, 0.0f, 0f),
            new Vector3(-0.5f * scale, -0.2f * scale, 0f)
        }, ref pointIndex, scale, 18);
        
        // ===== MIDDLE FINGER BASE (transition to extended finger) =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(-0.5f * scale, -0.2f * scale, 0f),
            new Vector3(-0.3f * scale, 0.0f, 0f),
            new Vector3(-0.2f * scale, 0.2f * scale, 0f),      // Middle finger base knuckle
            new Vector3(-0.15f * scale, 0.5f * scale, 0f)
        }, ref pointIndex, scale, 15);
        
        // ===== MIDDLE FINGER - LEFT SIDE (extended upward!) =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(-0.15f * scale, 0.5f * scale, 0f),
            new Vector3(-0.2f * scale, 1.0f * scale, 0f),
            new Vector3(-0.22f * scale, 1.5f * scale, 0f),
            new Vector3(-0.23f * scale, 2.0f * scale, 0f),
            new Vector3(-0.24f * scale, 2.5f * scale, 0f),
            new Vector3(-0.25f * scale, 3.0f * scale, 0f),
            new Vector3(-0.25f * scale, 3.5f * scale, 0f),
            new Vector3(-0.24f * scale, 4.0f * scale, 0f)
        }, ref pointIndex, scale, 35);
        
        // ===== MIDDLE FINGER TIP (rounded) =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(-0.24f * scale, 4.0f * scale, 0f),
            new Vector3(-0.20f * scale, 4.3f * scale, 0f),
            new Vector3(-0.10f * scale, 4.5f * scale, 0f),
            new Vector3(0.0f, 4.6f * scale, 0f),               // Tip!
            new Vector3(0.10f * scale, 4.5f * scale, 0f),
            new Vector3(0.20f * scale, 4.3f * scale, 0f),
            new Vector3(0.24f * scale, 4.0f * scale, 0f)
        }, ref pointIndex, scale, 25);
        
        // ===== MIDDLE FINGER - RIGHT SIDE (back down) =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.24f * scale, 4.0f * scale, 0f),
            new Vector3(0.25f * scale, 3.5f * scale, 0f),
            new Vector3(0.25f * scale, 3.0f * scale, 0f),
            new Vector3(0.24f * scale, 2.5f * scale, 0f),
            new Vector3(0.23f * scale, 2.0f * scale, 0f),
            new Vector3(0.22f * scale, 1.5f * scale, 0f),
            new Vector3(0.2f * scale, 1.0f * scale, 0f),
            new Vector3(0.15f * scale, 0.5f * scale, 0f)
        }, ref pointIndex, scale, 35);
        
        // ===== INDEX FINGER (clenched) =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.15f * scale, 0.5f * scale, 0f),
            new Vector3(0.2f * scale, 0.2f * scale, 0f),
            new Vector3(0.3f * scale, 0.0f, 0f),               // Index knuckle
            new Vector3(0.5f * scale, -0.2f * scale, 0f),
            new Vector3(0.6f * scale, 0.0f, 0f),
            new Vector3(0.7f * scale, 0.1f * scale, 0f),
            new Vector3(0.9f * scale, -0.1f * scale, 0f),
            new Vector3(1.1f * scale, -0.3f * scale, 0f)
        }, ref pointIndex, scale, 25);
        
        // ===== THUMB (clenched over fingers) =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(1.1f * scale, -0.3f * scale, 0f),
            new Vector3(1.2f * scale, -0.5f * scale, 0f),
            new Vector3(1.3f * scale, -0.8f * scale, 0f),      // Thumb over knuckles
            new Vector3(1.35f * scale, -1.2f * scale, 0f),
            new Vector3(1.3f * scale, -1.6f * scale, 0f),
            new Vector3(1.2f * scale, -2.0f * scale, 0f)
        }, ref pointIndex, scale, 22);
        
        // ===== RIGHT SIDE OF FIST (index finger side) =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(1.2f * scale, -2.0f * scale, 0f),
            new Vector3(1.4f * scale, -2.3f * scale, 0f),
            new Vector3(1.5f * scale, -2.6f * scale, 0f),
            new Vector3(1.5f * scale, -2.9f * scale, 0f),
            new Vector3(1.3f * scale, -3.0f * scale, 0f)
        }, ref pointIndex, scale, 20);
        
        // ===== RIGHT WRIST =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(1.3f * scale, -3.0f * scale, 0f),
            new Vector3(1.4f * scale, -3.5f * scale, 0f),
            new Vector3(1.5f * scale, -4.0f * scale, 0f)       // Right wrist
        }, ref pointIndex, scale, 15);
        
        // ===== BOTTOM OF WRIST (close the shape) =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(1.5f * scale, -4.0f * scale, 0f),
            new Vector3(1.0f * scale, -4.2f * scale, 0f),
            new Vector3(0.5f * scale, -4.3f * scale, 0f),
            new Vector3(0.0f, -4.3f * scale, 0f),
            new Vector3(-0.5f * scale, -4.3f * scale, 0f),
            new Vector3(-1.0f * scale, -4.2f * scale, 0f),
            new Vector3(-1.5f * scale, -4.0f * scale, 0f)      // Back to start
        }, ref pointIndex, scale, 25);
        
        Debug.Log($"Middle Finger generated with {pointIndex} points! 🖕 Universal gesture of defiance!");
    }

    private void GenerateCannabisLeaf()
    {
        // Botanically accurate Cannabis Sativa leaf with 7 serrated leaflets
        // Based on scientific research: palmate structure, lanceolate leaflets, serrated margins
        
        float scale = radius;
        int pointIndex = 0;
        
        // ===== PETIOLE/STEM (connects leaf to plant) =====
        float petioleWidth = 0.06f * scale;
        float petioleLength = 1.2f * scale;
        
        // Petiole base to rachis
        AddDetailedCurve(new Vector3[] {
            new Vector3(-petioleWidth, -petioleLength - 0.5f * scale, 0f),
            new Vector3(-petioleWidth * 0.9f, -petioleLength, 0f),
            new Vector3(-petioleWidth * 0.7f, -petioleLength * 0.7f, 0f),
            new Vector3(-petioleWidth * 0.5f, -petioleLength * 0.4f, 0f),
            new Vector3(-petioleWidth * 0.3f, 0f, 0f)
        }, ref pointIndex, scale, 20);
        
        // ===== CENTRAL LEAFLET (tallest, most prominent) =====
        // Lanceolate shape: narrow at base, widest in middle, tapering to sharp point
        
        // Left edge ascending with serrations
        AddDetailedCurve(new Vector3[] {
            new Vector3(-petioleWidth * 0.3f, 0f, 0f),
            new Vector3(-0.10f * scale, 0.4f * scale, 0f),
            new Vector3(-0.18f * scale, 0.9f * scale, 0f),    // Serration out
            new Vector3(-0.14f * scale, 1.1f * scale, 0f),    // Serration in
            new Vector3(-0.22f * scale, 1.5f * scale, 0f),    // Serration out
            new Vector3(-0.18f * scale, 1.7f * scale, 0f),    // Serration in
            new Vector3(-0.24f * scale, 2.1f * scale, 0f),    // Serration out (widest point)
            new Vector3(-0.20f * scale, 2.3f * scale, 0f),    // Serration in
            new Vector3(-0.22f * scale, 2.7f * scale, 0f),    // Serration out
            new Vector3(-0.18f * scale, 2.9f * scale, 0f),    // Serration in
            new Vector3(-0.16f * scale, 3.3f * scale, 0f),    // Serration out
            new Vector3(-0.12f * scale, 3.5f * scale, 0f),    // Serration in
            new Vector3(-0.08f * scale, 3.8f * scale, 0f)     // Near tip
        }, ref pointIndex, scale, 45);
        
        // Sharp acuminate tip
        AddDetailedCurve(new Vector3[] {
            new Vector3(-0.08f * scale, 3.8f * scale, 0f),
            new Vector3(-0.04f * scale, 3.95f * scale, 0f),
            new Vector3(0.0f, 4.0f * scale, 0f),              // Apex
            new Vector3(0.04f * scale, 3.95f * scale, 0f),
            new Vector3(0.08f * scale, 3.8f * scale, 0f)
        }, ref pointIndex, scale, 12);
        
        // Right edge descending with serrations
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.08f * scale, 3.8f * scale, 0f),
            new Vector3(0.12f * scale, 3.5f * scale, 0f),     // Serration in
            new Vector3(0.16f * scale, 3.3f * scale, 0f),     // Serration out
            new Vector3(0.18f * scale, 2.9f * scale, 0f),     // Serration in
            new Vector3(0.22f * scale, 2.7f * scale, 0f),     // Serration out
            new Vector3(0.20f * scale, 2.3f * scale, 0f),     // Serration in
            new Vector3(0.24f * scale, 2.1f * scale, 0f),     // Serration out (widest)
            new Vector3(0.18f * scale, 1.7f * scale, 0f),     // Serration in
            new Vector3(0.22f * scale, 1.5f * scale, 0f),     // Serration out
            new Vector3(0.14f * scale, 1.1f * scale, 0f),     // Serration in
            new Vector3(0.18f * scale, 0.9f * scale, 0f),     // Serration out
            new Vector3(0.10f * scale, 0.4f * scale, 0f),
            new Vector3(petioleWidth * 0.3f, 0f, 0f)
        }, ref pointIndex, scale, 45);
        
        // ===== FIRST PAIR (adjacent to center, slightly shorter) =====
        
        // Right leaflet of first pair - outer edge
        AddDetailedCurve(new Vector3[] {
            new Vector3(petioleWidth * 0.3f, 0f, 0f),
            new Vector3(0.28f * scale, 0.1f * scale, 0f),
            new Vector3(0.45f * scale, 0.4f * scale, 0f),
            new Vector3(0.58f * scale, 0.7f * scale, 0f),     // Serration out
            new Vector3(0.54f * scale, 0.9f * scale, 0f),     // Serration in
            new Vector3(0.68f * scale, 1.3f * scale, 0f),     // Serration out
            new Vector3(0.64f * scale, 1.5f * scale, 0f),     // Serration in
            new Vector3(0.76f * scale, 1.9f * scale, 0f),     // Serration out (widest)
            new Vector3(0.72f * scale, 2.1f * scale, 0f),     // Serration in
            new Vector3(0.78f * scale, 2.5f * scale, 0f),     // Serration out
            new Vector3(0.72f * scale, 2.7f * scale, 0f),     // Serration in
            new Vector3(0.68f * scale, 3.0f * scale, 0f)      // Near tip
        }, ref pointIndex, scale, 40);
        
        // Tip
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.68f * scale, 3.0f * scale, 0f),
            new Vector3(0.64f * scale, 3.1f * scale, 0f),
            new Vector3(0.60f * scale, 3.15f * scale, 0f),
            new Vector3(0.56f * scale, 3.1f * scale, 0f),
            new Vector3(0.52f * scale, 3.0f * scale, 0f)
        }, ref pointIndex, scale, 12);
        
        // Inner edge
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.52f * scale, 3.0f * scale, 0f),
            new Vector3(0.48f * scale, 2.7f * scale, 0f),     // Serration in
            new Vector3(0.52f * scale, 2.5f * scale, 0f),     // Serration out
            new Vector3(0.48f * scale, 2.3f * scale, 0f),     // Serration in
            new Vector3(0.52f * scale, 2.1f * scale, 0f),     // Serration out
            new Vector3(0.46f * scale, 1.8f * scale, 0f),     // Serration in
            new Vector3(0.48f * scale, 1.5f * scale, 0f),     // Serration out
            new Vector3(0.42f * scale, 1.2f * scale, 0f),     // Serration in
            new Vector3(0.44f * scale, 0.9f * scale, 0f),     // Serration out
            new Vector3(0.38f * scale, 0.6f * scale, 0f),
            new Vector3(0.32f * scale, 0.3f * scale, 0f)
        }, ref pointIndex, scale, 40);
        
        // Left leaflet of first pair - outer edge (mirrored)
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.32f * scale, 0.3f * scale, 0f),
            new Vector3(-0.28f * scale, 0.1f * scale, 0f),
            new Vector3(-0.45f * scale, 0.4f * scale, 0f),
            new Vector3(-0.58f * scale, 0.7f * scale, 0f),
            new Vector3(-0.54f * scale, 0.9f * scale, 0f),
            new Vector3(-0.68f * scale, 1.3f * scale, 0f),
            new Vector3(-0.64f * scale, 1.5f * scale, 0f),
            new Vector3(-0.76f * scale, 1.9f * scale, 0f),
            new Vector3(-0.72f * scale, 2.1f * scale, 0f),
            new Vector3(-0.78f * scale, 2.5f * scale, 0f),
            new Vector3(-0.72f * scale, 2.7f * scale, 0f),
            new Vector3(-0.68f * scale, 3.0f * scale, 0f)
        }, ref pointIndex, scale, 40);
        
        // Tip
        AddDetailedCurve(new Vector3[] {
            new Vector3(-0.68f * scale, 3.0f * scale, 0f),
            new Vector3(-0.64f * scale, 3.1f * scale, 0f),
            new Vector3(-0.60f * scale, 3.15f * scale, 0f),
            new Vector3(-0.56f * scale, 3.1f * scale, 0f),
            new Vector3(-0.52f * scale, 3.0f * scale, 0f)
        }, ref pointIndex, scale, 12);
        
        // Inner edge
        AddDetailedCurve(new Vector3[] {
            new Vector3(-0.52f * scale, 3.0f * scale, 0f),
            new Vector3(-0.48f * scale, 2.7f * scale, 0f),
            new Vector3(-0.52f * scale, 2.5f * scale, 0f),
            new Vector3(-0.48f * scale, 2.3f * scale, 0f),
            new Vector3(-0.52f * scale, 2.1f * scale, 0f),
            new Vector3(-0.46f * scale, 1.8f * scale, 0f),
            new Vector3(-0.48f * scale, 1.5f * scale, 0f),
            new Vector3(-0.42f * scale, 1.2f * scale, 0f),
            new Vector3(-0.44f * scale, 0.9f * scale, 0f),
            new Vector3(-0.38f * scale, 0.6f * scale, 0f),
            new Vector3(-0.32f * scale, 0.3f * scale, 0f)
        }, ref pointIndex, scale, 40);
        
        // ===== SECOND PAIR (medium length) =====
        
        // Right leaflet of second pair - outer edge
        AddDetailedCurve(new Vector3[] {
            new Vector3(-0.32f * scale, 0.3f * scale, 0f),
            new Vector3(-0.42f * scale, 0.15f * scale, 0f),
            new Vector3(-0.65f * scale, 0.0f, 0f),
            new Vector3(-0.88f * scale, 0.05f * scale, 0f),
            new Vector3(-1.05f * scale, 0.25f * scale, 0f),   // Serration out
            new Vector3(-1.00f * scale, 0.4f * scale, 0f),    // Serration in
            new Vector3(-1.15f * scale, 0.65f * scale, 0f),   // Serration out
            new Vector3(-1.10f * scale, 0.85f * scale, 0f),   // Serration in
            new Vector3(-1.25f * scale, 1.15f * scale, 0f),   // Serration out (widest)
            new Vector3(-1.20f * scale, 1.35f * scale, 0f),   // Serration in
            new Vector3(-1.24f * scale, 1.65f * scale, 0f),   // Serration out
            new Vector3(-1.18f * scale, 1.85f * scale, 0f)    // Near tip
        }, ref pointIndex, scale, 38);
        
        // Tip
        AddDetailedCurve(new Vector3[] {
            new Vector3(-1.18f * scale, 1.85f * scale, 0f),
            new Vector3(-1.14f * scale, 1.95f * scale, 0f),
            new Vector3(-1.10f * scale, 2.0f * scale, 0f),
            new Vector3(-1.06f * scale, 1.95f * scale, 0f),
            new Vector3(-1.02f * scale, 1.85f * scale, 0f)
        }, ref pointIndex, scale, 12);
        
        // Inner edge
        AddDetailedCurve(new Vector3[] {
            new Vector3(-1.02f * scale, 1.85f * scale, 0f),
            new Vector3(-0.98f * scale, 1.65f * scale, 0f),   // Serration in
            new Vector3(-1.02f * scale, 1.45f * scale, 0f),   // Serration out
            new Vector3(-0.96f * scale, 1.25f * scale, 0f),   // Serration in
            new Vector3(-0.98f * scale, 1.05f * scale, 0f),   // Serration out
            new Vector3(-0.92f * scale, 0.85f * scale, 0f),   // Serration in
            new Vector3(-0.94f * scale, 0.65f * scale, 0f),   // Serration out
            new Vector3(-0.86f * scale, 0.45f * scale, 0f),   // Serration in
            new Vector3(-0.78f * scale, 0.25f * scale, 0f),
            new Vector3(-0.62f * scale, 0.05f * scale, 0f),
            new Vector3(-0.48f * scale, -0.05f * scale, 0f)
        }, ref pointIndex, scale, 38);
        
        // Left leaflet of second pair - outer edge (mirrored)
        AddDetailedCurve(new Vector3[] {
            new Vector3(-0.48f * scale, -0.05f * scale, 0f),
            new Vector3(0.42f * scale, 0.15f * scale, 0f),
            new Vector3(0.65f * scale, 0.0f, 0f),
            new Vector3(0.88f * scale, 0.05f * scale, 0f),
            new Vector3(1.05f * scale, 0.25f * scale, 0f),
            new Vector3(1.00f * scale, 0.4f * scale, 0f),
            new Vector3(1.15f * scale, 0.65f * scale, 0f),
            new Vector3(1.10f * scale, 0.85f * scale, 0f),
            new Vector3(1.25f * scale, 1.15f * scale, 0f),
            new Vector3(1.20f * scale, 1.35f * scale, 0f),
            new Vector3(1.24f * scale, 1.65f * scale, 0f),
            new Vector3(1.18f * scale, 1.85f * scale, 0f)
        }, ref pointIndex, scale, 38);
        
        // Tip
        AddDetailedCurve(new Vector3[] {
            new Vector3(1.18f * scale, 1.85f * scale, 0f),
            new Vector3(1.14f * scale, 1.95f * scale, 0f),
            new Vector3(1.10f * scale, 2.0f * scale, 0f),
            new Vector3(1.06f * scale, 1.95f * scale, 0f),
            new Vector3(1.02f * scale, 1.85f * scale, 0f)
        }, ref pointIndex, scale, 12);
        
        // Inner edge
        AddDetailedCurve(new Vector3[] {
            new Vector3(1.02f * scale, 1.85f * scale, 0f),
            new Vector3(0.98f * scale, 1.65f * scale, 0f),
            new Vector3(1.02f * scale, 1.45f * scale, 0f),
            new Vector3(0.96f * scale, 1.25f * scale, 0f),
            new Vector3(0.98f * scale, 1.05f * scale, 0f),
            new Vector3(0.92f * scale, 0.85f * scale, 0f),
            new Vector3(0.94f * scale, 0.65f * scale, 0f),
            new Vector3(0.86f * scale, 0.45f * scale, 0f),
            new Vector3(0.78f * scale, 0.25f * scale, 0f),
            new Vector3(0.62f * scale, 0.05f * scale, 0f),
            new Vector3(0.48f * scale, -0.05f * scale, 0f)
        }, ref pointIndex, scale, 38);
        
        // ===== THIRD PAIR (smallest, basal leaflets) =====
        
        // Right leaflet of third pair - outer edge
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.48f * scale, -0.05f * scale, 0f),
            new Vector3(0.62f * scale, -0.15f * scale, 0f),
            new Vector3(0.85f * scale, -0.25f * scale, 0f),
            new Vector3(1.08f * scale, -0.28f * scale, 0f),
            new Vector3(1.28f * scale, -0.2f * scale, 0f),    // Serration out
            new Vector3(1.24f * scale, -0.05f * scale, 0f),   // Serration in
            new Vector3(1.38f * scale, 0.15f * scale, 0f),    // Serration out
            new Vector3(1.34f * scale, 0.32f * scale, 0f),    // Serration in
            new Vector3(1.44f * scale, 0.52f * scale, 0f),    // Serration out (widest)
            new Vector3(1.38f * scale, 0.68f * scale, 0f),    // Serration in
            new Vector3(1.36f * scale, 0.85f * scale, 0f)     // Near tip
        }, ref pointIndex, scale, 35);
        
        // Tip
        AddDetailedCurve(new Vector3[] {
            new Vector3(1.36f * scale, 0.85f * scale, 0f),
            new Vector3(1.32f * scale, 0.93f * scale, 0f),
            new Vector3(1.28f * scale, 0.98f * scale, 0f),
            new Vector3(1.24f * scale, 0.93f * scale, 0f),
            new Vector3(1.20f * scale, 0.85f * scale, 0f)
        }, ref pointIndex, scale, 10);
        
        // Inner edge
        AddDetailedCurve(new Vector3[] {
            new Vector3(1.20f * scale, 0.85f * scale, 0f),
            new Vector3(1.16f * scale, 0.7f * scale, 0f),     // Serration in
            new Vector3(1.20f * scale, 0.55f * scale, 0f),    // Serration out
            new Vector3(1.14f * scale, 0.4f * scale, 0f),     // Serration in
            new Vector3(1.16f * scale, 0.25f * scale, 0f),    // Serration out
            new Vector3(1.10f * scale, 0.1f * scale, 0f),     // Serration in
            new Vector3(1.08f * scale, -0.05f * scale, 0f),   // Serration out
            new Vector3(1.00f * scale, -0.18f * scale, 0f),   // Serration in
            new Vector3(0.85f * scale, -0.28f * scale, 0f),
            new Vector3(0.68f * scale, -0.32f * scale, 0f),
            new Vector3(0.54f * scale, -0.3f * scale, 0f)
        }, ref pointIndex, scale, 35);
        
        // Left leaflet of third pair - outer edge (mirrored)
        AddDetailedCurve(new Vector3[] {
            new Vector3(0.54f * scale, -0.3f * scale, 0f),
            new Vector3(-0.62f * scale, -0.15f * scale, 0f),
            new Vector3(-0.85f * scale, -0.25f * scale, 0f),
            new Vector3(-1.08f * scale, -0.28f * scale, 0f),
            new Vector3(-1.28f * scale, -0.2f * scale, 0f),
            new Vector3(-1.24f * scale, -0.05f * scale, 0f),
            new Vector3(-1.38f * scale, 0.15f * scale, 0f),
            new Vector3(-1.34f * scale, 0.32f * scale, 0f),
            new Vector3(-1.44f * scale, 0.52f * scale, 0f),
            new Vector3(-1.38f * scale, 0.68f * scale, 0f),
            new Vector3(-1.36f * scale, 0.85f * scale, 0f)
        }, ref pointIndex, scale, 35);
        
        // Tip
        AddDetailedCurve(new Vector3[] {
            new Vector3(-1.36f * scale, 0.85f * scale, 0f),
            new Vector3(-1.32f * scale, 0.93f * scale, 0f),
            new Vector3(-1.28f * scale, 0.98f * scale, 0f),
            new Vector3(-1.24f * scale, 0.93f * scale, 0f),
            new Vector3(-1.20f * scale, 0.85f * scale, 0f)
        }, ref pointIndex, scale, 10);
        
        // Inner edge back to petiole base
        AddDetailedCurve(new Vector3[] {
            new Vector3(-1.20f * scale, 0.85f * scale, 0f),
            new Vector3(-1.16f * scale, 0.7f * scale, 0f),
            new Vector3(-1.20f * scale, 0.55f * scale, 0f),
            new Vector3(-1.14f * scale, 0.4f * scale, 0f),
            new Vector3(-1.16f * scale, 0.25f * scale, 0f),
            new Vector3(-1.10f * scale, 0.1f * scale, 0f),
            new Vector3(-1.08f * scale, -0.05f * scale, 0f),
            new Vector3(-1.00f * scale, -0.18f * scale, 0f),
            new Vector3(-0.85f * scale, -0.28f * scale, 0f),
            new Vector3(-0.68f * scale, -0.32f * scale, 0f),
            new Vector3(-0.54f * scale, -0.3f * scale, 0f)
        }, ref pointIndex, scale, 35);
        
        // ===== CLOSE PATH - Return to petiole base =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(-0.54f * scale, -0.3f * scale, 0f),
            new Vector3(-0.38f * scale, -0.4f * scale, 0f),
            new Vector3(-0.22f * scale, -0.5f * scale, 0f),
            new Vector3(-petioleWidth * 0.5f, -petioleLength * 0.4f, 0f),
            new Vector3(-petioleWidth * 0.7f, -petioleLength * 0.7f, 0f),
            new Vector3(-petioleWidth * 0.9f, -petioleLength, 0f),
            new Vector3(-petioleWidth, -petioleLength - 0.25f * scale, 0f)
        }, ref pointIndex, scale, 25);
        
        // Bottom of petiole
        AddDetailedCurve(new Vector3[] {
            new Vector3(-petioleWidth, -petioleLength - 0.25f * scale, 0f),
            new Vector3(-petioleWidth, -petioleLength - 0.5f * scale, 0f),
            new Vector3(-petioleWidth * 0.5f, -petioleLength - 0.52f * scale, 0f),
            new Vector3(0.0f, -petioleLength - 0.53f * scale, 0f),
            new Vector3(petioleWidth * 0.5f, -petioleLength - 0.52f * scale, 0f),
            new Vector3(petioleWidth, -petioleLength - 0.5f * scale, 0f),
            new Vector3(petioleWidth, -petioleLength - 0.25f * scale, 0f)
        }, ref pointIndex, scale, 18);
        
        // Right side of petiole back up
        AddDetailedCurve(new Vector3[] {
            new Vector3(petioleWidth, -petioleLength - 0.25f * scale, 0f),
            new Vector3(petioleWidth * 0.9f, -petioleLength, 0f),
            new Vector3(petioleWidth * 0.7f, -petioleLength * 0.7f, 0f),
            new Vector3(petioleWidth * 0.5f, -petioleLength * 0.4f, 0f),
            new Vector3(0.22f * scale, -0.5f * scale, 0f),
            new Vector3(0.38f * scale, -0.4f * scale, 0f),
            new Vector3(0.54f * scale, -0.3f * scale, 0f)
        }, ref pointIndex, scale, 25);
        
        Debug.Log($"🌿 Botanically Accurate Cannabis Leaf: {pointIndex} points with 7 lanceolate leaflets, serrated margins, and palmate structure!");
    }

    private void GenerateCountIQ()
    {
        // COUNT IQ in the COOLEST font style imaginable!
        // Using bold, thick letters with decorative serifs and style
        
        float scale = letterScale;
        int pointIndex = 0;
        float spacing = 1.2f; // Much tighter spacing
        float xOffset = -spacing * 3.5f; // Center the text
        
        // ===== LETTER C (in COUNT) =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(xOffset + 0.9f, 0.85f, 0f),
            new Vector3(xOffset + 0.7f, 1.0f, 0f),
            new Vector3(xOffset + 0.3f, 1.0f, 0f),
            new Vector3(xOffset + 0.05f, 0.85f, 0f),
            new Vector3(xOffset + 0.0f, 0.5f, 0f),
            new Vector3(xOffset + 0.05f, 0.15f, 0f),
            new Vector3(xOffset + 0.3f, 0.0f, 0f),
            new Vector3(xOffset + 0.7f, 0.0f, 0f),
            new Vector3(xOffset + 0.9f, 0.15f, 0f)
        }, ref pointIndex, scale, 35);
        
        // C inner curve
        AddDetailedCurve(new Vector3[] {
            new Vector3(xOffset + 0.75f, 0.25f, 0f),
            new Vector3(xOffset + 0.55f, 0.15f, 0f),
            new Vector3(xOffset + 0.35f, 0.15f, 0f),
            new Vector3(xOffset + 0.2f, 0.25f, 0f),
            new Vector3(xOffset + 0.15f, 0.5f, 0f),
            new Vector3(xOffset + 0.2f, 0.75f, 0f),
            new Vector3(xOffset + 0.35f, 0.85f, 0f),
            new Vector3(xOffset + 0.55f, 0.85f, 0f),
            new Vector3(xOffset + 0.75f, 0.75f, 0f)
        }, ref pointIndex, scale, 35);
        
        xOffset += spacing;
        
        // ===== LETTER O (in COUNT) =====
        AddCircle(new Vector3(xOffset + 0.5f, 0.5f, 0f), 0.5f, ref pointIndex, scale, 40);
        AddCircle(new Vector3(xOffset + 0.5f, 0.5f, 0f), 0.35f, ref pointIndex, scale, 35);
        
        xOffset += spacing;
        
        // ===== LETTER U (in COUNT) =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(xOffset + 0.0f, 1.0f, 0f),
            new Vector3(xOffset + 0.0f, 0.3f, 0f),
            new Vector3(xOffset + 0.15f, 0.05f, 0f),
            new Vector3(xOffset + 0.35f, 0.0f, 0f),
            new Vector3(xOffset + 0.65f, 0.0f, 0f),
            new Vector3(xOffset + 0.85f, 0.05f, 0f),
            new Vector3(xOffset + 1.0f, 0.3f, 0f),
            new Vector3(xOffset + 1.0f, 1.0f, 0f)
        }, ref pointIndex, scale, 40);
        
        // U inner
        AddDetailedCurve(new Vector3[] {
            new Vector3(xOffset + 0.85f, 1.0f, 0f),
            new Vector3(xOffset + 0.85f, 0.35f, 0f),
            new Vector3(xOffset + 0.75f, 0.18f, 0f),
            new Vector3(xOffset + 0.5f, 0.15f, 0f),
            new Vector3(xOffset + 0.25f, 0.18f, 0f),
            new Vector3(xOffset + 0.15f, 0.35f, 0f),
            new Vector3(xOffset + 0.15f, 1.0f, 0f)
        }, ref pointIndex, scale, 35);
        
        xOffset += spacing;
        
        // ===== LETTER N (in COUNT) =====
        // Left vertical stroke
        AddDetailedCurve(new Vector3[] {
            new Vector3(xOffset + 0.0f, 0.0f, 0f),
            new Vector3(xOffset + 0.0f, 1.0f, 0f),
            new Vector3(xOffset + 0.15f, 1.0f, 0f),
            new Vector3(xOffset + 0.15f, 0.0f, 0f),
            new Vector3(xOffset + 0.0f, 0.0f, 0f)
        }, ref pointIndex, scale, 20);
        
        // Diagonal stroke
        AddDetailedCurve(new Vector3[] {
            new Vector3(xOffset + 0.0f, 1.0f, 0f),
            new Vector3(xOffset + 0.15f, 1.0f, 0f),
            new Vector3(xOffset + 1.0f, 0.0f, 0f),
            new Vector3(xOffset + 0.85f, 0.0f, 0f),
            new Vector3(xOffset + 0.0f, 1.0f, 0f)
        }, ref pointIndex, scale, 25);
        
        // Right vertical stroke
        AddDetailedCurve(new Vector3[] {
            new Vector3(xOffset + 0.85f, 0.0f, 0f),
            new Vector3(xOffset + 1.0f, 0.0f, 0f),
            new Vector3(xOffset + 1.0f, 1.0f, 0f),
            new Vector3(xOffset + 0.85f, 1.0f, 0f),
            new Vector3(xOffset + 0.85f, 0.0f, 0f)
        }, ref pointIndex, scale, 20);
        
        xOffset += spacing;
        
        // ===== LETTER T (in COUNT) =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(xOffset + 0.0f, 1.0f, 0f),
            new Vector3(xOffset + 1.0f, 1.0f, 0f),
            new Vector3(xOffset + 1.0f, 0.85f, 0f),
            new Vector3(xOffset + 0.575f, 0.85f, 0f),
            new Vector3(xOffset + 0.575f, 0.0f, 0f),
            new Vector3(xOffset + 0.425f, 0.0f, 0f),
            new Vector3(xOffset + 0.425f, 0.85f, 0f),
            new Vector3(xOffset + 0.0f, 0.85f, 0f),
            new Vector3(xOffset + 0.0f, 1.0f, 0f)
        }, ref pointIndex, scale, 40);
        
        xOffset += spacing * 1.5f; // Extra space between words
        
        // ===== LETTER I (in IQ) =====
        AddDetailedCurve(new Vector3[] {
            new Vector3(xOffset + 0.15f, 1.0f, 0f),
            new Vector3(xOffset + 0.85f, 1.0f, 0f),
            new Vector3(xOffset + 0.85f, 0.85f, 0f),
            new Vector3(xOffset + 0.575f, 0.85f, 0f),
            new Vector3(xOffset + 0.575f, 0.15f, 0f),
            new Vector3(xOffset + 0.85f, 0.15f, 0f),
            new Vector3(xOffset + 0.85f, 0.0f, 0f),
            new Vector3(xOffset + 0.15f, 0.0f, 0f),
            new Vector3(xOffset + 0.15f, 0.15f, 0f),
            new Vector3(xOffset + 0.425f, 0.15f, 0f),
            new Vector3(xOffset + 0.425f, 0.85f, 0f),
            new Vector3(xOffset + 0.15f, 0.85f, 0f),
            new Vector3(xOffset + 0.15f, 1.0f, 0f)
        }, ref pointIndex, scale, 45);
        
        xOffset += spacing;
        
        // ===== LETTER Q (in IQ) - Circle with tail =====
        AddCircle(new Vector3(xOffset + 0.5f, 0.5f, 0f), 0.5f, ref pointIndex, scale, 40);
        AddCircle(new Vector3(xOffset + 0.5f, 0.5f, 0f), 0.35f, ref pointIndex, scale, 35);
        
        // Q tail (descender making it cool)
        AddDetailedCurve(new Vector3[] {
            new Vector3(xOffset + 0.65f, 0.25f, 0f),
            new Vector3(xOffset + 0.8f, 0.1f, 0f),
            new Vector3(xOffset + 0.95f, -0.1f, 0f),
            new Vector3(xOffset + 1.05f, -0.25f, 0f),
            new Vector3(xOffset + 1.0f, -0.3f, 0f),
            new Vector3(xOffset + 0.85f, -0.18f, 0f),
            new Vector3(xOffset + 0.7f, -0.05f, 0f),
            new Vector3(xOffset + 0.6f, 0.15f, 0f)
        }, ref pointIndex, scale, 30);
        
        Debug.Log($"🔥 COUNT IQ generated with {pointIndex} points in the COOLEST FONT EVER! 🔥");
    }
}
