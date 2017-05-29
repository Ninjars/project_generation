using System;

namespace Node {
    public class NodeConnection {
        private readonly GameNode a;
        private readonly GameNode b;

        public NodeConnection(GameNode a, GameNode b) {
            this.a = a;
            this.b = b;
        }

        public GameNode getA() {
            return a;
        }

        public GameNode getB() {
            return b;
        }

        public GameNode getOther(GameNode node) {
            if (a.Equals(node)) {
                return b;
            } else if (b.Equals(node)) {
                return a;
            } else {
                return null;
            }
        }

        public override bool Equals(Object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            NodeConnection other = (NodeConnection) obj;
            return (a.Equals(other.a) && b.Equals(other.b)) || (a.Equals(other.b) && b.Equals(other.a));
        }

        public override int GetHashCode() {
            return a.GetHashCode() * b.GetHashCode();
        }

        public override string ToString() {
            return "<NodeConnection " + a + ":" + b + ">";
        }
    }
}