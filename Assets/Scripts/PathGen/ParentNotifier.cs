using UnityEngine;

namespace PathGen {
    public class ParentNotifier : MonoBehaviour {
        public IClickListener listener;
        void OnMouseDown() {
            listener.onClick();
        }
    }
}
