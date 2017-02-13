using System;

namespace PathGen {
    public class NodeConnection {
        private readonly Node a;
        private readonly Node b;

        public NodeConnection(Node a, Node b) {
            this.a = a;
            this.b = b;
        }

        public Node getA() {
            return a;
        }

        public Node getB() {
            return b;
        }

        public Node getOther(Node node) {
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
            return (a.Equals(other.a) || a.Equals(other.b)) && (b.Equals(other.a) || b.Equals(other.b));
        }

        public override int GetHashCode() {
            return a.GetHashCode() ^ b.GetHashCode();
        }

        public override string ToString() {
            return "<NodeConnection " + a + ":" + b + ">";
        }
    }
}