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

        [field: SerializeField] public LidarSensorManager SensorManager { get; } = new();
        [field: SerializeField] public AgentSpawnMessagePublisher AgentSpawnMessagePublisher = new();

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