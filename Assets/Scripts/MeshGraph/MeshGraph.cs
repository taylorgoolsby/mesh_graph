using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// A data structure.
// A MeshGraph is a graph of the mesh where each vertex, edge, and triangle is a node.
// Two nodes are connected if the two elements are adjacent in the mesh.
[System.Serializable]
public class MeshGraph : ScriptableObject, ISerializationCallbackReceiver {
    public List<VertexNode> vertexNodes; // Nodes don't get serialized.
    public List<EdgeNode> edgeNodes;
    public List<TriangleNode> triangleNodes;

    // id used to deserialize
    private static int nextID; // id is local to the current meshGraph. Gets reset at the beginning of CreateGraph().
    // Since these IDs are being used as storage locations and pointers/references, a value of 0 is used for null.
    // Serialization methods at the end of the class declarations.
    
    // Creates a new MeshGraph given a mesh.
    public static MeshGraph CreateGraph(Mesh mesh) {
        MeshGraph result = CreateInstance<MeshGraph>();

        // used for assigning ids to nodes.
        nextID = 1; // reset id, id = 0 is reserved for null reference.

        int[] mapping;
        result.vertexNodes = CreateVertexNodes(mesh, out mapping);
        result.triangleNodes = CreateTriangleNodes(mesh, result.vertexNodes, mapping);
        result.edgeNodes = CreateEdgeNodes(result.triangleNodes);

        Debug.Log("Created mesh graph from mesh \"" + mesh.name);
        return result;
    }

    // Creates vertex nodes that are unique with respect to their position.
    // Worst case O(n^2) when all vertices are already unique.
    private static List<VertexNode> CreateVertexNodes(Mesh mesh, out int[] mapping) {
        List<VertexNode> uniqueNodes = new List<VertexNode>();
        mapping = new int[mesh.vertices.Length];

        VertexNode node;
        bool match;
        for (int i = 0; i < mesh.vertices.Length; i++) {
            // Check for match
            match = false;
            for (int j = 0; j < uniqueNodes.Count; j++) {
                if (EqualsPosition(mesh.vertices[i], uniqueNodes[j].position)) {
                    match = true;
                    mapping[i] = j;
                    break;
                }
            }

            if (!match) {
                node = new VertexNode();
                node.id = nextID;
                nextID++;
                node.index = i;
                node.position = mesh.vertices[i];
                mapping[i] = uniqueNodes.Count;
                uniqueNodes.Add(node);
            }
        }

        return uniqueNodes;
    }

    // Creates triangle nodes. O(n)
    private static List<TriangleNode> CreateTriangleNodes(Mesh mesh, List<VertexNode> unique, int[] mapping) {
        List<TriangleNode> triangleNodes = new List<TriangleNode>();

        TriangleNode t;
        VertexNode v;
        for (int i = 0; i < mesh.triangles.Length; i += 3) {
            // Create triangle region
            t = new TriangleNode();
            t.id = nextID;
            nextID++;
    
            // Cycle through vertices
            for (int j = 0; j < 3; j++) {
                // Use mapping to get the unique vertex.
                v = unique[mapping[mesh.triangles[i + j]]];

                // Store the index of the original vertex.
                t.originalVertices[j] = mesh.triangles[i + j];

                // Make connection.
                t.vertexNodes[j] = v;
                v.triangleNodes.Add(t);
            }

            // Compute triangle position.
            t.position = t.AverageVertexPosition();
           
            triangleNodes.Add(t);
        }

        return triangleNodes;
    }

    // Creates edge nodes. O(n)
    private static List<EdgeNode> CreateEdgeNodes(List<TriangleNode> triangleNodes) {
        // Uses hashing to find unique edges in O(1).
        Dictionary<ulong, EdgeNode> dictionary = new Dictionary<ulong, EdgeNode>();
        int i0;
        int i1;
        ulong key;
        EdgeNode e;
        for (int i = 0; i < triangleNodes.Count; i++) {
            for (int j = 0; j < 3; j++) {
                // Since the triangle vertices are all unique, each edge is unique.
                // But a different triangle may have already created this edge.
                i0 = triangleNodes[i].vertexNodes[j].index;
                i1 = triangleNodes[i].vertexNodes[(j + 1) % 3].index;

                key = MakeEdgeKey(i0, i1);

                if (!dictionary.ContainsKey(key)) {
                    // This edge has not been created yet.
                    e = new EdgeNode();
                    e.id = nextID;
                    nextID++;
                    
                    // Connect vertices.
                    e.vertexNodes[0] = triangleNodes[i].vertexNodes[j];
                    e.vertexNodes[1] = triangleNodes[i].vertexNodes[(j + 1) % 3];
                    triangleNodes[i].vertexNodes[j].edgeNodes.Add(e);
                    triangleNodes[i].vertexNodes[(j + 1) % 3].edgeNodes.Add(e);

                    // Connect triangle.
                    e.triangleNodes[0] = triangleNodes[i];
                    triangleNodes[i].edgeNodes[j] = e;

                    dictionary[key] = e;
                } else {
                    // If this edge has been created before, it already has it's vertices,
                    // but it does not have a connection to this triangle.
                    e = dictionary[key];
                    e.triangleNodes[1] = triangleNodes[i];
                    triangleNodes[i].edgeNodes[j] = e;
                }
            }
        }

        // Turn the dictionary into a list.
        List<EdgeNode> list = new List<EdgeNode>();
        foreach (EdgeNode edge in dictionary.Values) {
            // Compute edge position.
            edge.position = edge.AverageVertexPosition();
            list.Add(edge);
        }

        return list;
    }

