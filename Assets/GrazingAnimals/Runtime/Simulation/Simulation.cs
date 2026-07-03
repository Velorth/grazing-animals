using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

namespace GrazingAnimals

{
    public class Simulation : MonoBehaviour, INeighboursProvider
    {
        private const int CellSize = 10;
        private const string SavePath = "simulation.sav";
        
        [SerializeField] private Ground _ground;
        [SerializeField] private Agent _agentPrefab;
        [SerializeField] private Food _foodPrefab;

        private int _size;
        private int _agentsCount;
        private readonly List<Agent> _agents = new();
        private NativeParallelMultiHashMap<(int, int), int> _spatialMap;
        private List<(int, int)> _positions = new();
        private bool _started;
        private bool _isPaused;
        private float _timeScale = 1f;
        private float _speed;

        public float TimeScale
        {
            get => _timeScale;
            set
            {
                _timeScale = value;
            }
        }

        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                _isPaused = value;
            }
        }
        
        public bool HasSave => File.Exists(SavePath);

        public void StartSimulation(int size, int agentsCount, float speed)
        {
            _speed = speed;
            _started = true;
            _spatialMap = new NativeParallelMultiHashMap<(int, int), int>(agentsCount, Allocator.Persistent);
            _size = size;
            _agentsCount = agentsCount;
            
            _ground.Generate(size);

            _positions.Capacity = Mathf.Max(size * size, _positions.Capacity);
            _positions.Clear();
            for (var x = 0; x < size; x++)
            for (var y = 0; y < size; y++)
            {
                _positions.Add((x, y));
            }

            var agentPositions = new List<(int, int)>(_positions);
            
            for (int i = 0; i < agentsCount; i++)
            {
                var agentPosition = GetRandomPosition(agentPositions);
                var agent = Instantiate(_agentPrefab, agentPosition, Quaternion.identity);
                agent.NeighboursProvider = this;
                agent.MaxSpeed = speed;
                _agents.Add(agent);

                var foodPosition = GetRandomPosition(_positions);
                var food = Instantiate(_foodPrefab,foodPosition, Quaternion.identity);
                food.Collected += () => OnFoodCollected(food);

                agent.Target = food;
            }

            _isPaused = false;
            _timeScale = 1f;
        }

        private void OnFoodCollected(Food food)
        {
            var foodPosition = food.transform.localPosition;
            
            _positions.Add(((int)foodPosition.x, (int)foodPosition.z));
            food.transform.position = GetRandomPosition(_positions);
        }

        public void StopSimulation()
        {
            _started = false;
            if (_spatialMap.IsCreated)
            {
                _spatialMap.Dispose();
            }

            foreach (var agent in _agents)
            {
                if (!agent)
                    continue;

                if (agent.Target)
                {
                    Destroy(agent.Target.gameObject);
                }
                
                Destroy(agent.gameObject);
            }
            
            _agents.Clear();
        }

        public bool TryLoadSimulation()
        {
            try
            {
                using var fileStream = new FileStream(SavePath, FileMode.Open);
                using var reader = new BinaryReader(fileStream);
                LoadSimulation(reader);

                return true;
            }
            catch (Exception e)
            {
                Debug.Log($"Failed to load simulation: {e.Message}");
                StopSimulation();
                return false;
            }
        }

        public void SaveSimulation()
        {
            try
            {
                using var fileStream = new FileStream(SavePath, FileMode.Create);
                using var writer = new BinaryWriter(fileStream);

                SaveSimulation(writer);
            }
            catch (Exception e)
            {
                Debug.Log($"Failed to save simulation: {e.Message}");
            }
        }

        private void SaveSimulation(BinaryWriter writer)
        {
            writer.Write(_size);
            writer.Write(_agentsCount);
            writer.Write(_speed);

            foreach (var agent in _agents)
            {
                writer.Write(agent.transform.localPosition);
                writer.Write(agent.transform.localRotation);
                writer.Write(agent.Target.transform.localPosition);
                writer.Write(agent.Target.transform.localRotation);
            }
        }

        private void LoadSimulation(BinaryReader reader)
        {
            _size = reader.ReadInt32();
            _agentsCount = reader.ReadInt32();
            _speed = reader.ReadSingle();
            
            _ground.Generate(_size);

            var positionsSet = new HashSet<(int, int)>(_size * _size);
            for (var x = 0; x < _size; x++)
            for (var y = 0; y < _size; y++)
            {
                positionsSet.Add((x, y));
            }
            
            for (int i = 0; i < _agentsCount; i++)
            {
                var agent = Instantiate(_agentPrefab, reader.ReadVector3(), reader.ReadQuaternion());
                agent.NeighboursProvider = this;
                agent.MaxSpeed = _speed;
                _agents.Add(agent);

                var foodPosition = reader.ReadVector3();
                var food = Instantiate(_foodPrefab, foodPosition, reader.ReadQuaternion());
                food.Collected += () => OnFoodCollected(food);

                agent.Target = food;

                positionsSet.Remove(((int)foodPosition.x, (int)foodPosition.z));
            }
            
            _positions.Clear();
            _positions.AddRange(positionsSet);
            _started = true;
            _spatialMap = new NativeParallelMultiHashMap<(int, int), int>(_agentsCount, Allocator.Persistent);
        }

        private void Update()
        {
            if (!_started || _isPaused)
                return;
            
            UpdateSpatialMap();
            
            var deltaTime = Time.deltaTime * _timeScale;
            foreach (var agent in _agents)
            {
                agent.Think(deltaTime);
                agent.Move(deltaTime);
            }
        }

        private void UpdateSpatialMap()
        {
            Profiler.BeginSample("UpdateSpatialMap");
            _spatialMap.Clear();
            for (var index = 0; index < _agents.Count; index++)
            {
                var agent = _agents[index];
                var position = agent.transform.localPosition;
                var key = GetSpatialMapKey(position);

                _spatialMap.Add(key, index);
            }
            Profiler.EndSample();
        }

        private static (int X, int Y) GetSpatialMapKey(Vector3 position)
        {
            return ((int)position.x / CellSize, (int)position.z / CellSize);
        }

        private void OnDisable()
        {
            if (_started)
            {
                StopSimulation();
            }
        }

        private static Vector3 GetRandomPosition(List<(int, int)> positions)
        {
            var take = Random.Range(0, positions.Count);
            var (x, z) = positions[take];
            positions.FastRemoveAt(take);
            return new  Vector3(x, 0, z);
        }

        public void CollectNeighbours(Agent actor, float radius, List<Agent> neighbours)
        {
            Profiler.BeginSample("CollectNeighbours");
            neighbours.Clear();

            var actorPosition = actor.transform.localPosition;
            var queryDistance = (radius + actor.Radius * 2);
            var expand = new Vector3(queryDistance, 0, queryDistance);
            var min = GetSpatialMapKey(actorPosition - expand);
            var max = GetSpatialMapKey(actorPosition + expand);

            var queryDistanceSq = queryDistance * queryDistance;

            for (var x = min.X; x <= max.X; x++)
            for (var y = min.Y; y <= max.Y; y++)
            {
                if (_spatialMap.TryGetFirstValue((x, y), out var index, out var iterator))
                {
                    do
                    {
                        var candidate = _agents[index];
                        var candidatePosition = candidate.transform.localPosition;

                        if (candidate != actor && queryDistanceSq >= (actorPosition - candidatePosition).sqrMagnitude)
                        {
                            neighbours.Add(_agents[index]);
                        }
                    } while (_spatialMap.TryGetNextValue(out index, ref iterator));
                }
            }

            Profiler.EndSample();
        }
    }
}
