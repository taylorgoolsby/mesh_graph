using UnityEngine;
using System.Collections;

// Nodes are used during runtime. For best performance, they hold references to other
// nodes they are connected to (constant time neighbor lookup). 
// This creates circular referencing so Unity does not know how to serialize them.
public class TriangleNode : GeometryNode {
    public int[] originalVertices; // indices in mesh.vertices

    public EdgeNode[] edgeNodes;
    public VertexNode[] vertexNodes;

    public TriangleNode() {
        originalVertices = new int[3];
        edgeNodes = new EdgeNode[3];
        vertexNodes = new VertexNode[3];
    }

    public int IndexOfCorrespondingMeshVertex(VertexNode v) {
        for (int i = 0; i < 3; i++) {
            if (v == vertexNodes[i]) {
                return originalVertices[i];
            }
        }
        return -1;
    }

    public VertexNode GetComplementVertexNode(EdgeNode e) {
        if (!Contains(e))
            return null;

        for (int i = 0; i < 3; i++) {
            if (!e.Contains(vertexNodes[i]))
                return vertexNodes[i];
        }

        return null;
    }

    public bool Contains(EdgeNode e) {
        for (int i = 0; i < 3; i++) {
            if (e.id == edgeNodes[i].id)
                return true;
        }
        return false;
    }

    public bool Contains(VertexNode v) {
        for (int i = 0; i < 3; i++) {
            if (v.id == vertexNodes[i].id) {
                return true;
            }
        }
        return false;
    }

    public bool Contains(Vector3 point) {
        return IsPointInTriangle(point);
    }

    public Vector3 AverageVertexPosition() {
        Vector3 result = Vector3.zero;
        result += vertexNodes[0].position;
        result += vertexNodes[1].position;
        result += vertexNodes[2].position;
        return result / 3;
    }

    /*
     * This should be the same normal used in the mesh renderer and shader.
     */
    public Vector3 GetNormal() {
        Vector3 a = vertexNodes[1].position - vertexNodes[0].position;
        Vector3 b = vertexNodes[2].position - vertexNodes[0].position;
        Vector3 c = Vector3.Cross(a, b);
        return c.normalized;
    }

    /*
     * Uses left hand rule to find normal of triangle.
     */
    public static Vector3 GetNormal(Vector3[] vertexPositions) {
        Vector3 a = vertexPositions[1] - vertexPositions[0];
        Vector3 b = vertexPositions[2] - vertexPositions[0];
        Vector3 c = Vector3.Cross(a, b);
        return c.normalized;
    }

    public bool IsPointInTriangle(Vector3 point) {
        Vector3[] vertices = new Vector3[3];
        vertices[0] = vertexNodes[0].position;
        vertices[1] = vertexNodes[1].position;
        vertices[2] = vertexNodes[2].position;
        return TriangleNode.IsPointInTriangle(point, vertices);
    }

    public static bool IsPointInTriangle(Vector3 point, Vector3[] vertices) {
        // Find normal of triangle based of order of appearance in `vertices` using left hand rule.
        Vector3 normal = TriangleNode.GetNormal(vertices);

        // Center the triangle plane on the origin (because Vector3.ProjectOnPlane assumes the plane contains the origin).
        Vector3 offset = vertices[0];

        // Project point onto triangle plane.
        Vector3 point_proj = Vector3.ProjectOnPlane(point - offset, normal);

        // Undo centering
        point_proj += offset;

        // There should be no difference if the point is in the plane.
        if ((point_proj - point).magnitude < Globals.EPSILON) {
            // no difference
        } else {
            return false;
        }

        // Check if point is within bounds using cross product method. 
        Vector3 cross1 = Vector3.Cross(vertices[0] - point, vertices[1] - point).normalized;
        Vector3 cross2 = Vector3.Cross(vertices[1] - point, vertices[2] - point).normalized;
        Vector3 cross3 = Vector3.Cross(vertices[2] - point, vertices[0] - point).normalized;

        // All three should be in same direction, so their sum should be of length 3.
        Vector3 crossSum = cross1 + cross2 + cross3;
        if (crossSum.magnitude > 2.5f) { // 2.5 instead of 3.0 because of rounding error. Length is discrete, 3 or 2, but not in between.
            // Point is in triangle.
            return true;
        } else {
            return false;
        }
    }

    public Vector3[] GetVertexPositions() {
        return GetVertexPositions(this);
    }

    public static Vector3[] GetVertexPositions(TriangleNode tri) {
        Vector3[] result = new Vector3[3];
        for (int i = 0; i < 3; i++) {
            result[i] = tri.vertexNodes[i].position;
        }
        return result;
    }
}
