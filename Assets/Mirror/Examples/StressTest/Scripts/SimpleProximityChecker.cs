using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.Examples.StressTest {
    public class SimpleProximityChecker : NetworkBehaviour {
        [TooltipAttribute("How often (in seconds) that this object should update the set of players that can see it.")]
        public float visUpdateInterval = 1.0f; // in seconds

        float m_VisUpdateTime;

        // called when a new player enters
        public override bool OnCheckObserver(NetworkConnection newObserver) {
            if (newObserver.playerController != null) {
                var pos = transform.position;
                return pos.x >= -100 && pos.x <= 100 && pos.y >= -100 && pos.y <= 100;
            }

            return false;
        }

        public override bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initial) {
            var pos = transform.position;
            if (pos.x >= -100 && pos.x <= 100 && pos.y >= -100 && pos.y <= 100) {
                foreach (var connection in NetworkServer.connections.Values) {
                    observers.Add(connection);
                }
            }
            return true;
        }
    }
}