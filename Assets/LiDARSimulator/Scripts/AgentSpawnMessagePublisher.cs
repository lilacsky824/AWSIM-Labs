using System;
using AWSIM;
using geometry_msgs.msg;
using UnityEngine;

namespace LiDARSimulator
{
    /// <summary>
    /// Spawn Agent by sending a ROS2 Message.
    /// </summary>
    [Serializable]
    public class AgentSpawnMessagePublisher
    {
        /// <summary>
        /// Same as <seealso cref="RVIZNPCSpawner.dummyPerceptionTopic"/>>
        /// </summary>
        [SerializeField] private string _spawnTopicName = "/simulation/dummy_perception_publisher/object_info";

        public void SpawnAgent(LidarSimulator.AgentType type, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation, float velocity)
        {
            tier4_simulation_msgs.msg.DummyObject spawnMsg = null;
            var publisher = SimulatorROS2Node.CreatePublisher<tier4_simulation_msgs.msg.DummyObject>(_spawnTopicName);

            switch (type)
            {
                case LidarSimulator.AgentType.Pedestrian:
                    spawnMsg = CreateSpawnPedestrianMessage(position, rotation, velocity);
                    break;
                case LidarSimulator.AgentType.Vehicle:
                    spawnMsg = CreateSpawnVehicleMessage(position, rotation, velocity);
                    break;
                default:
                    spawnMsg = null;
                    break;
            }

            publisher.Publish(spawnMsg);
        }

        private tier4_simulation_msgs.msg.DummyObject CreateSpawnMessage(UnityEngine.Vector3 worldPosition, UnityEngine.Quaternion worldRotation,
            float velocity)
        {
            var spawnMsg = new tier4_simulation_msgs.msg.DummyObject();

            spawnMsg.Action = 0;

            Point position = new Point();
            position.X = worldPosition.x;
            position.Y = worldPosition.y;
            position.Z = worldPosition.z;
            geometry_msgs.msg.Quaternion rotation = new geometry_msgs.msg.Quaternion();
            rotation.X = worldRotation.x;
            rotation.Y = worldRotation.y;
            rotation.Z = worldRotation.z;
            rotation.W = worldRotation.w;

            spawnMsg.Initial_state.Pose_covariance.Pose.Position = position;
            spawnMsg.Initial_state.Pose_covariance.Pose.Orientation = rotation;
            spawnMsg.Initial_state.Twist_covariance.Twist.Linear.X = velocity;

            return spawnMsg;
        }

        private tier4_simulation_msgs.msg.DummyObject CreateSpawnVehicleMessage(UnityEngine.Vector3 worldPosition,
            UnityEngine.Quaternion worldRotation, float velocity)
        {
            var spawnMsg = CreateSpawnMessage(worldPosition, worldRotation, velocity);
            spawnMsg.Classification.Label = 3;
            return spawnMsg;
        }

        private tier4_simulation_msgs.msg.DummyObject CreateSpawnPedestrianMessage(UnityEngine.Vector3 worldPosition,
            UnityEngine.Quaternion worldRotation, float velocity)
        {
            var spawnMsg = CreateSpawnMessage(worldPosition, worldRotation, velocity);
            spawnMsg.Classification.Label = 7;
            return spawnMsg;
        }
    }
}