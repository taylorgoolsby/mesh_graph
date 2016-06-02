using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshGraphFilter))]
public class MeshGraphRenderer : MonoBehaviour {
    public float nodeRadius = 0.01f;

    private MeshGraphFilter m_MeshGraphFilter;

    // one game object for each node
    private Dictionary<int, GameObject> gameObjects; // not serializable (no hot swapping)
    // key is the node id
    private Dictionary<int, GeometryNode> nodes; // not serializable

    void Awake() {
        
    }

    void Start() {
        m_MeshGraphFilter = GetComponent<MeshGraphFilter>();

        if (gameObjects == null) { // first instantiation
            gameObjects = new Dictionary<int, GameObject>();
            nodes = new Dictionary<int, GeometryNode>();

            MeshGraph meshGraph = GetComponent<MeshGraphFilter>().GetMeshGraph();

            // Create a rigidbody for each node. Use id for dictionary mapping.
            foreach (VertexNode v in meshGraph.vertexNodes) {
                GameObject go = CreateVertexGameObject(v);
                gameObjects[v.id] = go;
                nodes[v.id] = v;
            }

            foreach (EdgeNode e in meshGraph.edgeNodes) {
                GameObject go = CreateEdgeGameObject(e);
                gameObjects[e.id] = go;
                nodes[e.id] = e;
            }

            foreach (TriangleNode t in meshGraph.triangleNodes) {
                GameObject go = CreateTriangleGameObject(t);
                gameObjects[t.id] = go;
                nodes[t.id] = t;
            }
        }
    }

    // Update is called once per frame
    void Update() {
        // apply the host mesh's transform to the nodes so that they are rendered with the mesh.
        // These node objects are like the view of MVC.
        // The mesh graph is the model. It is unaffected by these transforms.
        // 
        foreach (int id in gameObjects.Keys) {
            GameObject go = gameObjects[id];
            go.transform.position = m_MeshGraphFilter.transform.TransformPoint(nodes[id].position); // update position

            float avgDist = 1;

            go.transform.localScale = new Vector3(nodeRadius * avgDist, nodeRadius * avgDist, nodeRadius * avgDist); // update scale
        }
    }

    private GameObject CreateVertexGameObject(VertexNode node) {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "VertexNode" + node.id;
        go.transform.position = node.position;
        
        // Set color.
        Material mat = go.GetComponent<MeshRenderer>().material;
        mat.color = Color.blue;

        return go;
    }

    private GameObject CreateEdgeGameObject(EdgeNode node) {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "EdgeNode" + node.id;
        go.transform.position = node.position;

        // Set color.
        Material mat = go.GetComponent<MeshRenderer>().material;
        mat.color = Color.green;

        return go;
    }

    private GameObject CreateTriangleGameObject(TriangleNode node) {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "TriangleNode" + node.id;
        go.transform.position = node.position;

        // Set color.
        Material mat = go.GetComponent<MeshRenderer>().material;
        mat.color = Color.red;

        return go;
    }
}
