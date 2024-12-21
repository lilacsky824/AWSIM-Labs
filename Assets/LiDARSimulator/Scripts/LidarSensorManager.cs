using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using AWSIM;
using RGLUnityPlugin;

namespace LiDARSimulator
{
    /// <summary>
    /// Manage multiple lidar sensors with object pool, can change amount dynamically.
    /// </summary>
    public class LidarSensorManager : MonoBehaviour
    {
        [SerializeField] private LidarSensor _sensorPrefab;
        [SerializeField, Range(0, 1)] private float _activeSensorRatio = 0.2f;
        [SerializeField] private int _maxSensorLimit = 10;

        ///TODO: Not sure how to determine the position of the LiDAR sensor, so for now, it will be placed manually.
        [SerializeField] private Transform[] _targetTransforms;

        [SerializeField] private string _lidarTopicId = "lidar/pointcloud_sim";
        [SerializeField] private string _lidarFrameId = "lidar";

        private ObjectPool<LidarSensor> _sensorPool;
        private readonly HashSet<LidarSensor> _activeSensors = new();
        private Queue<Transform> _candidateTransforms = new();

        private LidarPublisherWorldTransformBroadcaster _transformBroadcaster;

        public int ActiveSensorCount
        {
            get => _activeSensors.Count;
            set => AdjustSensorCount(value, out _);
        }

        public float ActiveSensorRatio
        {
            get
            {
                _activeSensorRatio = ActiveSensorCount / (float)_maxSensorLimit;
                return _activeSensorRatio;
            }
            set
            {
                _activeSensorRatio = value;
                ActiveSensorCount = Mathf.RoundToInt(_maxSensorLimit * _activeSensorRatio);
            }
        }

        public LidarSensor GetSensor()
        {
            AdjustSensorCount(ActiveSensorCount + 1, out LidarSensor sensor);
            return sensor;
        }

        public void ReturnSensor(LidarSensor sensor)
        {
            OnReleaseToPool(sensor);
        }

        private void Awake()
        {
            _sensorPool = new ObjectPool<LidarSensor>(
                createFunc: CreateSensor,
                actionOnGet: OnGetFromPool,
                actionOnRelease: OnReleaseToPool,
                actionOnDestroy: OnDestroyPoolObject,
                maxSize: _maxSensorLimit
            );

            _candidateTransforms = new Queue<Transform>(_targetTransforms);

            ActiveSensorRatio = _activeSensorRatio;

            _transformBroadcaster = new();
        }

        void Update()
        {
            //TODO: remove GetComponent frequently.
            _transformBroadcaster.LidarPublishers =
                _activeSensors.Select(sensor => sensor.GetComponent<RglLidarPublisher>()).ToArray();
            _transformBroadcaster.PublishTransformMessage();
        }

        private LidarSensor CreateSensor()
        {
            LidarSensor newSensor = Instantiate(_sensorPrefab);
            RglLidarPublisher publisher = newSensor.GetComponent<RglLidarPublisher>();
            newSensor.gameObject.SetActive(false);
            // Assign a unique id to prevent topic id conflict.
            publisher.pointCloud2Publishers[0].topic = _lidarTopicId + "_" + (uint)newSensor.GetInstanceID();
            publisher.frameId = _lidarFrameId + "_" + (uint)newSensor.GetInstanceID();
            return newSensor;
        }

        private void OnGetFromPool(LidarSensor sensor)
        {
            sensor.gameObject.SetActive(true);
            _activeSensors.Add(sensor);
        }

        private void OnReleaseToPool(LidarSensor sensor)
        {
            sensor.gameObject.SetActive(false);
            _activeSensors.Remove(sensor);
            _candidateTransforms.Enqueue(sensor.transform.parent);
        }

        private void OnDestroyPoolObject(LidarSensor sensor)
        {
            Destroy(sensor.gameObject);
        }

        public void PlaceLidarSensorToTransform(LidarSensor sensor, Transform parentTransform)
        {
            sensor.transform.SetParent(parentTransform);
            sensor.transform.localPosition = Vector3.zero;
        }

        private void AdjustSensorCount(int targetCount, out LidarSensor sensor)
        {
            targetCount = Mathf.Clamp(targetCount, 0, _maxSensorLimit);

            while (_activeSensors.Count < targetCount)
            {
                sensor = _sensorPool.Get();
                PlaceLidarSensorToTransform(sensor, _candidateTransforms.Dequeue());
            }

            while (_activeSensors.Count > targetCount)
            {
                LidarSensor sensorToRelease = _activeSensors.First();
                _sensorPool.Release(sensorToRelease);
                _activeSensors.Remove(sensorToRelease);
                sensor = null;
            }

            sensor = null;
        }

        public void ReleaseAllSensors()
        {
            var sensorsToRelease = new List<LidarSensor>(_activeSensors);
            foreach (var sensor in sensorsToRelease)
            {
                _sensorPool.Release(sensor);
            }

            _activeSensors.Clear();
        }

        private void OnDestroy()
        {
            _sensorPool.Dispose();
            _activeSensors.Clear();
            _candidateTransforms.Clear();
        }
    }
}