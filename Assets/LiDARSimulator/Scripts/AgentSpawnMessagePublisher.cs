using System;
using AWSIM;
using geometry_msgs.msg;
using ROS2;
using UnityEngine;
using UnityEngine.Serialization;

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
        [SerializeField] private QoSSettings _qosSettings = new QoSSettings()
        {
            ReliabilityPolicy = ReliabilityPolicy.QOS_POLICY_RELIABILITY_RELIABLE,
            DurabilityPolicy = DurabilityPolicy.QOS_POLICY_DURABILITY_VOLATILE,
            HistoryPolicy = HistoryPolicy.QOS_POLICY_HISTORY_KEEP_LAST,
            Depth = 1,
        };

        public void SpawnAgent(LidarSimulator.AgentType type, UnityEngine.Vector3 worldPosition, UnityEngine.Quaternion worldRotation, float velocity)
        {
            tier4_simulation_msgs.msg.DummyObject spawnMsg = null;
            var publisher = SimulatorROS2Node.CreatePublisher<tier4_simulation_msgs.msg.DummyObject>(_spawnTopicName, _qosSettings.GetQoSProfile());

            switch (type)
            {
                case LidarSimulator.AgentType.Pedestrian:
                    spawnMsg = CreateSpawnPedestrianMessage(worldPosition, worldRotation, velocity);
                    break;
                case LidarSimulator.AgentType.Vehicle:
                    spawnMsg = CreateSpawnVehicleMessage(worldPosition, worldRotation, velocity);
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

            UnityEngine.Vector3 framePosition = ROS2Utility.UnityToRosPosition(worldPosition) + AWSIM.Environment.Instance.MgrsOffsetPosition;
            Point position = new Point();
            position.X = framePosition.x;
            position.Y = framePosition.y;
            position.Z = framePosition.z;
            
            UnityEngine.Quaternion frameRotation = ROS2Utility.UnityToRosRotation(worldRotation);
            geometry_msgs.msg.Quaternion rotation = new geometry_msgs.msg.Quaternion();
            rotation.X = frameRotation.x;
            rotation.Y = frameRotation.y;
            rotation.Z = frameRotation.z;
            rotation.W = frameRotation.w;

            spawnMsg.Initial_state.Pose_covariance.Pose.Position = position;
            spawnMsg.Initial_state.Pose_covariance.Pose.Orientation = rotation;
            spawnMsg.Initial_state.Twist_covariance.Twist.Linear.X = velocity;

            return spawnMsg;
        }
        
        private tier4_simulation_msgs.msg.DummyObject CreateSpawnPedestrianMessage(UnityEngine.Vector3 worldPosition,
            UnityEngine.Quaternion worldRotation, float velocity)
        {
            var spawnMsg = CreateSpawnMessage(worldPosition, worldRotation, velocity);
            spawnMsg.Classification.Label = 7;
            return spawnMsg;
        }

        private tier4_simulation_msgs.msg.DummyObject CreateSpawnVehicleMessage(UnityEngine.Vector3 worldPosition,
            UnityEngine.Quaternion worldRotation, float velocity)
        {
            var spawnMsg = CreateSpawnMessage(worldPosition, worldRotation, velocity);
            spawnMsg.Classification.Label = 4;
            return spawnMsg;
        }
    }
}