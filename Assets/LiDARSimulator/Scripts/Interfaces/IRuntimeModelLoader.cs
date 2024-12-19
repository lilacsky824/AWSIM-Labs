using System.Threading.Tasks;
using UnityEngine;

namespace LiDARSimulator
{
    public interface IRuntimeModelLoader
    {
        public Task<GameObject> LoadModel(byte[] rawData);
    }
}