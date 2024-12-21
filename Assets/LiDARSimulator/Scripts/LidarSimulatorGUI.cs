using System;
using UnityEngine;
using VehiclePhysics;

namespace LiDARSimulator
{
    public class LidarSimulatorGUI : MonoBehaviour
    {
        [SerializeField] private LidarSimulator _lidarSimulator;
        [SerializeField] private VPCameraController _cameraController;
        [SerializeField, Range(0, 2)] private float _targetSize = 20;

        private Camera _mainCamera;
        private GUIStyle _guiStyle;
        private bool _isHit;
        private Vector3 _lastHitWorldPosition;
        private Quaternion _lastWorldRotation;

        private void Awake()
        {
            _mainCamera = Camera.main;

            if (_mainCamera == null)
            {
                Debug.LogWarning("Main Camera is not assigned.");
            }

            _guiStyle = new GUIStyle();
            Texture2D backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.33f));
            backgroundTexture.Apply();
            _guiStyle.normal.background = backgroundTexture;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _cameraController.enabled = !_cameraController.enabled;
            }

            _isHit = GetRaycastTargetFromCameraForward();
            _lastWorldRotation = Quaternion.AngleAxis(_mainCamera.transform.rotation.eulerAngles.y, Vector3.up);
        }

        void OnGUI()
        {
            GUILayout.BeginVertical(_guiStyle);

            GUILayout.Label("WSAD to move, Q to ascend, E to descend");
            GUILayout.Label("Esc to toggle camera movement");

            GUILayout.Space(8);

            GUILayout.BeginVertical(_guiStyle);
            GUILayout.Label("Lidar Sensor Management");
            string label = _lidarSimulator.SensorManager.ActiveSensorRatio >= 1
                ? "Reach Maximum"
                : _lidarSimulator.SensorManager.ActiveSensorRatio.ToString();
            GUILayout.Label($"Lidar Sensor Amount {_lidarSimulator.SensorManager.ActiveSensorCount} {label}");
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
            GUILayout.EndVertical();

            GUILayout.Space(8);

            GUILayout.BeginVertical(_guiStyle);
            GUILayout.Label("Spawn Agent in front of view");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Pedestrian") && _isHit)
            {
                SpawnAgentInFrontOfView(LidarSimulator.AgentType.Pedestrian);
            }

            if (GUILayout.Button("Vehicle") && _isHit)
            {
                SpawnAgentInFrontOfView(LidarSimulator.AgentType.Vehicle);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(8);

            label = _lidarSimulator.TrafficManager.targetVehicleCount >= _lidarSimulator.TrafficManager.maxVehicleCount
                ? "Reach Maximum"
                : string.Empty;
            GUILayout.BeginVertical(_guiStyle);
            GUILayout.Label($"Traffic Vehicle Amount {_lidarSimulator.TrafficManager.transform.childCount} {label}");
            GUILayout.Label("Set Vehicle Amount");
            GUILayout.BeginHorizontal();
            DrawSetVehicleAmountButton(10);
            DrawSetVehicleAmountButton(20);
            DrawSetVehicleAmountButton(50);
            DrawSetVehicleAmountButton(100);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.EndVertical();
        }

        bool GetRaycastTargetFromCameraForward()
        {
            Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Vector3 hitPoint = hitInfo.point;
                _lastHitWorldPosition = hitPoint;
                return true;
            }

            //_lastHitWorldPosition = Vector3.zero;
            return false;
        }

        void SpawnAgentInFrontOfView(LidarSimulator.AgentType agentType)
        {
            _lidarSimulator.AgentSpawnMessagePublisher.SpawnAgent(agentType, _lastHitWorldPosition, _lastWorldRotation,
                2);
            Debug.Log($"Spawn agent type {agentType} at {_lastHitWorldPosition} {_lastWorldRotation.eulerAngles}");
        }

        void SpawnTrafficVehicleInFrontOfView(Vector3 hitWorldPosition)
        {
            bool success = _lidarSimulator.VehicleSpawner.SpawnVehicle(hitWorldPosition);
            string result = success ? "Success" : "Failed";
            Debug.Log($"Spawn agent type TrafficVehicle at {hitWorldPosition} {result}");
        }

        void DrawSetVehicleAmountButton(int amount)
        {
            if (GUILayout.Button(amount.ToString()))
            {
                SetVehicleAmount(amount);
            }
        }

        void SetVehicleAmount(int amount)
        {
            _lidarSimulator.TrafficManager.targetVehicleCount = amount;
            _lidarSimulator.TrafficManager.RestartTraffic();
        }

        public void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            Vector3 origin = _mainCamera.transform.position;
            if (_isHit)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(origin, _lastHitWorldPosition);

                Gizmos.DrawSphere(_lastHitWorldPosition, _targetSize);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(origin, origin + _mainCamera.transform.forward * Single.MaxValue);
            }
        }
    }
}