using UnityEngine;

namespace PathGen {
    public class BasicPathMeshBuilder : MonoBehaviour, IPathMeshBuilder {

        public double width = 2;

        public UnityEngine.Mesh generateMesh(Vector3 start, Vector3 end) {
            Vector3 vector = (end - start).normalized;
            Debug.Log("generateMesh " + start + " to " + end + "vector " + vector);
//            Vector3 start1 = start.
            return null;
        }
    }
}