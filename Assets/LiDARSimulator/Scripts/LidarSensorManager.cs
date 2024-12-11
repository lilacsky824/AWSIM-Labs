using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using RGLUnityPlugin;
using AWSIM;
using UnityEngine.Serialization;

public class LidarSensorManager : MonoBehaviour
{
    [SerializeField] private RglLidarPublisher _sensorPrefab;
    [SerializeField] private int _maxSensorLimit = 10;
    [SerializeField] private Transform _targetTransform;
    /// <summary>
    /// Only first Publisher will be set.
    /// </summary>
    [SerializeField] private string _publisherTopicName = "lidar/pointcloud_sim_";

    private ObjectPool<RglLidarPublisher> _sensorPool;
    private HashSet<RglLidarPublisher> _activeSensors = new HashSet<RglLidarPublisher>();
    private Queue<Transform> _candidateTransforms = new Queue<Transform>();

    public int ActiveSensorCount
    {
        get => _activeSensors.Count;
        set 
        {
            AdjustSensorCount(value);
        }
    }

    private void Awake()
    {
        _sensorPool = new ObjectPool<RglLidarPublisher>(
            createFunc: CreateSensor,
            actionOnGet: OnGetFromPool,
            actionOnRelease: OnReleaseToPool,
            actionOnDestroy: OnDestroyPoolObject,
            maxSize: _maxSensorLimit
        );
        
        _candidateTransforms = TransformUtility.FindTransformsByName(_targetTransform.name).ToQueue();
        
        ActiveSensorCount = _maxSensorLimit;
    }

    private RglLidarPublisher CreateSensor()
    {
        RglLidarPublisher newSensor = Instantiate(_sensorPrefab);
        newSensor.gameObject.SetActive(false);
        return newSensor;
    }

    private void OnGetFromPool(RglLidarPublisher sensor)
    {
        Transform targetTransform = _candidateTransforms.Dequeue();
        
        if (targetTransform != null)
        {
            sensor.transform.SetParent(targetTransform);
            sensor.transform.localPosition = Vector3.zero;
        }

        sensor.gameObject.SetActive(true);
        _activeSensors.Add(sensor);
        
        sensor.pointCloud2Publishers[0].topic = _publisherTopicName + (uint)sensor.GetInstanceID();
    }

    private void OnReleaseToPool(RglLidarPublisher sensor)
    {
        sensor.gameObject.SetActive(false);
        _activeSensors.Remove(sensor);
        _candidateTransforms.Enqueue(sensor.transform.parent);
    }

    private void OnDestroyPoolObject(RglLidarPublisher sensor)
    {
        Destroy(sensor.gameObject);
    }

    private void AdjustSensorCount(int targetCount)
    {
        targetCount = Mathf.Clamp(targetCount, 0, _maxSensorLimit);

        while (_activeSensors.Count < targetCount)
        {
            _sensorPool.Get();
        }

        while (_activeSensors.Count > targetCount)
        {
            RglLidarPublisher sensorToRelease = _activeSensors.Single();
            _sensorPool.Release(sensorToRelease);
            _activeSensors.Remove(sensorToRelease);
        }
    }

    public void ReleaseAllSensors()
    {
        var sensorsToRelease = new List<RglLidarPublisher>(_activeSensors);
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