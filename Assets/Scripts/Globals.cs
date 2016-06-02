using UnityEngine;
using System.Collections;

public class Globals : ScriptableObject {
    public static readonly float EPSILON = 0.001f;

    /*
     * Given a line defined by a vector parallel to the line and a point on the line,
     * and given a point to test,
     * returns the smallest distance the test point is from the line.
     */
    public static float DistanceFromLine(Vector3 point, Vector3 lineParallel, Vector3 linePoint) {
        Vector3 projected = ProjectOnLine(point, lineParallel, linePoint);
        return (projected - point).magnitude;
    }

    /*
     * Draws an arrow using Debug.DrawRay.
     * The tip of an arrow is distinguishable from the other end.
     */
    public static void DrawArrow(Vector3 origin, Vector3 vector, Color color) {
        // Draw main stem
        Debug.DrawRay(origin, vector, color);

        // Find camera position.
        Camera cam = Camera.current;
        if (cam != null) {
            // Find direction orthogonal to stem, and orthogonal to the camera forward vector.
            Vector3 ortho = Vector3.Cross(vector, cam.transform.forward); // ortho.magnitude == vector.magnitude

            // Find the end points for the wings.
            Vector3 wing1 = origin + 0.9f * vector + 0.1f * ortho;
            Vector3 wing2 = origin + 0.9f * vector - 0.1f * ortho;

            // Draw the wings.
            Vector3 tip = origin + vector;
            Debug.DrawLine(tip, wing1, color);
            Debug.DrawLine(tip, wing2, color);
        } else {
            //Debug.Log("Can't draw vector because Camera.current is null.");
        }
    }

    /*
     * Given a line defined by a vector parallel to the line and a point on the line,
     * and given a point to test,
     * returns the projection of the test point onto the line.
     */
    private static Vector3 ProjectOnLine(Vector3 point, Vector3 lineDirection, Vector3 lineOrigin) {
        // Translate everything so that the ray goes through the origin.
        point -= lineOrigin;

        point = Vector3.Project(point, lineDirection);

        // Translate back
        return point + lineOrigin;
    }
}
