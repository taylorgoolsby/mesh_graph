using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Nodes are used during runtime. For best performance, they hold references to other
// nodes they are connected to (constant time neighbor lookup). 
// This creates circular referencing so Unity does not know how to serialize them.
public class VertexNode : GeometryNode {
    public int index; // index of the first unique occurrence in mesh.vertices

    public List<EdgeNode> edgeNodes;
    public List<TriangleNode> triangleNodes;

    public VertexNode() {
        index = 0;
        position = Vector3.zero;
        edgeNodes = new List<EdgeNode>();
        triangleNodes = new List<TriangleNode>();
    }

    /*
     * Average of adjacent triangle normals. (not area weighted)
     *
     * Alteratively it could use normal data from mesh data.
     */
    public Vector3 GetNormal() {
        Vector3 sum = Vector3.zero;

        foreach (TriangleNode triangle in triangleNodes) {
            sum += triangle.GetNormal();
        }

        if (sum.magnitude != 0) {
            return sum.normalized;
        } else {
            // This happens in cases like if two triangles are anti-planar, but share a vertex.
            // There may be other reason this happens though.

            // Returning 0 is not the correct solution, but good enough for now.
            return Vector3.zero;
        }
    }

    public int OriginalIndex() {
        return triangleNodes[0].IndexOfCorrespondingMeshVertex(this);
    }
}
