using System;
using System.Collections.Generic;
using System.Linq;

namespace Node {
    public class NodeValue {

        private Dictionary<int, int> playerStakes = new Dictionary<int, int>();
        private int neutralValue;
        private int maxValue;
        private int owningPlayer = GameManager.NEUTRAL_PLAYER_ID;
        private Action<int> onOwnerChangeCallback;

        public NodeValue(int player, int maximumValue, int initialValue, Action<int> onOwnerChangeCallback) {
            maxValue = maximumValue;
            if (initialValue > 0) {
                if (player == GameManager.NEUTRAL_PLAYER_ID) {
                    neutralValue = initialValue;
                } else {
                    playerStakes[player] = initialValue;
                    neutralValue = 0;
                }
            } else {
                neutralValue = 0;
            }
            owningPlayer = player;
            this.onOwnerChangeCallback = onOwnerChangeCallback;
        }

        private void setOwningPlayer(int player) {
            if (player != GameManager.NEUTRAL_PLAYER_ID) {
                // once captured, remove the neutral value buffer
                neutralValue = 0;
            }
            owningPlayer = player;
            onOwnerChangeCallback(player);
        }

        public void changePlayerValue(int player, int value) {
            if (isOwned()) {
                // update owning player value accordingly, possibly removing ownership if stake falls below 1
                if (player == owningPlayer) {
                    updateOwnerValue(value);
                } else {
                    updateOwnerValue(-value);
                }
            } else {
                // neutral player value is threshold for ownership
                // until owned, value can't exceed neutral player value
                // when total of contesting players values reaches that of neutral player value,
                // any additional value reduces the lowest player's stake until only one player has a stake
                // equalling that of the neutral player, and triggering ownership change

                int playerValue;
                playerStakes.TryGetValue(player, out playerValue);
                playerValue += value;

                if (playerValue <= 0) {
                    playerStakes.Remove(player);
                } else {
                    playerStakes[player] = playerValue;
                }

                int totalValue = getTotalValue();
                if (totalValue > neutralValue && ! isUncontested()) {
                    purgeOtherPlayersRecursively(player, totalValue - neutralValue);
                }
            }
            checkForOwnerChange();
        }

        private void checkForOwnerChange() {
            bool isOwned = isUncontested() && playerStakes.Values.DefaultIfEmpty(-1).First() >= neutralValue;
            if (isOwned) {
                int owningId = playerStakes.Keys.DefaultIfEmpty(0).First();
                bool ownerChange = owningId != getOwnerId();
                if (ownerChange) {
                    setOwningPlayer(owningId);
                }
            }
        }

        private bool updateOwnerValue(int valueChange) {
            int ownerStake = getValueForPlayer(owningPlayer);
            int stake = Math.Min(maxValue, ownerStake + valueChange);
            bool ownerChange = false;
            if (stake <= 0) {
                playerStakes.Remove(owningPlayer);
                owningPlayer = getHighestValuePlayer();
                ownerChange = true;
            } else {
                playerStakes[owningPlayer] = stake;
            }
            return ownerChange;
        }

        private void purgeOtherPlayersRecursively(int player, int valueToPurge) {
            if (isUncontested()) {
                return;
            }
            int remainingToPurge = valueToPurge;
            int targetPlayer = getLowestValuePlayer(player);
            int playerValue;
            playerStakes.TryGetValue(targetPlayer, out playerValue);
            playerValue -= remainingToPurge;
            if (playerValue <= 0) {
                playerStakes.Remove(targetPlayer);
                int remaining = remainingToPurge - playerValue;
                purgeOtherPlayersRecursively(player, remaining);
            } else {
                playerStakes[targetPlayer] = playerValue;
            }
        }

        private int getLowestValuePlayer(int excludePlayerId) {
            int lowestPlayerId = -1;
            int lowestValue = -1;
            foreach (KeyValuePair<int, int> entry in playerStakes) {
                if (entry.Key == excludePlayerId) {
                    continue;
                } else if (entry.Value > lowestValue) {
                    lowestValue = entry.Value;
                    lowestPlayerId = entry.Key;
                }
            }
            return lowestPlayerId;
        }

        internal int getMaxValue() {
            return isOwned() ? maxValue : neutralValue;
        }

        private int getHighestValuePlayer() {
            int playerId = GameManager.NEUTRAL_PLAYER_ID;
            int value = -1;
            foreach (KeyValuePair<int, int> entry in playerStakes) {
                if (entry.Value > value) {
                    value = entry.Value;
                    playerId = entry.Key;
                }
            }
            return playerId;
        }

        public int getTotalValue() {
            int totalValue = 0;
            foreach (int value in playerStakes.Values) {
                totalValue += value;
            }
            return totalValue;
        }

        public int getValueForPlayer(int playerId) {
            int returnValue;
            playerStakes.TryGetValue(playerId, out returnValue);
            return returnValue;
        }

        public bool isUncontested() {
            return playerStakes.Count < 2;
        }

        public bool isOwned() {
            return owningPlayer != GameManager.NEUTRAL_PLAYER_ID;
        }

        public int getOwnerId() {
            return owningPlayer;
        }

        public int getNeutralValue() {
            return neutralValue;
        }

        public Dictionary<int, int> getPlayerStakes() {
            return playerStakes;
        }
    }
}
