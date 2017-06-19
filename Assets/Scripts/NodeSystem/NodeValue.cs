using System;
using System.Collections.Generic;
using System.Linq;

namespace Node {
    public class NodeValue {

        private Dictionary<Player, int> playerStakes = new Dictionary<Player, int>();
        private int neutralValue;
        private int maxValue;
        private Player owningPlayer;
        private Action<Player> onOwnerChangeCallback;
        private GameManager gameManager;

        public NodeValue(GameManager gameManager, Player player, int maximumValue, int initialValue, Action<Player> onOwnerChangeCallback) {
            this.gameManager = gameManager;
            maxValue = maximumValue;
            if (initialValue > 0) {
                if (player.isNeutralPlayer()) {
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

        public void setMaxValue(int newValue) {
            maxValue = newValue;
        }

        private void setOwningPlayer(Player player) {
            if (!player.isNeutralPlayer()) {
                // once captured, remove the neutral value buffer
                neutralValue = 0;
            }
            owningPlayer = player;
            onOwnerChangeCallback(player);
        }

        public void changePlayerValue(Player player, int value) {
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
                Player owningPlayer = playerStakes.Keys.DefaultIfEmpty(gameManager.getNeutralPlayer()).First();
                bool ownerChange = owningPlayer != getOwningPlayer();
                if (ownerChange) {
                    setOwningPlayer(owningPlayer);
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

        private void purgeOtherPlayersRecursively(Player player, int valueToPurge) {
            if (isUncontested()) {
                return;
            }
            int remainingToPurge = valueToPurge;
            Player targetPlayer = getLowestValuePlayer(player);
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

        private Player getLowestValuePlayer(Player excludePlayer) {
            Player player = gameManager.getNeutralPlayer();
            int lowestValue = -1;
            foreach (KeyValuePair<Player, int> entry in playerStakes) {
                if (entry.Key == excludePlayer) {
                    continue;
                } else if (entry.Value > lowestValue) {
                    lowestValue = entry.Value;
                    player = entry.Key;
                }
            }
            return player;
        }

        internal int getMaxValue() {
            return isOwned() ? maxValue : neutralValue;
        }

        private Player getHighestValuePlayer() {
            Player player = gameManager.getNeutralPlayer();
            int value = -1;
            foreach (KeyValuePair<Player, int> entry in playerStakes) {
                if (entry.Value > value) {
                    value = entry.Value;
                    player = entry.Key;
                }
            }
            return player;
        }

        public int getTotalValue() {
            int totalValue = 0;
            foreach (int value in playerStakes.Values) {
                totalValue += value;
            }
            return totalValue;
        }

        public int getValueForPlayer(Player player) {
            int returnValue;
            playerStakes.TryGetValue(player, out returnValue);
            return returnValue;
        }

        public bool isUncontested() {
            return playerStakes.Count < 2;
        }

        public bool isOwned() {
            return owningPlayer != gameManager.getNeutralPlayer();
        }

        public Player getOwningPlayer() {
            return owningPlayer;
        }

        public int getNeutralValue() {
            return neutralValue;
        }

        public Dictionary<Player, int> getPlayerStakes() {
            return playerStakes;
        }
    }
}
