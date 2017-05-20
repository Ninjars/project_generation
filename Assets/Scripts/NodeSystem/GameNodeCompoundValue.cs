using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    [RequireComponent(typeof(Node))]
    public abstract class GameNodeCompoundValue : GameNode {

        public int currentSubValue = 0;
        public int maxSubValue = 5;

        public override void changeValue(int playerId, int change) {
            int subValue = currentSubValue + change;
            currentSubValue = subValue % maxSubValue;
            base.changeValue(playerId, (int)Mathf.Floor(subValue / maxSubValue));
        }

        public int getRawValue() {
            return getOwnerValue() * maxSubValue + currentSubValue;
        }
    }
}
