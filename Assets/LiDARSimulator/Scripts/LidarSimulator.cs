using UnityEngine;
using RGLUnityPlugin;

namespace LiDARSimulator
{
    public class LidarSimulator : MonoBehaviour
    {
        public enum AgentType
        {
            Pedestrian,
            Vehicle
        }

        [field: SerializeField] public LidarSensorManager SensorManager { get; } = new LidarSensorManager();
        [field: SerializeField] public AgentSpawnMessagePublisher _agentSpawnMessagePublisher = new AgentSpawnMessagePublisher();

        public LidarSensor GetLidarSensor()
        {
            return SensorManager.GetSensor();
        }

        public void ReleaseLidarSensor(LidarSensor sensor)
        {
            SensorManager.ReturnSensor(sensor);
        }
    }
}