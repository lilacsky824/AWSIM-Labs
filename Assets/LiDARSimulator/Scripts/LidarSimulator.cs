using AWSIM;
using UnityEngine;
using RGLUnityPlugin;
using UnityEngine.Serialization;

namespace LiDARSimulator
{
    public class LidarSimulator : MonoBehaviour
    {
        public enum AgentType
        {
            Pedestrian,
            Vehicle
        }
        
        public LidarSensorManager SensorManager => _sensorManager;
        public AgentSpawnMessagePublisher AgentSpawnMessagePublisher => _agentSpawnMessagePublisher;

        [SerializeField] private LidarSensorManager _sensorManager;
        [SerializeField] private AgentSpawnMessagePublisher _agentSpawnMessagePublisher;

        void Awake()
        {
            // A temporary workaround to prevent trigger create singleton outside main thread.
            // Since some Unity API cannot be called while singleton creation.
            Debug.Log(Environment.Instance);
        }
        
        public LidarSensor GetLidarSensor()
        {
            return SensorManager.GetSensor();
        }

        public void ReleaseLidarSensor(LidarSensor sensor)
        {
            SensorManager.ReturnSensor(sensor);
        }

        public void SetLidarSensorTransform(LidarSensor sensor, Vector3 worldPosition, Vector3 worldRotation)
        {
            sensor.transform.position = worldPosition;
            sensor.transform.rotation = Quaternion.Euler(worldRotation);
        }
    }
}