using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshGraphUtils {

    /*
     * Appends vertices adjacent to the selection to the selection.
     */
    public static List<VertexNode> GrowSelection(List<VertexNode> selection) {
        // Fill expansion with selection.
        List<VertexNode> expansion = new List<VertexNode>(selection);

        for (int i = 0; i < selection.Count; i++) {
            VertexNode s = selection[i];
            foreach (EdgeNode e in s.edgeNodes) {
                // Get the vertex node on the other side of the edge node.
                VertexNode v = e.Complement(s);

                // Add it to expansion if it is not part of the original selection.
                if (!selection.Contains(v)) 
                    expansion.Add(v); 
            }
        }

        return expansion;
    }
}
