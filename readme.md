Mesh Graph
===

#### What is it?
A mesh graph is a data structure repesenting the connectivity of the faces, edges, and vertices in a mesh.
Each node in the graph is either a face, edge, or vertex (aka geometry primitives), and two nodes are connected if the two corresponding geometry primitives are connected.

#### What is it for?
* I used it to prototype a system where objects must remain in contact with each other. It's useful in cases when the two object might be moving extremely quickly, or when you need an object to "stand" on the fine edge of another object. Systems that use raytracing to detect the surface of another object will fail if the objects move fast enough, or if the edge if very fine and thin.
* This is also useful when you need to calculate the distance between two points over the surface of a mesh (the path must remain on the surface of the mesh). You can implement graph traversing algorithms like BFS to find the distance between two points.


This system is built on top of Unity's already established physics engine so that it can take advantage of Unity's collision detection system. You can use the `onCollision` hooks you are already used to using.

![Mesh Graph Visualizer](http://i.imgur.com/B8vrfQ5.jpg)

Left: A regular old mesh. Right: Debug mode draws a sphere at the position of each node in the graph. Red node are triangles, blue nodes are vertices, and green nodes are edges.

___

![3 Body](http://i.imgur.com/mCazOHI.gif)

3 bodies are contrained to remain in contact with the triangle. A repulsive `1/r^2` force is between them. Distance is calculated in world-space. In other words, distance is not limited a path on the surfact of the triangle.

___

![Electrons](http://i.imgur.com/aR5dwv0.gif)

This simulation illustrates why tesla coils arc from sharp edges. Electrons will cluster at sharp edges, thus increasing the voltage in those places. Bodies are repulsed by a `1/r^2` force. Velocity is dampened. Distance is calculated in world-space.

___

![Camera follows sphere](http://i.imgur.com/d6kIL8L.gif)

The camera's target is an object (red sphere) which is constrained to the surface of a triangle. When the sphere reaches the edge of the triangle, it remains in contact with the edge. It wraps around to the other side of the triangle. The camera follows the sphere, so it appears the that sphere is standing still while the triangle is flipping over. It almost looks like the triangle is unfolding itself just in time to stay under the sphere, similar to how a red carpet might be rolled out in front of someone.
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTk4MzIwMTYyMl19
-->