    // Hashing function for two integers
    private static ulong MakeEdgeKey(int i0, int i1) {
        uint lower;
        uint upper;

        if (i0 < i1) {
            lower = (uint) i0;
            upper = (uint) i1;
        } else {
            lower = (uint) i1;
            upper = (uint) i0;
        }

        ulong result = upper;
        result = result << 32;
        result = result + lower;

        return result;
    }

    // Returns true if positions are exactly the same.
    private static bool EqualsPosition(Vector3 p0, Vector3 p1) {
        if (p0.x == p1.x && p0.y == p1.y && p0.z == p1.z) {
            return true;
        }
        return false;
    }

    /* BEGIN SERIALIZATION FIELDS AND METHODS */

    [System.Serializable]
    public struct SerializableVertexNode {
        public int id;
        public int index;
        public Vector3 position;
        public int[] edgeIDs;
        public int[] triangleIDs;
    }

    [System.Serializable]
    public struct SerializableEdgeNode {
        public int id;
        public Vector3 position;
        public int[] vertexIDs;
        public int[] triangleIDs;
    }

    [System.Serializable]
    public struct SerializableTriangleNode {
        public int id;
        public Vector3 position;
        public int[] originalVertices;
        public int[] vertexIDs;
        public int[] edgeIDs;
    }

    public List<SerializableVertexNode> s_vertexNodes;
    public List<SerializableEdgeNode> s_edgeNodes;
    public List<SerializableTriangleNode> s_triangleNodes;

    public void OnBeforeSerialize() {
        // Check if lists have been created yet.
        if (s_vertexNodes == null)
            s_vertexNodes = new List<SerializableVertexNode>();
        if (s_edgeNodes == null)
            s_edgeNodes = new List<SerializableEdgeNode>();
        if (s_triangleNodes == null)
            s_triangleNodes = new List<SerializableTriangleNode>();

        // Clear lists.
        s_vertexNodes.Clear();
        s_edgeNodes.Clear();
        s_triangleNodes.Clear();

        // Add serialized nodes to lists.
        foreach (VertexNode v in vertexNodes) {
            s_vertexNodes.Add(SerializeVertexNode(v));
        }

        foreach (EdgeNode e in edgeNodes) {
            s_edgeNodes.Add(SerializeEdgeNode(e));
        }

        foreach (TriangleNode t in triangleNodes) {
            s_triangleNodes.Add(SerializeTriangleNode(t));
        }
    }

    private SerializableVertexNode SerializeVertexNode(VertexNode v) {
        SerializableVertexNode result = new SerializableVertexNode();
        result.id = v.id;
        result.index = v.index;
        result.position = v.position;

        result.edgeIDs = new int[v.edgeNodes.Count];
        for (int i = 0; i < v.edgeNodes.Count; i++) {
            result.edgeIDs[i] = v.edgeNodes[i].id;
        }

        result.triangleIDs = new int[v.triangleNodes.Count];
        for (int i = 0; i < v.triangleNodes.Count; i++) {
            result.triangleIDs[i] = v.triangleNodes[i].id;
        }

        return result;
    }

    private SerializableEdgeNode SerializeEdgeNode(EdgeNode e) {
        SerializableEdgeNode result = new SerializableEdgeNode();
        result.id = e.id;
        result.position = e.position;

        result.vertexIDs = new int[2];
        result.vertexIDs[0] = e.vertexNodes[0].id;
        result.vertexIDs[1] = e.vertexNodes[1].id;

        result.triangleIDs = new int[2];
        result.triangleIDs[0] = e.triangleNodes[0].id;
        if (e.triangleNodes[1] != null) {
            // some edges are connected to 2 triangles, some are connected to 1.
            result.triangleIDs[1] = e.triangleNodes[1].id;
        } // if only one connected triangle, then result.triangleIDs was initialized to 0. ID == 0 means null.

        return result;
    }

