using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    public struct PlayerMaterialViewModel {
        public readonly Material material;
        public readonly int count;

        public PlayerMaterialViewModel(Material material, int count) {
            this.material = material;
            this.count = count;
        }
    }

    public struct NodeViewModel {
        public readonly bool isOwned;
        public readonly int maxValue;
        public readonly List<PlayerMaterialViewModel> valueModel;

        public NodeViewModel(bool isOwned, int maxValue, List<PlayerMaterialViewModel> valueModel) {
            this.isOwned = isOwned;
            this.maxValue = maxValue;
            this.valueModel = valueModel;
        }
    }
}
