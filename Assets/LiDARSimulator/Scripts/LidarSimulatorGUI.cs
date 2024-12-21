using System;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using VehiclePhysics;

namespace LiDARSimulator
{
    public class LidarSimulatorGUI : MonoBehaviour
    {
        [SerializeField] private LidarSimulator _lidarSimulator;
        [SerializeField] private VPCameraController _cameraController;
        [SerializeField, Range(0, 256)] private uint _targetSize = 20;

        private Camera _mainCamera;
        private GUIStyle _guiStyle;

        private void Awake()
        {
            _mainCamera = Camera.main;

            if (_mainCamera == null)
            {
                Debug.LogWarning("Main Camera is not assigned.");
            }

            _guiStyle = new GUIStyle();
            _guiStyle.normal.background = Texture2D.grayTexture;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _cameraController.enabled = !_cameraController.enabled;
            }
        }

        void OnGUI()
        {
            GUILayout.BeginVertical(_guiStyle);

            GUILayout.Label("WSAD to move, Q to ascend, E to descend");
            GUILayout.Label("Esc to toggle camera movement");

            GUILayout.Label("Lidar Sensor Management");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Lidar Sensor"))
            {
                _lidarSimulator.SensorManager.ActiveSensorCount++;
            }

            if (GUILayout.Button("Remove a Lidar Sensor"))
            {
                _lidarSimulator.SensorManager.ActiveSensorCount--;
            }

            GUILayout.EndHorizontal();

            bool isHit = GetRaycastTargetFromCameraForward(out Vector3 hitWorldPosition);
            Quaternion rotation = Quaternion.AngleAxis(_mainCamera.transform.rotation.eulerAngles.y, Vector3.up);
            GUILayout.Label("Spawn Agent in front of view");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Pedestrian") && isHit)
            {
                SpawnAgentInFrontOfView(LidarSimulator.AgentType.Pedestrian, hitWorldPosition, rotation);
            }

            if (GUILayout.Button("Vehicle") && isHit)
            {
                SpawnAgentInFrontOfView(LidarSimulator.AgentType.Vehicle, hitWorldPosition, rotation);
            }

            if (GUILayout.Button("Traffic Vehicle") && isHit)
            {
                SpawnTrafficVehicleInFrontOfView(hitWorldPosition);
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        bool GetRaycastTargetFromCameraForward(out Vector3 hitWorldPosition)
        {
            Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Vector3 hitPoint = hitInfo.point;

                Handles.color = Color.green;
                Handles.DrawLine(ray.origin, hitPoint);

                float handleWorldSize =
                    HandleUtility.GetHandleSize(hitPoint) * ((float)_targetSize / Screen.height);

                Handles.SphereHandleCap(0, hitPoint, Quaternion.identity, handleWorldSize, EventType.Repaint);

                hitWorldPosition = hitPoint;
                return true;
            }
            else
            {
                Handles.color = Color.red;
                Handles.DrawLine(ray.origin, ray.origin + _mainCamera.transform.forward * Single.MaxValue);
            }

            hitWorldPosition = Vector3.zero;
            return false;
        }

        void SpawnAgentInFrontOfView(LidarSimulator.AgentType agentType, Vector3 hitWorldPosition,
            Quaternion worldRotation)
        {
            _lidarSimulator.AgentSpawnMessagePublisher.SpawnAgent(agentType, hitWorldPosition, worldRotation, 2);
            Debug.Log($"Spawn agent type {agentType} at {hitWorldPosition} {worldRotation.eulerAngles}");
        }

        void SpawnTrafficVehicleInFrontOfView(Vector3 hitWorldPosition)
        {
            bool success = _lidarSimulator.VehicleSpawner.SpawnVehicle(hitWorldPosition);
            string result = success ? "Success" : "Failed";
            Debug.Log($"Spawn agent type TrafficVehicle at {hitWorldPosition} {result}");
        }
    }
}