using System;
using System.Numerics;

public class RayIntersect
{
    public static bool RayIntersectsSegment(Vector2 rayDirection, Vector2 pointA, Vector2 pointB, out float distance)
    {
        distance = 0;

        // The origin of the ray
        Vector2 rayOrigin = Vector2.Zero;

        // Calculate direction vectors
        Vector2 v1 = rayOrigin - pointA;
        Vector2 v2 = pointB - pointA;
        Vector2 v3 = new Vector2(-rayDirection.Y, rayDirection.X);

        // Check if the line and ray are parallel
        float dot = Vector2.Dot(v2, v3);
        if (Math.Abs(dot) < float.Epsilon)
        {
            return false; // They are parallel so they don't intersect
        }

        // Calculate the t and u values to find the intersection point
        float t = Cross(v2, v1) / dot;
        float u = Vector2.Dot(v1, v3) / dot;

        // Check if the intersection point is on the line segment and in the ray's direction
        if (t >= 0 && u >= 0 && u <= 1)
        {
            Vector2 intersection = rayOrigin + t * rayDirection;
            distance = Vector2.Distance(rayOrigin, intersection);
            return true;
        }

        return false;
    }

    // Helper method to calculate the cross product of two 2D vectors, which is not built into Vector2
    public static float Cross(Vector2 v1, Vector2 v2)
    {
        return v1.X * v2.Y - v1.Y * v2.X;
    }
}
