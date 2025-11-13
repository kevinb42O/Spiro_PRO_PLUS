using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AutoPathTracer : MonoBehaviour
{
    [Header("Auto-assign path points to SpirographRoller")]
    [Tooltip("The SpirographRoller component to assign path points to")]
    public SpirographRoller spirographRoller;
    
    [Header("Settings")]
    [Tooltip("If true, will automatically run on Start")]
    public bool autoAssignOnStart = true;
    
    [Tooltip("Method to trace boundary: Convex Hull (outer only) or Nearest Neighbor (all corners)")]
    public enum TracingMethod { ConvexHull, NearestNeighbor }
    public TracingMethod tracingMethod = TracingMethod.NearestNeighbor;
    
    void Start()
    {
        if (autoAssignOnStart)
        {
            AssignPathPoints();
        }
    }
    
    [ContextMenu("Assign Path Points")]
    public void AssignPathPoints()
    {
        // Find SpirographRoller if not assigned
        if (spirographRoller == null)
        {
            spirographRoller = FindFirstObjectByType<SpirographRoller>();
            if (spirographRoller == null)
            {
                Debug.LogError("No SpirographRoller found in scene!");
                return;
            }
        }
        
        // Get all child transforms
        Transform[] children = GetComponentsInChildren<Transform>();
        List<Transform> childList = new List<Transform>();
        
        foreach (Transform child in children)
        {
            if (child != transform) // Skip self
            {
                childList.Add(child);
            }
        }
        
        if (childList.Count == 0)
        {
            Debug.LogWarning("No child objects found! Add child objects to trace their positions.");
            return;
        }
        
        Transform[] pathPoints;
        
        if (tracingMethod == TracingMethod.ConvexHull && childList.Count >= 3)
        {
            // Use Graham Scan to find convex hull (outer boundary only)
            pathPoints = ComputeConvexHull(childList);
        }
        else
        {
            // Use nearest neighbor to trace all boundary points
            pathPoints = TraceByNearestNeighbor(childList);
        }
        
        // Assign to SpirographRoller
        spirographRoller.pathPoints = pathPoints;
        
        Debug.Log($"✓ Assigned {pathPoints.Length} path points using {tracingMethod} method");
    }
    
    Transform[] ComputeConvexHull(List<Transform> points)
    {
        if (points.Count < 3) return points.ToArray();
        
        // Convert to 2D (using X-Z plane, ignoring Y)
        List<Vector2> points2D = points.Select(t => new Vector2(t.position.x, t.position.z)).ToList();
        
        // Find lowest Y point (or leftmost if tied)
        int lowestIndex = 0;
        for (int i = 1; i < points2D.Count; i++)
        {
            if (points2D[i].y < points2D[lowestIndex].y || 
                (points2D[i].y == points2D[lowestIndex].y && points2D[i].x < points2D[lowestIndex].x))
            {
                lowestIndex = i;
            }
        }
        
        Vector2 pivot = points2D[lowestIndex];
        Transform pivotTransform = points[lowestIndex];
        
        // Create list of points with their angles from pivot
        List<(Transform transform, float angle, float distance)> sortedPoints = new List<(Transform, float, float)>();
        
        for (int i = 0; i < points.Count; i++)
        {
            if (i == lowestIndex) continue;
            
            Vector2 direction = points2D[i] - pivot;
            float angle = Mathf.Atan2(direction.y, direction.x);
            float distance = direction.magnitude;
            sortedPoints.Add((points[i], angle, distance));
        }
        
        // Sort by angle, if same angle keep furthest point
        sortedPoints.Sort((a, b) =>
        {
            int angleCompare = a.angle.CompareTo(b.angle);
            if (angleCompare == 0)
                return b.distance.CompareTo(a.distance); // Furthest first
            return angleCompare;
        });
        
        // Remove points with same angle (keep only furthest)
        List<(Transform transform, float angle, float distance)> uniqueAngles = new List<(Transform, float, float)>();
        for (int i = 0; i < sortedPoints.Count; i++)
        {
            if (i == 0 || !Mathf.Approximately(sortedPoints[i].angle, sortedPoints[i - 1].angle))
            {
                uniqueAngles.Add(sortedPoints[i]);
            }
        }
        
        // Graham Scan
        Stack<Transform> hull = new Stack<Transform>();
        hull.Push(pivotTransform);
        
        if (uniqueAngles.Count > 0)
            hull.Push(uniqueAngles[0].transform);
        
        for (int i = 1; i < uniqueAngles.Count; i++)
        {
            Transform top = hull.Pop();
            
            while (hull.Count > 0 && !IsLeftTurn(
                new Vector2(hull.Peek().position.x, hull.Peek().position.z),
                new Vector2(top.position.x, top.position.z),
                new Vector2(uniqueAngles[i].transform.position.x, uniqueAngles[i].transform.position.z)))
            {
                top = hull.Pop();
            }
            
            hull.Push(top);
            hull.Push(uniqueAngles[i].transform);
        }
        
        // Convert stack to array in correct order
        Transform[] result = hull.ToArray();
        System.Array.Reverse(result);
        
        return result;
    }
    
    bool IsLeftTurn(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        // Cross product: (p2 - p1) × (p3 - p1)
        float cross = (p2.x - p1.x) * (p3.y - p1.y) - (p2.y - p1.y) * (p3.x - p1.x);
        return cross > 0;
    }
    
    Transform[] TraceByNearestNeighbor(List<Transform> points)
    {
        if (points.Count == 0) return new Transform[0];
        if (points.Count == 1) return points.ToArray();
        
        // Find starting point (leftmost, then lowest if tied)
        Transform start = points[0];
        foreach (Transform t in points)
        {
            Vector3 pos = t.position;
            Vector3 startPos = start.position;
            if (pos.x < startPos.x || (Mathf.Approximately(pos.x, startPos.x) && pos.z < startPos.z))
            {
                start = t;
            }
        }
        
        List<Transform> ordered = new List<Transform>();
        HashSet<Transform> visited = new HashSet<Transform>();
        Transform current = start;
        
        while (visited.Count < points.Count)
        {
            ordered.Add(current);
            visited.Add(current);
            
            if (visited.Count == points.Count) break;
            
            // Find nearest unvisited neighbor
            Transform nearest = null;
            float minDistance = float.MaxValue;
            
            foreach (Transform candidate in points)
            {
                if (visited.Contains(candidate)) continue;
                
                float distance = Vector3.Distance(current.position, candidate.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = candidate;
                }
            }
            
            current = nearest;
        }
        
        return ordered.ToArray();
    }
    
    Transform[] SortByAngleFromCenter(List<Transform> points)
    {
        // Calculate center
        Vector3 center = Vector3.zero;
        foreach (Transform t in points)
            center += t.position;
        center /= points.Count;
        
        // Sort by angle from center
        return points.OrderBy(t =>
        {
            Vector3 direction = t.position - center;
            return Mathf.Atan2(direction.z, direction.x);
        }).ToArray();
    }
    
    void OnDrawGizmos()
    {
        // Visual preview of the path in editor
        List<Transform> childList = new List<Transform>();
        Transform[] children = GetComponentsInChildren<Transform>();
        
        foreach (Transform child in children)
        {
            if (child != transform)
            {
                childList.Add(child);
            }
        }
        
        if (childList.Count == 0) return;
        
        Transform[] points;
        
        if (tracingMethod == TracingMethod.ConvexHull && childList.Count >= 3)
        {
            points = ComputeConvexHull(childList);
        }
        else
        {
            points = TraceByNearestNeighbor(childList);
        }
        
        // Draw path preview
        Gizmos.color = Color.cyan;
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] != null)
            {
                // Draw sphere at each point
                Gizmos.DrawWireSphere(points[i].position, 0.1f);
                
                // Draw line to next point
                if (i < points.Length - 1 && points[i + 1] != null)
                {
                    Gizmos.DrawLine(points[i].position, points[i + 1].position);
                }
                
                // Draw line back to start (closing the loop)
                if (i == points.Length - 1 && points.Length > 0)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(points[i].position, points[0].position);
                }
            }
        }
    }
}
