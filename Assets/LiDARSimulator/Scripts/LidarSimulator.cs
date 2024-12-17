using UnityEngine;

public class LidarSimulator : MonoBehaviour
{
    [field: SerializeField] public LidarSensorManager SensorManager { get; } = new LidarSensorManager();
}
