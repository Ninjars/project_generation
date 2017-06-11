using UnityEngine;

namespace PathGen {
    public interface IPathMeshBuilder {
        Mesh generateMesh(Vector3 start, Vector3 end);
    }

    [RequireComponent(typeof(IPathMeshBuilder))]
    public class PathwayGenerator : MonoBehaviour {
        private IPathMeshBuilder pathMeshGenerator;
        public Material pathMaterial;

        private void Awake() {
            pathMeshGenerator = gameObject.GetComponent<IPathMeshBuilder>();
        }

        public GameObject createConnection(GameObject start, Vector3 end) {
            Mesh mesh = pathMeshGenerator.generateMesh(start.transform.position, end);
            GameObject container = new GameObject();
            container.transform.parent = start.transform;
            MeshFilter meshFilter = container.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            MeshRenderer meshRenderer = container.AddComponent<MeshRenderer>();
            meshRenderer.material = pathMaterial;
            return container;
        }
    }
}