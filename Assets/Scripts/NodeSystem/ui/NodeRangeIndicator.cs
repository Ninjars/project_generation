
using UnityEngine;

namespace Node {
    public class NodeRangeIndicator : MonoBehaviour {

        public Material material;
        private GameObject indicatorObject;
        private float indicatorDepth = 0.001f;

        void Start() {
            indicatorObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            indicatorObject.transform.parent = gameObject.transform;
            indicatorObject.transform.position = gameObject.transform.position;
            indicatorObject.transform.localScale = new Vector3(1, indicatorDepth, 1);
            indicatorObject.GetComponent<MeshRenderer>().material = material;
        }

        public void setRadius(float radius) {
            if (indicatorObject != null) {
                indicatorObject.transform.localScale = new Vector3(radius * 2, indicatorDepth, radius * 2);
            }
        }
    }
}
