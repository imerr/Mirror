using System;
using UnityEngine;

namespace Mirror
{
    /// <summary>
    /// This helper class is currently just acting as a storage for the previous grid position and to get events when an entity is added/destroyed
    /// </summary>
    public class AOISpatialHashingData : NetworkBehaviour
    {
        [NonSerialized] public Vector2Int Previous;
        [NonSerialized] public NetworkIdentity Identity;

        public void Start()
        {
            Identity = GetComponent<NetworkIdentity>();
            if (NetworkServer.active && AOISpatialHashingInterestManagement.Instance)
            {
                AOISpatialHashingInterestManagement.Instance.Register(this);
            }
        }

        public void OnDestroy()
        {
            if (NetworkServer.active && AOISpatialHashingInterestManagement.Instance)
            {
                AOISpatialHashingInterestManagement.Instance.Deregister(this);
            }
        }

        private void OnDrawGizmos()
        {
            if (isLocalPlayer && AOISpatialHashingInterestManagement.Instance)
            {
                Vector2Int pos = AOISpatialHashingInterestManagement.ProjectToGrid(transform.position,
                    AOISpatialHashingInterestManagement.CheckMethod.XZ_FOR_3D,
                    AOISpatialHashingInterestManagement.Instance.resolution);
                Gizmos.color = Color.blue;
                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        Vector2Int tile = pos + new Vector2Int(x, y);
                        tile *= AOISpatialHashingInterestManagement.Instance.resolution;
                        Gizmos.DrawWireCube(new Vector3(tile.x, transform.position.y, tile.y),
                            new Vector3(AOISpatialHashingInterestManagement.Instance.resolution, 2,
                                AOISpatialHashingInterestManagement.Instance.resolution));
                    }
                }
            }
        }
    }
}
