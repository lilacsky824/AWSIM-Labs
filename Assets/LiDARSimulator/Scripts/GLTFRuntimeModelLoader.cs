using System.Threading.Tasks;
using GLTFast;
using UnityEngine;

namespace LiDARSimulator
{
    public class GLTFRuntimeModelLoader : IRuntimeModelLoader
    {
        public async Task<GameObject> LoadModel(byte[] rawData)
        {
            var gltf = new GltfImport();
            bool success = await gltf.LoadGltfBinary(rawData);

            GameObject loadedModelRoot = new GameObject("Loaded Model Root");
            if (success)
            {
                success = await gltf.InstantiateMainSceneAsync(loadedModelRoot.transform);
            }
            else
            {
                Debug.LogError("Loading glTF model failed!");
                return null;
            }

            if (success)
            {
                return loadedModelRoot;
            }

            Debug.LogError("Instantiate glTF model failed!");
            return null;
        }
    }
}