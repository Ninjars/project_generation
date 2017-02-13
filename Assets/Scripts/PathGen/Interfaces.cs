using UnityEngine;

namespace PathGen {
    public interface IPathMeshBuilder {
        Mesh generateMesh(Vector3 start, Vector3 end);
    }

    public interface INodeMeshBuilder {
        Mesh generateMesh();
    }
}