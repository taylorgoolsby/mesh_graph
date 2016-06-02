using UnityEngine;
using System.Collections;

// Nodes are used during runtime. For best performance, they hold references to other
// nodes they are connected to (constant time neighbor lookup). 
// This creates circular referencing so Unity does not know how to serialize them.
public class EdgeNode : GeometryNode {
    public VertexNode[] vertexNodes;
    public TriangleNode[] triangleNodes;

    public Vector3 tail {
        get {
            return vertexNodes[0].position;
        }
        set {
            vertexNodes[0].position = value;
        }
    }

    public Vector3 head {
        get {
            return vertexNodes[1].position;
        }
        set {
            vertexNodes[1].position = value;
        }
    }

    public EdgeNode() {
        vertexNodes = new VertexNode[2];
        triangleNodes = new TriangleNode[2];
    }

    public VertexNode Complement(VertexNode v) {
        if (vertexNodes[0] == v) {
            return vertexNodes[1];
        } else if (vertexNodes[1] == v) {
            return vertexNodes[0];
        } else {
            Debug.LogError("Edge node does not contain the vertex node.");
            return null;
        }
    }

    public TriangleNode Complement(TriangleNode t) {
        if (GetNumberOfTriangles() == 2) {
            if (triangleNodes[0] == t) {
                return triangleNodes[1];
            } else if (triangleNodes[1] == t) {
                return triangleNodes[0];
            } else {
                Debug.LogError("Triangle node does not contain the triangle node.");
                return null;
            }
        }
        if (GetNumberOfTriangles() == 1) {
            return null;
        } else {
            Debug.LogError("Unhandled number of triangles.");
            return null;
        }
    }

    public bool Contains(VertexNode v) {
        if (v.id == vertexNodes[0].id)
            return true;
        if (v.id == vertexNodes[1].id)
            return true;
        return false;
    }

    public bool Contains(TriangleNode t) {
        if (t.id == triangleNodes[0].id)
            return true;
        if (t.id == triangleNodes[1].id)
            return true;
        return false;
    }

    /*
     * Returns true if the distance of the point from the edge is very small.
     */
    public bool Contains(Vector3 point) {
        // Check line containing edge contains point.
        Vector3 projected = ProjectOnLine(point, GetParallel(), vertexNodes[0].position);
        Vector3 displacement = projected - point;
        if (displacement.magnitude > Globals.EPSILON) {
            return false;
        }

        // Check if point is between edge endpoints.
        Vector3 tailToPoint = point - tail;
        Vector3 headToPoint = point - head;
        if (Vector3.Dot(tailToPoint, headToPoint) < 0) {
            // Vectors are opposing.
            return true;
        }

        // Otherwise check if the point is very close to the endpoints.
        if (tailToPoint.magnitude < Globals.EPSILON
            || headToPoint.magnitude < Globals.EPSILON) {
            return true;
        }

        return false;
    }

    public int GetNumberOfTriangles() {
        if (triangleNodes[1] == null)
            return 1;
        else if (triangleNodes[0] == null)
            return 0;
        else
            return 2;
    }

    /*
     * A ball hits a wall and bounces off. The ball and wall do not exchange energy.
     * They spend zero (infinitesimal) seconds in contact with each other.
     * In an instant the component of the ball's velocity perpendicular to the wall flips.
     */
    public Vector3 BounceVelocity(Vector3 velocity) {
        // Find the wall to bounce off of.
        Vector3 wallNormal = GetNormal(); // ComputeWall() must be normalized unless you need the magnitude.

        // Find the component of velocity tangential to the wall.
        Vector3 tangent = Vector3.ProjectOnPlane(velocity, wallNormal);

        // Find the component of velocity perpendicular to the wall.
        Vector3 perpen = velocity - tangent; // using algebra on resultant = tanget + perpendicular.

        // Flip the perpendicular then return.
        return tangent - perpen;

        // TODO:
        // Use ComputeWall().magnitude to affect the strength of the bounce. (weak bounce for planar triangles)
    }

    /*
     * Finds the normal of the edge.
     *
     * An edge's normal depnds on it's adjacent triangles.
     * Similar to how a vertex normal is computed as the average of adjacent triangles,
     * the edge normal is the average of adjacent triangles.
     */
    public Vector3 GetNormal() {
        // In the limit as two triangle fold along the edge from an open configuration to a folded flat configuration,
        // The edge normal starts as inline with the triangle normals,
        // then at the end it points away from the triangle.

        // The angle swept by the folding triangle is twice the angle swept by the edge normal.

        // Determine if there are 2 triangles
        if (triangleNodes[1] == null) {
            // There is one adjacent triangle.
            // Or you could say the front and the back of the only triangle should be considered as two triangles.
            Vector3 normal = -GetEdgeToComplementary(triangleNodes[0]);
            return normal.normalized;
        } else {
            // There are two adjacent triangles.

            // Find the angle between the two triangles' normals.
            Vector3 triA = triangleNodes[0].GetNormal();
            Vector3 triB = triangleNodes[1].GetNormal();
            float angle = Vector3.Angle(triA, triB);
            
            // Find axis of folding rotation.
            Vector3 axis = Vector3.Cross(triA, triB).normalized;

            // triA to triB is double the rotation of triA to edge normal.
            Quaternion rotation = Quaternion.AngleAxis(0.5f * angle, axis); // notice half angle

            // Use half angle rotation to find the edge normal.
            Vector3 normal = rotation * triA; // since triA and triB are equal length, this is the bisector unless they are anti-parallel.

            return normal.normalized;
        }

    }

