using SanAndreasUnity.Importing.Paths;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Net;
using SanAndreasUnity.Utilities;
using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Behaviours.WorldSystem;
using UnityEngine;
using WorldSystemArea = SanAndreasUnity.Behaviours.WorldSystem.WorldSystem<SanAndreasUnity.Behaviours.PedAI>.Area;

namespace SanAndreasUnity.Behaviours
{
    public class NPCPedSpawner : StartupSingleton<NPCPedSpawner>
    {
        public int totalMaxNumSpawnedPeds = 40;
        public int maxNumSpawnedPedsPerPlayer = 10;

        private WorldSystem<PedAI> _worldSystem;

        [SerializeField] private int _areaSize = 200;
        [SerializeField] private int _revealRadius = 100;
        public float timeToKeepRevealingAfterRemoved = 3f;

        public float minSpawnDistanceFromFocusPoint = 40;

        private readonly List<WorldSystemArea> _areasWhichBecameInvisible = new List<WorldSystemArea>(32);
        private readonly List<WorldSystemArea> _visibleAreas = new List<WorldSystemArea>(32);

        private readonly List<(PedAI pedAI, WorldSystemArea area)> _spawnedPeds = new List<(PedAI, WorldSystemArea)>();

        private float _timeUntilNewSpawn = 0f;
        public float timeIntervalToAttemptSpawn = 0.33f;

        public FocusPointManager<PedAI> FocusPointManager { get; private set; }


        protected override void OnSingletonAwake()
        {
            _timeUntilNewSpawn = this.timeIntervalToAttemptSpawn;

            Ped.onStart += PedOnStart;
        }

        private void PedOnStart(Ped ped)
        {
            if (!NetStatus.IsServer)
                return;

            if (this.FocusPointManager != null && ped.PlayerOwner != null)
            {
                this.FocusPointManager.RegisterFocusPoint(
                    ped.transform,
                    new FocusPointParameters(false, 0f, this.timeToKeepRevealingAfterRemoved));
            }
        }

        void OnLoaderFinished()
        {
            if (!NetStatus.IsServer)
                return;

            int worldSize = Cell.Instance != null ? Cell.Instance.WorldSize : Cell.DefaultWorldSize;
            int numAreasPerAxis = Mathf.CeilToInt(worldSize / (float)_areaSize);

            _worldSystem = new WorldSystem<PedAI>(
                new WorldSystemParams { worldSize = (uint) worldSize, numAreasPerAxis = (ushort) numAreasPerAxis },
                new WorldSystemParams { worldSize = 50000, numAreasPerAxis = 1 },
                OnAreaChangedVisibility);

            this.FocusPointManager = new FocusPointManager<PedAI>(_worldSystem, _revealRadius);
        }

        private void OnAreaChangedVisibility(WorldSystem<PedAI>.Area area, bool isVisible)
        {
            if (!isVisible)
                _areasWhichBecameInvisible.Add(area);

            if (isVisible)
                _visibleAreas.Add(area);
            else
                _visibleAreas.Remove(area);
        }

        void UpdateAreas()
        {
            if (null == _worldSystem)
                return;

            this.FocusPointManager.Update();
            _worldSystem.Update();

            for (int i = 0; i < _areasWhichBecameInvisible.Count; i++)
            {
                var area = _areasWhichBecameInvisible[i];

                // area no longer visible by any player
                // destroy all NPCs from this area

                this.DestroySpawnedPedsFromArea(area);
            }

            _areasWhichBecameInvisible.Clear();

            // check if we should spawn new peds

            _timeUntilNewSpawn -= Time.deltaTime;
            if (_timeUntilNewSpawn <= 0)
            {
                _timeUntilNewSpawn = this.timeIntervalToAttemptSpawn;

                _spawnedPeds.RemoveAll(_ => null == _.pedAI);

                if (!this.IsOverLimit())
                {
                    // we should spawn new ped

                    // get area where to spawn
                    var area = this.GetAreaWhereToSpawnPed();

                    if (area != null)
                        this.StartCoroutine(this.SpawnPedCoroutine(area));
                }
            }
        }

        private WorldSystemArea GetAreaWhereToSpawnPed()
        {
            // we need to choose random visible area, because otherwise it will always try
            // to spawn in the same area (assuming focus points don't change position)

            if (_visibleAreas.Count == 0)
                return null;

            // maybe prefer areas which have less spawned peds inside ?
            // but how to find those areas efficiently ?

            return _visibleAreas.RandomElement();

            /*for (int i = 0; i < _visibleAreas.Count; i++)
            {
                var area = _visibleAreas[i];
            }*/
        }

