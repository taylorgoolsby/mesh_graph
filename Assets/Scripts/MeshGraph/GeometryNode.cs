using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeometryNode {
    public int id; // Used for serialization.
    public Vector3 position; // average of node's vertices

    /*
     * Returns a list of neighboring nodes.
     */
    public List<GeometryNode> GetNeighbors() {
        List<GeometryNode> result = new List<GeometryNode>();

        // Get node type
        if (GetType() == typeof(TriangleNode)) {
            // Cast to triangle node.
            TriangleNode tri = (TriangleNode) this;

            // Add neighbors
            foreach (EdgeNode edge in tri.edgeNodes) {
                result.Add(edge);
            }

            foreach (VertexNode vert in tri.vertexNodes) {
                result.Add(vert);
            }
        } else if (GetType() == typeof(EdgeNode)) {
            // Cast to edge node.
            EdgeNode edge = (EdgeNode) this;

            // Add neighbors.
            for (int i = 0; i < 2; i++) {
                result.Add(edge.triangleNodes[0]);
                if (edge.triangleNodes[1] != null) // check if a second triangle exists.
                    result.Add(edge.triangleNodes[1]);
            }

            foreach (VertexNode vert in edge.vertexNodes) {
                result.Add(vert);
            }
        } else if (GetType() == typeof(VertexNode)) {
            // Cast to vertex node.
            VertexNode vert = (VertexNode) this;

            // Add neighbors.
            foreach (TriangleNode tri in vert.triangleNodes) {
                result.Add(tri);
            }

            foreach (EdgeNode edge in vert.edgeNodes) {
                result.Add(edge);
            }
        } else {
            Debug.LogError("Unhandled type.");
        }

        return result;
    }
}
