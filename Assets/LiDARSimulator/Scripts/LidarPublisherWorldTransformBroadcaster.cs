using AWSIM;
using geometry_msgs.msg;
using ROS2;
using tf2_msgs.msg;

namespace LiDARSimulator
{
    public class LidarPublisherWorldTransformBroadcaster
    {
        public RglLidarPublisher[] LidarPublishers { get; set; }

        private Publisher<TFMessage> _publisher;
        private TFMessage _transformMessage = new();
        private const string _worldFrameId = "world";

        public LidarPublisherWorldTransformBroadcaster()
        {
            QualityOfServiceProfile qualityOfServiceProfile = new QualityOfServiceProfile();
            qualityOfServiceProfile.SetDurability(DurabilityPolicy.QOS_POLICY_DURABILITY_VOLATILE);
            qualityOfServiceProfile.SetHistory(HistoryPolicy.QOS_POLICY_HISTORY_KEEP_LAST, 5);
            qualityOfServiceProfile.SetReliability(ReliabilityPolicy.QOS_POLICY_RELIABILITY_RELIABLE);
            _publisher = SimulatorROS2Node.CreatePublisher<TFMessage>("/tf", qualityOfServiceProfile);
        }

        public void PublishTransformMessage()
        {
            _publisher.Publish(GetWorldTransformMessage());
        }

        public TFMessage GetWorldTransformMessage()
        {
            if (LidarPublishers.Length != _transformMessage.Transforms.Length)
            {
                _transformMessage.Transforms = new TransformStamped[LidarPublishers.Length];
            }

            var stamp = SimulatorROS2Node.GetCurrentRosTime();
            for (int i = 0; i < LidarPublishers.Length; i++)
            {
                _transformMessage.Transforms[i] = GetWorldTransformStampedMessage(LidarPublishers[i]);
                _transformMessage.Transforms[i].Header.Stamp = stamp;
            }

            return _transformMessage;
        }

        public TransformStamped GetWorldTransformStampedMessage(RglLidarPublisher node)
        {
            // TODO: should not recreate instance each time, can just override value.
            TransformStamped transform = new TransformStamped();
            transform.SetHeaderFrame(_worldFrameId);
            transform.Child_frame_id = node.frameId;

            UnityEngine.Vector3 worldPosition = ROS2Utility.UnityToRosPosition(node.transform.position);
            transform.Transform.Translation.X = worldPosition.x;
            transform.Transform.Translation.Y = worldPosition.y;
            transform.Transform.Translation.Z = worldPosition.z;

            UnityEngine.Quaternion worldRotation = ROS2Utility.UnityToRosRotation(node.transform.rotation);
            transform.Transform.Rotation = new Quaternion();
            transform.Transform.Rotation.X = worldRotation.x;
            transform.Transform.Rotation.Y = worldRotation.y;
            transform.Transform.Rotation.Z = worldRotation.z;
            transform.Transform.Rotation.W = worldRotation.w;

            return transform;
        }
    }
}