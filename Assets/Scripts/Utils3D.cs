using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils3D 
{
    
    public static float SqrDistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        float dist1 = Vector3.SqrMagnitude(ProjectPointLine(point, lineStart, lineEnd) - point);
        return dist1;
    }

    public static float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        float dist1 = Vector3.Magnitude(ProjectPointLine(point, lineStart, lineEnd) - point);
        return dist1;
    }

    public static Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 rhs = point - lineStart;
        Vector3 vector2 = lineEnd - lineStart;
        float magnitude = vector2.magnitude;
        Vector3 lhs = vector2;
        if (magnitude > 1E-06f)
        {
            lhs = (Vector3)(lhs / magnitude);
        }
        float num2 = Mathf.Clamp(Vector3.Dot(lhs, rhs), 0f, magnitude);
        return (lineStart + ((Vector3)(lhs * num2)));
    }

    public static float DistancePointRay(Vector3 rayOrigin, Vector3 rayDirection, Vector3 point)
    {
        return Vector3.Cross(rayDirection, point - rayOrigin).magnitude;
    }

    public static float DistancePointRay(Ray ray, Vector3 point)
    {
        return DistancePointRay(ray.origin, ray.direction, point);
    }

    public static Vector3 ProjectPointRay(Ray ray, Vector3 point)
    {
        return ray.origin + ray.direction * Vector3.Dot(ray.direction, point - ray.origin);
    }


    // Debug - function for drawing lines in build
    public static GameObject DrawLine(Vector3 start, Vector3 end, Color color = default(Color), Transform parentObj = null, float duration = -1) //180f)
    {
        if (start == null || end == null)
            return null;

        GameObject myLine = new GameObject("DebugLine");
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        //myLine.tag = "Invisible";
        myLine.transform.parent = parentObj; // debugPlanesObj;
        //myLine.layer = productRectLineLayerMask;

        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.1f; // 0.005f; // 0.001f;
        lr.endWidth = 0.1f; //0.005f; // 0.001f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        if (duration > 0)
            UnityEngine.Object.Destroy(myLine, duration);

        return myLine;
    }

    public static GameObject DrawLine(Vector3 start, Vector3 direction, float length, Color color = default(Color), Transform parentObj = null, float duration = -1)
    {
        Vector3 end = start + direction * length;
        return DrawLine(start, end, color, parentObj, duration);
    }
}
