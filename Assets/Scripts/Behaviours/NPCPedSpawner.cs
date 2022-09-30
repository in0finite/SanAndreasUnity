using SanAndreasUnity.Importing.Paths;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Net;
using UGameCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Behaviours.Peds.AI;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Behaviours.WorldSystem;
using UnityEngine;
using Random = UnityEngine.Random;
using WorldSystemArea = SanAndreasUnity.Behaviours.WorldSystem.WorldSystem<SanAndreasUnity.Behaviours.Peds.AI.PedAI>.Area;

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
        public float maxSpawnDistanceFromFocusPoint = 100;

        private readonly List<WorldSystemArea> _visibleAreas = new List<WorldSystemArea>(32);

        private readonly List<(PedAI pedAI, WorldSystemArea area)> _spawnedPeds = new List<(PedAI, WorldSystemArea)>();
        private readonly List<PedAI> _pedsToDestroy = new List<PedAI>();

        private float _timeUntilNewSpawn = 0f;
        public float timeIntervalToAttemptSpawn = 0.33f;

        private float _timeUntilNewDestroy = 0f;
        public float timeIntervalToAttemptDestroy = 0.65f;

        public FocusPointManager<PedAI> FocusPointManager { get; private set; }


        protected override void OnSingletonAwake()
        {
            _timeUntilNewSpawn = this.timeIntervalToAttemptSpawn;
            _timeUntilNewDestroy = this.timeIntervalToAttemptDestroy;

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
            if (isVisible)
                _visibleAreas.Add(area);
            else
                _visibleAreas.Remove(area);
        }

        void OnPedChangedArea(PedAI pedAI, AreaIndex oldAreaIndex, AreaIndex newAreaIndex)
        {
            var newArea = _worldSystem.GetAreaAt(newAreaIndex);
            if (null == newArea || !newArea.WasVisibleInLastUpdate) // new area not visible
                _pedsToDestroy.AddIfNotPresent(pedAI);
        }

        void UpdateAreas()
        {
            if (null == _worldSystem)
                return;

            this.FocusPointManager.Update();
            _worldSystem.Update();

            this.UpdateDestroying();

            this.UpdateSpawning();
        }

        void UpdateDestroying()
        {
            _timeUntilNewDestroy -= Time.deltaTime;
            if (_timeUntilNewDestroy > 0)
                return;

            _timeUntilNewDestroy = this.timeIntervalToAttemptDestroy;

            // destroy only 1 ped per attempt

            // check if there are registered peds to destroy

            _pedsToDestroy.RemoveDeadObjects();
            if (_pedsToDestroy.Count > 0)
            {
                Destroy(_pedsToDestroy[0].MyPed.gameObject);
                _pedsToDestroy.RemoveAt(0);

                return;
            }

            // check if any of spawned peds is in invisible area - can happen if area became invisible

            _spawnedPeds.RemoveAll(_ => null == _.pedAI);

            int index = _spawnedPeds.FindIndex(_ =>
            {
                var area = _worldSystem.GetAreaAt(_.pedAI.MyPed.transform.position);
                return null == area || !area.WasVisibleInLastUpdate;
            });

            if (index >= 0)
            {
                Destroy(_spawnedPeds[index].pedAI.MyPed.gameObject);
                _spawnedPeds.RemoveAt(index);
            }
        }

        void UpdateSpawning()
        {
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

            List<NodeFile> areasToSearch = NodeReader.GetAreasInRadius(targetZone, areaRadius)
                .ToList();

            if (areasToSearch.Count == 0)
                yield break;

            // choose random node among all nodes that satisfy conditions

            float randomValue = Random.Range(0f, 15f);

            var pathNode = areasToSearch
                .SelectMany(_ => _.PedNodes
                    .Where(pn => pn.ShouldPedBeSpawnedHere
                                 && pn.Flags.SpawnProbability != 0
                                 && Vector3.Distance(pn.Position, targetZone) < areaRadius
                                 && (!hasFocusPointsThatSeeArea || worldSystemArea.FocusPointsThatSeeMe.All(f => Vector3.Distance(pn.Position, f.Position).BetweenExclusive(this.minSpawnDistanceFromFocusPoint, this.maxSpawnDistanceFromFocusPoint)))))
                .RandomElementOrDefault();

            if (EqualityComparer<PathNode>.Default.Equals(pathNode, default))
                yield break;

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

            ai.StartWalkingAround(pathNode);

            var areaChangeDetector = newPed.gameObject.AddComponent<AreaChangeDetector>();
            areaChangeDetector.Init(_worldSystem);
            areaChangeDetector.onAreaChanged += (oldIndex, newIndex) => OnPedChangedArea(ai, oldIndex, newIndex);

            return ai;
        }

        public void AddWeaponToPed(Ped ped)
        {
            if (null == ped.PedDef)
                return;

            Weapon weapon = null;

            var defaultType = ped.PedDef.DefaultType;

            if (defaultType.IsCop())
                weapon = ped.WeaponHolder.AddWeapon(WeaponId.Pistol);
            else if (defaultType.IsCriminal())
                weapon = ped.WeaponHolder.AddWeapon(new int[]{WeaponId.Pistol, WeaponId.DesertEagle}.RandomElement());
            else if (defaultType.IsGangMember())
                weapon = ped.WeaponHolder.AddWeapon(new int[]{WeaponId.MicroUzi, WeaponId.Tec9}.RandomElement());

            if (weapon != null)
            {
                ped.WeaponHolder.SwitchWeapon(weapon.SlotIndex);
                weapon.AddRandomAmmoAmount();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (null == Ped.Instance)
                return;

            float radius = 200f;
            Vector3 center = Ped.Instance.transform.position;

            var pathNodes = NodeReader.GetAreasInRadius(center, radius)
                .SelectMany(_ => _.PedNodes)
                .Where(pn => pn.ShouldPedBeSpawnedHere
                             && pn.Flags.SpawnProbability != 0
                             && Vector3.Distance(pn.Position, center) < radius);

            Gizmos.color = Color.yellow;
            foreach (PathNode pathNode in pathNodes)
            {
                Gizmos.DrawWireSphere(pathNode.Position, pathNode.PathWidth / 2f);
                foreach (PathNode linkedNode in NodeReader.GetAllLinkedNodes(pathNode))
                    Gizmos.DrawLine(pathNode.Position, linkedNode.Position);
            }
        }
    }
}