    private SerializableTriangleNode SerializeTriangleNode(TriangleNode t) {
        SerializableTriangleNode result = new SerializableTriangleNode();
        result.id = t.id;
        result.position = t.position;
        result.originalVertices = t.originalVertices;

        result.vertexIDs = new int[3];
        result.vertexIDs[0] = t.vertexNodes[0].id;
        result.vertexIDs[1] = t.vertexNodes[1].id;
        result.vertexIDs[2] = t.vertexNodes[2].id;

        result.edgeIDs = new int[3];
        result.edgeIDs[0] = t.edgeNodes[0].id;
        result.edgeIDs[1] = t.edgeNodes[1].id;
        result.edgeIDs[2] = t.edgeNodes[2].id;

        return result;
    }

    public void OnAfterDeserialize() {
        // Unity has just written to s_ fields.
        // Construct new node objects with missing connection references.
        vertexNodes = new List<VertexNode>();
        edgeNodes = new List<EdgeNode>();
        triangleNodes = new List<TriangleNode>();

        // Add objects without references to dictionaries.
        Dictionary<int, VertexNode> vertexDictionary = new Dictionary<int, VertexNode>();
        Dictionary<int, EdgeNode> edgeDictionary = new Dictionary<int, EdgeNode>();
        Dictionary<int, TriangleNode> triangleDictionary = new Dictionary<int, TriangleNode>();

        foreach (SerializableVertexNode sv in s_vertexNodes) {
            VertexNode v = DeserializeVertexNode(sv);
            vertexDictionary.Add(v.id, v);
            vertexNodes.Add(v);
        }

        foreach (SerializableEdgeNode se in s_edgeNodes) {
            EdgeNode e = DeserializeEdgeNode(se);
            edgeDictionary.Add(e.id, e);
            edgeNodes.Add(e);
        }

        foreach (SerializableTriangleNode st in s_triangleNodes) {
            TriangleNode t = DeserializeTriangleNode(st);
            triangleDictionary.Add(t.id, t);
            triangleNodes.Add(t);
        }

        // Second pass to use dictionaries to make connections
        foreach (SerializableVertexNode sv in s_vertexNodes) {
            VertexNode v = vertexDictionary[sv.id];
            foreach (int i in sv.edgeIDs) {
                v.edgeNodes.Add(edgeDictionary[i]);
            }
            foreach (int i in sv.triangleIDs) {
                v.triangleNodes.Add(triangleDictionary[i]);
            }
        }

        foreach (SerializableEdgeNode se in s_edgeNodes) {
            EdgeNode e = edgeDictionary[se.id];
            e.vertexNodes[0] = vertexDictionary[se.vertexIDs[0]];
            e.vertexNodes[1] = vertexDictionary[se.vertexIDs[1]];
            e.triangleNodes[0] = triangleDictionary[se.triangleIDs[0]];
            if (se.triangleIDs[1] != 0) { // not null second triangle reference
                e.triangleNodes[1] = triangleDictionary[se.triangleIDs[1]];
            }
        }

        foreach (SerializableTriangleNode st in s_triangleNodes) {
            TriangleNode t = triangleDictionary[st.id];
            for (int i = 0; i < 3; i++) {
                t.vertexNodes[i] = vertexDictionary[st.vertexIDs[i]];
                t.edgeNodes[i] = edgeDictionary[st.edgeIDs[i]];
            }
        }
    }

    // Ignores connection references
    private VertexNode DeserializeVertexNode(SerializableVertexNode sv) {
        VertexNode result = new VertexNode();
        result.id = sv.id;
        result.index = sv.index;
        result.position = sv.position;
        return result;
    }

    // Ignores connection references
    private EdgeNode DeserializeEdgeNode(SerializableEdgeNode se) {
        EdgeNode result = new EdgeNode();
        result.id = se.id;
        result.position = se.position;
        return result;
    }

    // Ignores connection references
    private TriangleNode DeserializeTriangleNode(SerializableTriangleNode st) {
        TriangleNode result = new TriangleNode();
        result.id = st.id;
        result.position = st.position;
        result.originalVertices = st.originalVertices;
        return result;
    }

    /* END SERIALIZATION FIELDS AND METHODS */
}
