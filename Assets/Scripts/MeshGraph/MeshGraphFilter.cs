using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * All access to the mesh graph is done through the filter.
 * Attach this component to a MeshFilter to make its MeshGraph generated.
 */
[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class MeshGraphFilter : MonoBehaviour {
    public bool activateRendering = false;
    
    private static Dictionary<int, MeshGraph> instances;

    private int m_InstanceID;
    
    // Does not get called in edit mode.
    void Awake() {
        m_InstanceID = GetInstanceID();

        if (instances == null) {
            instances = new Dictionary<int, MeshGraph>();
        }
        
        if (activateRendering) {
            MeshGraphRenderer renderer = GetComponent<MeshGraphRenderer>();
            if (renderer == null) {
                gameObject.AddComponent<MeshGraphRenderer>();
            }
        }
    }

    void Update() {
        if (instances == null) {
            instances = new Dictionary<int, MeshGraph>();
        }
        if (instances.ContainsKey(m_InstanceID)) {
                
        } else {
            OnKeyMissing();
        }
    }

    public MeshGraph GetMeshGraph() {
        // instances should exist because Awake() should be called first.
        if (instances == null) {
            return null;
        }

        if (instances.ContainsKey(m_InstanceID)) {
            // it exists
            //print("key exists");
            return instances[m_InstanceID];
        } else {
            return OnKeyMissing();
        }
    }
    
    private void OnNullDictionary() {
        
    }
    
    private MeshGraph OnKeyMissing() {
        // it does not exist
        //print("key does not exist");
        instances[m_InstanceID] = MeshGraph.CreateGraph(GetComponent<MeshFilter>().sharedMesh);
        //print("new key created");
        return instances[m_InstanceID];
    }
}
