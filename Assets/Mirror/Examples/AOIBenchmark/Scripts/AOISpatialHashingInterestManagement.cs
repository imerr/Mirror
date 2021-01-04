// Spatial Hashing based on uMMORPG GridChecker

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace Mirror
{
    public class AOISpatialHashingInterestManagement : InterestManagement
    {
        public static AOISpatialHashingInterestManagement Instance;

        // view range
        // note: unlike uMMORPG, this can now be changed AT RUNTIME because
        // the grid is cleared in every Rebuild :)
        public int visibilityRadius = 100;

        // if we see 8 neighbors then 1 entry is visRange/3
        public int resolution => visibilityRadius / 3;

        // the grid
        // 2D vs 3D
        public enum CheckMethod
        {
            XZ_FOR_3D,
            XY_FOR_2D
        }

        [TooltipAttribute("Which method to use for checking proximity of players.")]
        public CheckMethod checkMethod = CheckMethod.XZ_FOR_3D;

        // using a List here since hashset allocates and finding an element in the list should be fast enough
        Dictionary<Vector2Int, List<NetworkIdentity>> grid = new Dictionary<Vector2Int, List<NetworkIdentity>>();
        private HashSet<AOISpatialHashingData> _entities = new HashSet<AOISpatialHashingData>();

        // project 3d position to grid
        public static Vector2Int ProjectToGrid(Vector3 position, CheckMethod checkMethod, int resolution)
        {
            // simple rounding for now
            // 3D uses xz (horizontal plane)
            // 2D uses xy
            if (checkMethod == CheckMethod.XZ_FOR_3D)
            {
                return Vector2Int.RoundToInt(new Vector2(position.x, position.z) / resolution);
            }
            else
            {
                return Vector2Int.RoundToInt(new Vector2(position.x, position.y) / resolution);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        public override void RebuildAll()
        {
            // this is dumb
            // just tell me the reason for rebuilding so we can do it selectively
            // dont assume its faster to throw everything away every rebuild

            // looking at usages this happens when:
            // - player object goes away (disconnect, replace object, and set not ready)
            // - host mode switches (wouldn't that kill the players object anyways and rebuild above?)
            // we cover those ourselves
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private void Update()
        {
            foreach (var entity in _entities)
            {
                var current = ProjectToGrid(entity.transform.position, checkMethod, resolution);
                if (entity.Previous != current)
                {
                    RebuildRemove(entity, current);
                    RebuildAdd(entity, current, true);
                }
            }
        }

        private void RebuildRemove(AOISpatialHashingData entity, Vector2Int? current)
        {
            Profiler.BeginSample("Remove from old tile");
            var removeTile = grid[entity.Previous];
            // remove from a list by swapping with the last element and removing the last element instead
            // so we don't need to move all elements past the removed - order doesn't matter for our use case

            // List<T>.FindIndex allocates.. *sigh*
            int index = -1;
            for (var i = 0; i < removeTile.Count; i++)
            {
                NetworkIdentity identity = removeTile[i];
                if (identity == entity.Identity)
                {
                    index = i;
                    break;
                }
            }


            // "swap" with the last element (even if its ourself - performance should be indifferent between if check)
            removeTile[index] = removeTile[removeTile.Count - 1];
            // remove last element, since we've swapped that with our position
            removeTile.RemoveAt(removeTile.Count - 1);

            Profiler.EndSample();
            Profiler.BeginSample("Remove old tile observers");
            // Remove from grid tiles we don't see anymore
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    var tilePos = entity.Previous + new Vector2Int(x, y);
                    if (current.HasValue && (Mathf.Abs(tilePos.x - current.Value.x) <= 1 &&
                                             Mathf.Abs(tilePos.y - current.Value.y) <= 1))
                    {
                        continue;
                    }

                    if (!grid.TryGetValue(tilePos, out var tile))
                    {
                        continue;
                    }

                    foreach (var identity in tile)
                    {
                        if (identity.connectionToClient != null)
                        {
                            Profiler.BeginSample("RemoveObserver - myself");
                            RemoveObserver(entity.Identity, identity.connectionToClient);
                            Profiler.EndSample();
                        }

                        if (entity.Identity.connectionToClient != null)
                        {
                            Profiler.BeginSample("RemoveObserver - other");
                            RemoveObserver(identity, entity.Identity.connectionToClient);
                            Profiler.EndSample();
                        }
                    }
                }
            }
            Profiler.EndSample();
        }

        private void RebuildAdd(AOISpatialHashingData entity, Vector2Int current, bool usePrevious)
        {
            Profiler.BeginSample("Add observers for new tiles");
            // Add from new grid tiles we now see
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    var tilePos = current + new Vector2Int(x, y);
                    if (usePrevious && (Mathf.Abs(tilePos.x - entity.Previous.x) <= 1 &&
                                        Mathf.Abs(tilePos.y - entity.Previous.y) <= 1))
                    {
                        continue;
                    }

                    if (!grid.TryGetValue(tilePos, out var tile))
                    {
                        continue;
                    }

                    foreach (var identity in tile)
                    {
                        if (identity.connectionToClient != null)
                        {
                            Profiler.BeginSample("AddObserver - myself");
                            AddObserver(entity.Identity, identity.connectionToClient);
                            Profiler.EndSample();
                        }

                        if (entity.Identity.connectionToClient != null)
                        {
                            Profiler.BeginSample("AddObserver - other");
                            AddObserver(identity, entity.Identity.connectionToClient);
                            Profiler.EndSample();
                        }
                    }
                }
            }
            Profiler.EndSample();

            Profiler.BeginSample("Add myself to new tiles");

            // add out selves to the new grid position
            if (!grid.TryGetValue(current, out var addTile))
            {
                addTile = new List<NetworkIdentity>();
                grid[current] = addTile;
            }

            addTile.Add(entity.Identity);
            entity.Previous = current;
            Profiler.EndSample();
        }

        // TODO: use a mirror hook here when a new entity is spawned
        public void Register(AOISpatialHashingData entity)
        {
            _entities.Add(entity);
            // client always sees itself
            if (entity.Identity.connectionToClient != null)
            {
                AddObserver(entity.Identity, entity.Identity.connectionToClient);
            }

            Vector2Int current = ProjectToGrid(entity.transform.position, checkMethod, resolution);
            RebuildAdd(entity, current, false);
        }

        // TODO: use a mirror hook here when an entity is destroyed
        public void Deregister(AOISpatialHashingData entity)
        {
            _entities.Remove(entity);
            RebuildRemove(entity, null);
        }
    }
}