        public bool IsOverLimit()
        {
            int numPlayers = Player.AllPlayersList.Count;

            return _spawnedPeds.Count >= this.totalMaxNumSpawnedPeds || _spawnedPeds.Count >= numPlayers * this.maxNumSpawnedPedsPerPlayer;
        }

        private void DestroySpawnedPedsFromArea(WorldSystemArea area)
        {
            if (area.ObjectsInside != null && area.ObjectsInside.Count > 0)
            {
                area.ObjectsInside.ForEach(pedAI =>
                {
                    if (pedAI != null)
                        UnityEngine.Object.Destroy(pedAI.MyPed.gameObject);
                });
                _worldSystem.RemoveAllObjectsFromArea(area);
            }
        }

        private void Update()
        {
            if (NetStatus.IsServer)
            {
                this.UpdateAreas();
            }
        }

        private System.Collections.IEnumerator SpawnPedCoroutine(WorldSystemArea worldSystemArea)
        {
            Vector3 worldSystemAreaCenter = _worldSystem.GetAreaCenter(worldSystemArea);
            Vector3 targetZone = worldSystemAreaCenter;
            float areaRadius = _areaSize * Mathf.Sqrt(2) / 2f; // radius of outer circle
            bool hasFocusPointsThatSeeArea = worldSystemArea.FocusPointsThatSeeMe != null && worldSystemArea.FocusPointsThatSeeMe.Count > 0;

            int currentArea = NodeFile.GetAreaFromPosition(targetZone);
            List<int> areaIdsToSearch = NodeFile.GetAreaNeighborhood(currentArea);
            areaIdsToSearch.Add(currentArea);
            areaIdsToSearch.RemoveAll(_ => _ < 0);
            areaIdsToSearch = areaIdsToSearch.Distinct().ToList(); // just in case above functions don't work properly

            if (areaIdsToSearch.Count == 0)
                yield break;

            // choose random node among all nodes that satisfy conditions

            float randomValue = Random.Range(0f, 15f);

            var pathNode = areaIdsToSearch
                .Select(NodeReader.GetAreaById)
                .SelectMany(_ => _.PathNodes
                    .Where(pn => pn.NodeType > 2 // ?
                                 && pn.Flags.SpawnProbability != 0
                                 && Vector3.Distance(pn.Position, targetZone) < areaRadius
                                 && (!hasFocusPointsThatSeeArea || worldSystemArea.FocusPointsThatSeeMe.All(f => Vector3.Distance(pn.Position, f.Position) > this.minSpawnDistanceFromFocusPoint))))
                .RandomElementOrDefault();

            if (EqualityComparer<PathNode>.Default.Equals(pathNode, default))
                yield break;

            /*PathNode pathNode = default;
            bool foundPathNode = false;

            foreach (NodeFile file in areaIdsToSearch.Select(NodeReader.GetAreaById))
            {
                if (foundPathNode)
                    break;

                foreach (PathNode node in file.PathNodes
                    .Where(pn => pn.NodeType > 2 && Vector3.Distance(pn.Position, targetZone) < areaRadius))
                {
                    pathNode = node;
                    foundPathNode = true;
                    break;
                }
            }

            if (!foundPathNode)
                yield break;
                */

            var newPed = this.SpawnPed(worldSystemArea, pathNode);

            // TODO: initialize PedDefinition right after ped is created, so we don't have to wait 1 frame until it is available
            yield return null;
            yield return null;

            this.AddWeaponToPed(newPed.MyPed);
        }

        private PedAI SpawnPed(WorldSystemArea worldSystemArea, PathNode pathNode)
        {
            Vector3 spawnPos = new Vector3(pathNode.Position.x, pathNode.Position.y, pathNode.Position.z);

            Ped newPed = Ped.SpawnPed(Ped.RandomPedId, spawnPos + new Vector3(0, 1, 0), Quaternion.identity, true);

            var ai = newPed.gameObject.GetOrAddComponent<PedAI>();

            _spawnedPeds.Add((ai, worldSystemArea));
            _worldSystem.AddObjectToArea(worldSystemArea, ai);

            ai.CurrentNode = pathNode;
            ai.TargetNode = pathNode;

            return ai;
        }

        private void AddWeaponToPed(Ped ped)
        {
            if (null == ped.PedDef)
                return;

            Weapon weapon = null;

            var defaultType = ped.PedDef.DefaultType;

            if (defaultType == PedestrianType.Cop
                || defaultType == PedestrianType.Criminal)
                weapon = ped.WeaponHolder.AddWeapon(WeaponId.Pistol);
            else if (defaultType.IsGangMember())
                weapon = ped.WeaponHolder.AddWeapon(WeaponId.MicroUzi);

            if (weapon != null)
            {
                ped.WeaponHolder.SwitchWeapon(weapon.SlotIndex);
                weapon.AddRandomAmmoAmount();
            }
        }
    }
}