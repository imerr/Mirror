// interest management from DOTSNET

using UnityEngine;

namespace Mirror
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkManager))]
    public abstract class InterestManagement : MonoBehaviour
    {
        // configure NetworkServer
        protected virtual void Awake() { NetworkServer.interestManagement = this; }

        // rebuild all observers
        public abstract void RebuildAll();

        /// <summary>
        /// Adds an observer to the specified identity
        /// Modifies the identity.observers collection
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="observer"></param>
        protected void AddObserver(NetworkIdentity identity, NetworkConnectionToClient observer)
        {
            identity.observers.Add(observer);
            NetworkServer.ShowForConnection(identity, observer);
        }

        /// <summary>
        /// Removes an observer from the specified identity
        /// Modifies the identity.observers collection
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="observer"></param>
        protected void RemoveObserver(NetworkIdentity identity, NetworkConnectionToClient observer)
        {
            if (identity.observers.Remove(observer))
            {
                NetworkServer.HideForConnection(identity, observer);
            }
        }
    }
}