    /*
     * Returns the unit vector that is perpendicular to the edge and
     * points from the edge to the third point in the triangle containing this edge.
     */
    private Vector3 GetEdgeToComplementary(TriangleNode t) {
        if (!Contains(t))
            return Vector3.zero;

        Vector3 complementary = t.GetComplementVertexNode(this).position;
        Vector3 tangent = vertexNodes[0].position - vertexNodes[1].position;
        Vector3 perpen = complementary - vertexNodes[1].position;
        Vector3 projection = Vector3.Project(perpen, tangent);
        Vector3 edgeToComplementary = complementary - (vertexNodes[1].position + projection);

        return edgeToComplementary;
    }

    public Vector3 AverageVertexPosition() {
        Vector3 result = Vector3.zero;
        result += vertexNodes[0].position;
        result += vertexNodes[1].position;
        return result / 2;
    }

    /*
     * Given an edge and point position, this method will return a quaternion that can be applied to the edge-point system
     * to transform it so that the edge is aligned with the x axis and the point lies within the negative y region.
     */
    private static Quaternion SimplifyBasisRotation(EdgeNode edge, Vector3 position) {
        // Translate 3 point system so that one of the edge vertices lies at the origin.
        position -= edge.vertexNodes[0].position;
        Vector3 vert1 = Vector3.zero;
        Vector3 vert2 = edge.vertexNodes[1].position - edge.vertexNodes[0].position;

        // Define some variables for the next step (for readability).
        Vector3 edgeDirection = vert2.normalized;
        Vector3 edgeOrigin = vert1;

        // Make rotation so that the edge normal coincides with <0, 1>.
        Vector3 projectedOnEdge = ProjectOnLine(position, edgeDirection, edgeOrigin);
        Vector3 edgeNormal = (projectedOnEdge - position).normalized;
        Quaternion rotationUp = Quaternion.FromToRotation(edgeNormal, Vector3.up);

        // up rotation needs to be applied in order to find the right rotation.
        vert2 = rotationUp * vert2; // now vert2 lies in xz plane.

        // Make rotation so that vert2 lies on the x axis.
        Quaternion rotationRight = Quaternion.FromToRotation(vert2, Vector3.right);

        // combine rotations and return
        return rotationRight * rotationUp;
    }

    private static Vector3 ProjectOnLine(Vector3 point, Vector3 lineDirection, Vector3 lineOrigin) {
        // Translate everything so that the ray goes through the origin.
        point -= lineOrigin;

        point = Vector3.Project(point, lineDirection);

        // Translate back
        return point + lineOrigin;
    }

    /*
     * Returns a vector that is parallel this edge. (normalized)
     */
    public Vector3 GetParallel() {
        return (vertexNodes[1].position - vertexNodes[0].position).normalized;
    }

    /*
     * Static version of the instance version of the same name
     */
    public static Vector3 GetParallel(Vector3[] vertexPositions) {
        return (vertexPositions[1] - vertexPositions[0]).normalized;
    }

    public bool IsPointInEdge(Vector3 point) {
        return EdgeNode.IsPointInEdge(point, this);
    }

    public static bool IsPointInEdge(Vector3 point, EdgeNode edge) {
        Vector3[] vertices = new Vector3[2];
        vertices[0] = edge.vertexNodes[0].position;
        vertices[1] = edge.vertexNodes[1].position;
        return IsPointInEdge(point, vertices);
    }

    public static bool IsPointInEdge(Vector3 point, Vector3[] vertices) {
        // Center the edge so that it contains the origin.
        Vector3 offset = vertices[0];

        // Project the point onto the line.
        Vector3 point_proj = Vector3.Project(point - offset, GetParallel(vertices));

        // Undo centering
        point_proj += offset;

        // There should be no difference in magnitude.
        if ((point_proj - point).magnitude < Globals.EPSILON) {
            // no difference
        } else {
            return false;
        }

        // Check if point is within edge segment of the line.
        float dot = Vector3.Dot(vertices[0] - point, vertices[1] - point);
        if (dot > 0) {
            // dot positive means the vectors are in the same direction.
            return false;
        } else {
            // vectors in opposite direction means the point is in between them.
            return true;
        }
    }

    public Vector3[] GetVertexPositions() {
        return GetVertexPositions(this);
    }

    public static Vector3[] GetVertexPositions(EdgeNode edge) {
        Vector3[] result = new Vector3[2];
        result[0] = edge.vertexNodes[0].position;
        result[1] = edge.vertexNodes[1].position;
        return result;
    }
}
