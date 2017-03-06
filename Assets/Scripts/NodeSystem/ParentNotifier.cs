using UnityEngine;

namespace Node {
    public class ParentNotifier : MonoBehaviour {
        public IClickListener listener;
        void OnMouseDown() {
            listener.onClick();
        }
    }
}
