using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace LiDARSimulator
{
    /// <summary>
    /// A sample script to load model in the runtime.
    /// TODO: How to assign traffic lanes and intersections? May require add some GUI.
    /// TODO: How to assign object's material and layer?
    /// </summary>
    public class SampleModelLoader: MonoBehaviour
    {
        [SerializeField]
        private string _modelFilePath;
        [SerializeField]
        private Transform _modelRoot;
        private GLTFRuntimeModelLoader _loader = new ();

        private void OnEnable() => LoadSampleModelAsync();

        async Task LoadSampleModelAsync()
        {
            byte[] rawModelData = await LoadFileAsByteArray(_modelFilePath);
            if (rawModelData != null)
            {
                Debug.Log("File loaded successfully. Length: " + rawModelData.Length);
            }
            else
            {
                Debug.LogError("Failed to load file.");
            }

            GameObject loadedModel = await _loader.LoadModel(rawModelData);
            loadedModel.transform.SetParent(_modelRoot, false);
            Debug.Log("Successfully initiated loading model.");
        }
        
        async Task<byte[]> LoadFileAsByteArray(string fileName)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

            if (File.Exists(filePath))
            {
                Debug.Log("Start loading sample model as binary data: " + filePath);
                return await File.ReadAllBytesAsync(filePath);
            }
            else
            {
                Debug.LogError("File does not exist: " + filePath);
                return null;
            }
        }
    }
}